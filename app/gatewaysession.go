package main

import (
	"encoding/base64"
	"encoding/json"
	"errors"
	"strconv"
	"strings"
	"time"
)

// These claims are only for local cache separation. The Gateway validates the JWT for every API request.
func gatewayClaims(token string) (string, string, int64, error) {
	parts := strings.Split(token, ".")
	if len(parts) != 3 {
		return "", "", 0, errors.New("AUTH_REQUIRED")
	}
	bytes, err := base64.RawURLEncoding.DecodeString(parts[1])
	if err != nil {
		return "", "", 0, errors.New("AUTH_REQUIRED")
	}
	var claims map[string]json.RawMessage
	if json.Unmarshal(bytes, &claims) != nil {
		return "", "", 0, errors.New("AUTH_REQUIRED")
	}
	text := func(keys ...string) string {
		for _, key := range keys {
			if raw, ok := claims[key]; ok {
				var value string
				if json.Unmarshal(raw, &value) == nil {
					return value
				}
				return string(raw)
			}
		}
		return ""
	}
	school := text("PostalCode", "postalcode", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode")
	user := text("Sid", "sid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")
	expiry, _ := strconv.ParseInt(text("exp"), 10, 64)
	schoolID, _ := strconv.Atoi(school)
	userID, _ := strconv.Atoi(user)
	if schoolID <= 0 || userID <= 0 || text("AccessLevel") != "1" || expiry <= 0 {
		return "", "", 0, errors.New("AUTH_REQUIRED")
	}
	return strconv.Itoa(schoolID), strconv.Itoa(userID), expiry, nil
}

func parseGatewayLogin(raw, username string, schoolID int, now time.Time) (*DesktopSession, string) {
	var response struct {
		Code    int `json:"code"`
		Results struct {
			Token              string `json:"token"`
			TemporaryToken     string `json:"temporaryToken"`
			ForceResetPassword bool   `json:"forceResetPassword"`
		} `json:"results"`
	}
	if json.Unmarshal([]byte(raw), &response) != nil || response.Code != 200 {
		return nil, "unavailable"
	}
	if response.Results.ForceResetPassword {
		return nil, "password_reset_required"
	}
	if response.Results.Token == "" && response.Results.TemporaryToken != "" {
		return nil, "verification_required"
	}
	school, _, expiry, err := gatewayClaims(response.Results.Token)
	if err != nil || school != strconv.Itoa(schoolID) || !now.Before(time.Unix(expiry, 0)) {
		return nil, "unavailable"
	}
	return &DesktopSession{Username: username, Token: response.Results.Token, ExpiresAt: time.Unix(expiry, 0).UTC().Format(time.RFC3339)}, "success"
}
