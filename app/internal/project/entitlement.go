package project

import (
	"context"
)

// EntitlementChecker verifies whether the host holds an active entitlement.
type EntitlementChecker interface {
	CheckEntitlement(ctx context.Context) error
}

// InjectedEntitlement provides an explicit entitlement outcome without live cloud checks.
type InjectedEntitlement struct {
	Active bool
	Reason string
}

// CheckEntitlement evaluates the explicitly injected state.
func (e InjectedEntitlement) CheckEntitlement(_ context.Context) error {
	if !e.Active {
		msg := "active license entitlement required for local projects"
		if e.Reason != "" {
			msg = e.Reason
		}
		return &StoreError{
			Code:    ErrLicenseRequired,
			Message: msg,
		}
	}
	return nil
}
