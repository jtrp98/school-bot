package license_test

import (
	"bytes"
	"context"
	"errors"
	"schoolbot.local/app/internal/license"
	"testing"
	"time"
)

func TestHostEntitlementNormal(t *testing.T) {
	binary := buildHostServer(t)
	for _, backend := range []string{"memory", "file"} {
		t.Run(backend, func(t *testing.T) {
			t.Run("first_offline", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a := newHostComponent(server, 0)
				before := server.control("snapshot", nil)
				projection, err := a.host.Check(context.Background(), server.key, false)
				if err == nil || projection.Status != license.StatusUnactivated {
					t.Fatal("first offline must remain unactivated")
				}
				assertModes(t, a, false)
				a.restart()
				assertModes(t, a, false)
				credentials, err := license.OpenCredentials(context.Background(), a.storage)
				if err != nil {
					t.Fatal(err)
				}
				record, _ := credentials.Load(context.Background())
				if record != nil || a.network.calls != 0 {
					t.Fatal("offline request accepted a lease or called server")
				}
				if !bytes.Equal(before, server.control("snapshot", nil)) {
					t.Fatal("offline changed server")
				}
			})
			t.Run("activate_renew_restart_expire", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a := newHostComponent(server, 0)
				start := a.clock.wall.Unix()
				a.network.online = true
				projection, err := a.host.Check(context.Background(), server.key, false)
				if err != nil || projection.Status != license.StatusValid {
					t.Fatalf("activate: %v", err)
				}
				assertModes(t, a, true)
				original := a.claims(t)
				a.network.online = false
				a.restart()
				assertModes(t, a, true)
				a.at(start + 3600)
				server.control("time", start+3600)
				a.network.online = true
				projection, err = a.host.Check(context.Background(), server.key, true)
				if err != nil || projection.Status != license.StatusValid {
					t.Fatalf("renew: %v", err)
				}
				renewed := a.claims(t)
				if renewed.BindingID != original.BindingID || renewed.CheckedAt != start+3600 || renewed.ExpiresAt != start+3600+259200 {
					t.Fatal("renew did not preserve binding and use new server check time")
				}
				baseline := server.control("snapshot", nil)
				calls := a.network.calls
				a.network.online = false
				for _, day := range []int64{1, 2, 4} {
					a.at(start + day*86400)
					assertModes(t, a, day < 3)
					claims := a.claims(t)
					if claims.CheckedAt != renewed.CheckedAt || claims.ExpiresAt != renewed.ExpiresAt {
						t.Fatal("restart extended lease")
					}
				}
				if a.network.calls != calls || !bytes.Equal(baseline, server.control("snapshot", nil)) {
					t.Fatal("offline changed server")
				}
			})
			t.Run("denial_persists", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a := newHostComponent(server, 0)
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				server.control("time", a.clock.wall.Add(24*time.Hour).Unix())
				a.clock.Advance(24 * time.Hour)
				server.control("revoke", nil)
				before := server.control("snapshot", nil)
				projection, err := a.host.Check(context.Background(), server.key, true)
				var failure *license.LicenseError
				if !errors.As(err, &failure) || failure.Code != license.ErrLicenseInactive || projection.Status != license.StatusDenied {
					t.Fatalf("denial: %v", err)
				}
				assertModes(t, a, false)
				a.network.online = false
				a.restart()
				assertModes(t, a, false)
				if !bytes.Equal(before, server.control("snapshot", nil)) {
					t.Fatal("denial changed server unit")
				}
			})
			t.Run("move_preserves_old_expiry", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a, b := newHostComponent(server, 0), newHostComponent(server, 1)
				start := a.clock.wall.Unix()
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				original := a.claims(t)
				a.network.online = false
				server.control("time", start+3600)
				server.control("unbind", nil)
				b.at(start + 3600)
				if _, err := b.host.Check(context.Background(), server.key, false); err == nil {
					t.Fatal("new B offline unlocked")
				}
				assertModes(t, b, false)
				if b.network.calls != 0 {
					t.Fatal("offline B called server")
				}
				b.network.online = true
				if _, err := b.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				current := b.claims(t)
				if current.Confirmation.JKT == original.Confirmation.JKT || current.BindingID == original.BindingID {
					t.Fatal("B reused A identity")
				}
				assertModes(t, b, true)
				baseline := server.control("snapshot", nil)
				for _, day := range []int64{1, 4} {
					a.at(start + day*86400)
					assertModes(t, a, day < 3)
					claims := a.claims(t)
					if claims.CheckedAt != start || claims.ExpiresAt != start+259200 {
						t.Fatal("move extended A lease")
					}
				}
				if !bytes.Equal(baseline, server.control("snapshot", nil)) {
					t.Fatal("A offline altered moved server state")
				}
			})
		})
	}
}
