package license

import (
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"crypto/x509"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"math/big"
)

type DeviceJWK struct {
	Crv string `json:"crv"`
	Kty string `json:"kty"`
	X   string `json:"x"`
	Y   string `json:"y"`
}

func publicJWK(key *ecdsa.PrivateKey) DeviceJWK {
	return DeviceJWK{
		Crv: "P-256",
		Kty: "EC",
		X:   base64.RawURLEncoding.EncodeToString(key.X.FillBytes(make([]byte, 32))),
		Y:   base64.RawURLEncoding.EncodeToString(key.Y.FillBytes(make([]byte, 32))),
	}
}

func signProof(key *ecdsa.PrivateKey, payload interface{}) (string, error) {
	header := base64.RawURLEncoding.EncodeToString([]byte(`{"alg":"ES256","typ":"schoolbot-device-proof+jwt"}`))
	body, err := json.Marshal(payload)
	if err != nil {
		return "", err
	}
	unsigned := header + "." + base64.RawURLEncoding.EncodeToString(body)
	digest := sha256.Sum256([]byte(unsigned))
	r, s, err := ecdsa.Sign(rand.Reader, key, digest[:])
	if err != nil {
		return "", err
	}
	signature := append(r.FillBytes(make([]byte, 32)), s.FillBytes(make([]byte, 32))...)
	return unsigned + "." + base64.RawURLEncoding.EncodeToString(signature), nil
}

type ES256Verifier struct{ Keys map[string]*ecdsa.PublicKey }

func (v *ES256Verifier) VerifySignature(_ context.Context, data, signature []byte, kid string) error {
	key := v.Keys[kid]
	if key == nil || key.Curve != elliptic.P256() || len(signature) != 64 {
		return errors.New("unknown signing key or invalid signature")
	}
	digest := sha256.Sum256(data)
	if !ecdsa.Verify(key, digest[:], new(big.Int).SetBytes(signature[:32]), new(big.Int).SetBytes(signature[32:])) {
		return errors.New("invalid signature")
	}
	return nil
}

// BlobStore must protect confidentiality and integrity and atomically replace the whole value.
type BlobStore interface {
	Load(context.Context) ([]byte, error)
	Save(context.Context, []byte) error
}

type credentialEnvelope struct {
	PrivateKey []byte            `json:"private_key"`
	LicenseKey string            `json:"license_key"`
	Record     *GenerationRecord `json:"record"`
}

type Credentials struct {
	store      BlobStore
	envelope   credentialEnvelope
	key        *ecdsa.PrivateKey
	pendingKey string
}

func OpenCredentials(ctx context.Context, store BlobStore) (*Credentials, error) {
	if store == nil {
		return nil, errors.New("secure store missing")
	}
	data, err := store.Load(ctx)
	if err != nil {
		return nil, err
	}
	c := &Credentials{store: store}
	if len(data) == 0 {
		c.key, err = ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
		if err != nil {
			return nil, err
		}
		c.envelope.PrivateKey, err = x509.MarshalECPrivateKey(c.key)
		if err != nil {
			return nil, err
		}
		encoded, err := json.Marshal(c.envelope)
		if err != nil {
			return nil, err
		}
		if err = store.Save(ctx, encoded); err != nil {
			return nil, err
		}
	} else {
		if len(data) > 65536 || json.Unmarshal(data, &c.envelope) != nil {
			return nil, errors.New("invalid secure credentials")
		}
		c.key, err = x509.ParseECPrivateKey(c.envelope.PrivateKey)
		if err != nil || c.key.Curve != elliptic.P256() {
			return nil, errors.New("invalid device key")
		}
	}
	// Signing on every reopen proves that the protected private key remains usable.
	if _, err = signProof(c.key, map[string]string{"purpose": "startup"}); err != nil {
		return nil, err
	}
	return c, nil
}

func (c *Credentials) CurrentDeviceJKT(context.Context) (string, error) {
	encoded, err := json.Marshal(publicJWK(c.key))
	if err != nil {
		return "", err
	}
	digest := sha256.Sum256(encoded)
	return hex.EncodeToString(digest[:]), nil
}

func (c *Credentials) Load(context.Context) (*GenerationRecord, error) {
	if c.envelope.Record == nil {
		return nil, nil
	}
	record := *c.envelope.Record
	return &record, nil
}

func (c *Credentials) Save(ctx context.Context, record *GenerationRecord) error {
	next := c.envelope
	if record != nil {
		copyRecord := *record
		next.Record = &copyRecord
	} else {
		next.Record = nil
	}
	if c.pendingKey != "" && record != nil && record.Status == StatusValid {
		next.LicenseKey = c.pendingKey
	}
	encoded, err := json.Marshal(next)
	if err != nil {
		return err
	}
	if err = c.store.Save(ctx, encoded); err != nil {
		return err
	}
	c.envelope = next
	return nil
}
