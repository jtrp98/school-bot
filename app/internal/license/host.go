package license

import (
	"bytes"
	"context"
	"crypto/rand"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"net/url"
	"strings"
	"sync"
	"time"
)

type Host struct {
	mu          sync.Mutex
	baseURL     string
	client      *http.Client
	clock       Clock
	verifier    SignatureVerifier
	store       BlobStore
	credentials *Credentials
	service     *EntitlementService
}

func NewHost(baseURL string, client *http.Client, clock Clock, verifier SignatureVerifier, store BlobStore) *Host {
	return &Host{
		baseURL:  strings.TrimRight(baseURL, "/"),
		client:   client,
		clock:    clock,
		verifier: verifier,
		store:    store,
	}
}

func (h *Host) initialize(ctx context.Context) error {
	if h.service != nil {
		return nil
	}
	credentials, err := OpenCredentials(ctx, h.store)
	if err != nil {
		return &LicenseError{
			Code:    ErrCredentialStoreFailed,
			Message: "secure credentials unavailable; connect online to recover",
		}
	}
	h.credentials = credentials
	h.service = NewEntitlementService(h.clock, h.verifier, credentials, credentials)
	return nil
}

func (h *Host) Evaluate(ctx context.Context) (EntitlementProjection, error) {
	h.mu.Lock()
	defer h.mu.Unlock()
	if err := h.initialize(ctx); err != nil {
		return EntitlementProjection{Status: StatusUnactivated}, err
	}
	return h.service.Evaluate(ctx)
}

func (h *Host) Authorize(ctx context.Context, mode string) error {
	switch mode {
	case "block-programming", "robot-control", "coding-adventure":
	default:
		return &LicenseError{Code: ErrInvalidEntitlement, Message: "unsupported mode"}
	}
	projection, err := h.Evaluate(ctx)
	if err != nil {
		return err
	}
	if projection.Status != StatusValid {
		return &LicenseError{Code: ErrDeviceNotActivated, Message: "connect online to activate or renew this device"}
	}
	return nil
}

