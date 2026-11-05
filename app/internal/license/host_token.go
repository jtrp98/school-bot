package license

import (
	"context"
	"errors"
)

// DeviceEntitlement stays inside the native host and never enters the webview.
func (h *Host) DeviceEntitlement(ctx context.Context) (string, error) {
	h.mu.Lock()
	defer h.mu.Unlock()
	if err := h.initialize(ctx); err != nil {
		return "", err
	}
	projection, err := h.service.Evaluate(ctx)
	if err != nil || projection.Status != StatusValid {
		return "", errors.New("active device entitlement required")
	}
	record, err := h.credentials.Load(ctx)
	if err != nil || record == nil || record.Token == "" {
		return "", errors.New("device entitlement unavailable")
	}
	return record.Token, nil
}

// LoginContext comes only from a verified entitlement, never from an editable school selector.
func (h *Host) LoginContext(ctx context.Context) (int, string, error) {
	h.mu.Lock()
	defer h.mu.Unlock()
	if err := h.initialize(ctx); err != nil {
		return 0, "", err
	}
	projection, err := h.service.Evaluate(ctx)
	if err != nil || projection.Status != StatusValid {
		return 0, "", errors.New("active device entitlement required")
	}
	h.service.mu.Lock()
	defer h.service.mu.Unlock()
	claims := h.service.currentClaims
	if claims == nil || claims.SchoolID <= 0 || claims.Confirmation.JKT == "" {
		return 0, "", errors.New("verified login context unavailable")
	}
	return claims.SchoolID, claims.Confirmation.JKT, nil
}
