package main

import (
	"context"
	"schoolbot.local/app/internal/license"
	"testing"
)

type desktopCredentialDouble struct{ data []byte }

func (s *desktopCredentialDouble) Load(context.Context) ([]byte, error) {
	return append([]byte(nil), s.data...), nil
}
func (s *desktopCredentialDouble) Save(_ context.Context, data []byte) error {
	s.data = append([]byte(nil), data...)
	return nil
}

func TestLockedLicenseNeverReachesEngine(t *testing.T) {
	storage := &desktopCredentialDouble{}
	for restart := 0; restart < 2; restart++ {
		service := &DesktopService{entitlement: license.NewHost("https://fixture.invalid", nil, license.NewSystemClock(), &license.ES256Verifier{}, storage)}
		for _, actor := range []string{"guest", "login"} {
			for _, mode := range []string{"block-programming", "robot-control", "coding-adventure"} {
				if _, err := service.Run(mode, "run", "{}"); err == nil {
					t.Fatalf("%s/%s reached engine without first online activation", actor, mode)
				}
			}
		}
		projection, err := service.LicenseStatus()
		if err != nil || projection.Status != license.StatusUnactivated {
			t.Fatal("locked projection incorrect")
		}
	}
}