func (h *Host) Check(ctx context.Context, key string, validate bool) (EntitlementProjection, error) {
	h.mu.Lock()
	defer h.mu.Unlock()
	key = strings.TrimSpace(key)
	if len(key) == 0 || len(key) > 256 {
		return EntitlementProjection{Status: StatusUnactivated}, &LicenseError{
			Code:    ErrLicenseInvalid,
			Message: "invalid license key",
		}
	}
	if err := h.initialize(ctx); err != nil {
		return EntitlementProjection{Status: StatusUnactivated}, err
	}
	_, _ = h.service.Evaluate(ctx)
	challenge, err := h.post(ctx, "/v2/device-challenges", map[string]interface{}{
		"device_jwk": publicJWK(h.credentials.key),
	}, "")
	if err != nil {
		return h.service.ProcessOnlineOutcome(ctx, OnlineOutcome{NetworkErr: err})
	}
	if challenge.status != 200 {
		return h.service.ProcessOnlineOutcome(ctx, OnlineOutcome{StatusCode: challenge.status, ServerCode: challenge.code})
	}
	var challengeBody struct {
		Challenge string `json:"challenge"`
	}
	if json.Unmarshal(challenge.body, &challengeBody) != nil || challengeBody.Challenge == "" {
		return EntitlementProjection{Status: h.service.CurrentStatus()}, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid challenge response",
		}
	}
	path := "/v2/licenses/activate"
	body := map[string]interface{}{"key": key, "challenge": challengeBody.Challenge}
	requestKey := ""
	if validate {
		path = "/v2/licenses/validate"
		record, _ := h.credentials.Load(ctx)
		jkt, _ := h.credentials.CurrentDeviceJKT(ctx)
		if record == nil {
			return EntitlementProjection{Status: StatusUnactivated}, &LicenseError{
				Code:    ErrDeviceNotActivated,
				Message: "connect online to activate this device",
			}
		}
		claims, err := VerifyEntitlementToken(ctx, record.Token, jkt, h.verifier)
		if err != nil {
			return EntitlementProjection{Status: h.service.CurrentStatus()}, err
		}
		body["binding_id"] = claims.BindingID
	} else {
		body["device_jwk"] = publicJWK(h.credentials.key)
		random := make([]byte, 16)
		if _, err = rand.Read(random); err != nil {
			return EntitlementProjection{Status: h.service.CurrentStatus()}, &LicenseError{
				Code:    ErrDeviceProofInvalid,
				Message: "cannot create request key",
			}
		}
		random[6] = (random[6] & 15) | 64
		random[8] = (random[8] & 63) | 128
		encoded := hex.EncodeToString(random)
		requestKey = encoded[:8] + "-" + encoded[8:12] + "-" + encoded[12:16] + "-" + encoded[16:20] + "-" + encoded[20:]
	}
	canonical, err := canonicalBody(body)
	if err != nil {
		return EntitlementProjection{Status: h.service.CurrentStatus()}, err
	}
	digest := sha256.Sum256(canonical)
	proof, err := signProof(h.credentials.key, map[string]string{
		"challenge":       challengeBody.Challenge,
		"method":          "POST",
		"path":            path,
		"idempotency_key": requestKey,
		"body_hash":       hex.EncodeToString(digest[:]),
	})
	if err != nil {
		return EntitlementProjection{Status: h.service.CurrentStatus()}, &LicenseError{
			Code:    ErrDeviceProofInvalid,
			Message: "cannot sign device proof",
		}
	}
	body["proof"] = proof
	response, err := h.post(ctx, path, body, requestKey)
	if err != nil {
		return h.service.ProcessOnlineOutcome(ctx, OnlineOutcome{NetworkErr: err})
	}
	outcome := OnlineOutcome{StatusCode: response.status, ServerCode: response.code, KeyRef: "installation"}
	if response.status == 200 {
		var result struct {
			Valid     bool      `json:"valid"`
			LicenseID int       `json:"license_id"`
			BindingID int       `json:"binding_id"`
			Token     string    `json:"entitlement"`
			CheckedAt time.Time `json:"checked_at"`
			ExpiresAt time.Time `json:"expires_at"`
		}
		if json.Unmarshal(response.body, &result) != nil || !result.Valid {
			return EntitlementProjection{Status: h.service.CurrentStatus()}, &LicenseError{
				Code:    ErrInvalidEntitlement,
				Message: "invalid activation response",
			}
		}
		jkt, _ := h.credentials.CurrentDeviceJKT(ctx)
		claims, err := VerifyEntitlementToken(ctx, result.Token, jkt, h.verifier)
		if err != nil {
			return EntitlementProjection{Status: h.service.CurrentStatus()}, err
		}
		if claims.LicenseID != result.LicenseID || claims.BindingID != result.BindingID ||
			claims.CheckedAt != result.CheckedAt.Unix() || claims.ExpiresAt != result.ExpiresAt.Unix() {
			return EntitlementProjection{Status: h.service.CurrentStatus()}, &LicenseError{
				Code:    ErrInvalidEntitlement,
				Message: "inconsistent entitlement response",
			}
		}
		outcome.Token = result.Token
		h.credentials.pendingKey = key
		defer func() { h.credentials.pendingKey = "" }()
	}
	return h.service.ProcessOnlineOutcome(ctx, outcome)
}

type cloudResponse struct {
	status int
	code   string
	body   []byte
}

func (h *Host) post(ctx context.Context, path string, body interface{}, requestKey string) (cloudResponse, error) {
	endpoint, parseErr := url.Parse(h.baseURL)
	if parseErr != nil || endpoint.Host == "" || endpoint.User != nil || endpoint.RawQuery != "" || endpoint.Fragment != "" {
		return cloudResponse{}, errors.New("invalid API endpoint")
	}
	local := endpoint.Hostname() == "localhost" || endpoint.Hostname() == "127.0.0.1" || endpoint.Hostname() == "::1"
	if endpoint.Scheme != "https" && !(endpoint.Scheme == "http" && local) {
		return cloudResponse{}, errors.New("HTTPS required")
	}
	if h.client == nil {
		return cloudResponse{}, errors.New("network unavailable")
	}
	payload, err := canonicalBody(body)
	if err != nil {
		return cloudResponse{}, err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	request, err := http.NewRequestWithContext(ctx, "POST", h.baseURL+path, bytes.NewReader(payload))
	if err != nil {
		return cloudResponse{}, err
	}
	request.Header.Set("Content-Type", "application/json")
	if requestKey != "" {
		request.Header.Set("Idempotency-Key", requestKey)
	}
	client := *h.client
	client.CheckRedirect = func(*http.Request, []*http.Request) error { return errors.New("redirect refused") }
	response, err := client.Do(request)
	if err != nil {
		return cloudResponse{}, err
	}
	defer response.Body.Close()
	data, err := io.ReadAll(io.LimitReader(response.Body, 65537))
	if err != nil || len(data) > 65536 {
		return cloudResponse{status: response.StatusCode}, nil
	}
	var problem struct {
		Error struct {
			Code string `json:"code"`
		} `json:"error"`
	}
	_ = json.Unmarshal(data, &problem)
	return cloudResponse{status: response.StatusCode, code: problem.Error.Code, body: data}, nil
}
