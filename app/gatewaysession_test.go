package main

import (
	"encoding/base64"
	"encoding/json"
	"testing"
	"time"
)

func TestGatewayLoginRequiresFullSchoolScopedUnexpiredToken(t *testing.T) {
	now := time.Unix(1900000000, 0)
	token := func(school, access string, exp int64) string {
		payload, _ := json.Marshal(map[string]any{"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode": school,
			"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid": "301", "AccessLevel": access, "exp": exp})
		return "header." + base64.RawURLEncoding.EncodeToString(payload) + ".signature"
	}
	envelope := func(value any) string {
		bytes, _ := json.Marshal(map[string]any{"code": 200, "results": value})
		return string(bytes)
	}
	for _, tc := range []struct{ raw, expected string }{
		{`{"code":200,"results":{}}`, "unavailable"},
		{envelope(map[string]any{"temporaryToken": "temporary"}), "verification_required"},
		{envelope(map[string]any{"forceResetPassword": true}), "password_reset_required"},
		{envelope(map[string]any{"token": token("12", "1", now.Unix()+60)}), "unavailable"},
		{envelope(map[string]any{"token": token("11", "0", now.Unix()+60)}), "unavailable"},
		{envelope(map[string]any{"token": token("11", "1", now.Unix())}), "unavailable"},
		{envelope(map[string]any{"token": token("11", "1", now.Unix()+60)}), "success"},
	} {
		session, status := parseGatewayLogin(tc.raw, "student", 11, now)
		if status != tc.expected || (session != nil) != (status == "success") {
			t.Fatalf("status=%s expected=%s", status, tc.expected)
		}
	}
}
