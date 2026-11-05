package license_test

import (
	"bytes"
	"context"
	"encoding/json"
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
				before := server.control("snapshot", nil)
				status, body := server.call(0, "/fixture/unbind-without-role", "", json.RawMessage("null"))
				if status != 403 || !bytes.Contains(body, []byte(`"STAFF_ROLE_REQUIRED"`)) {
					t.Fatal("unbind without license admin role was not denied")
				}
				if !bytes.Equal(before, server.control("snapshot", nil)) {
					t.Fatal("unauthorized unbind changed server state")
				}
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
			t.Run("moved_device_online_denial_persists", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a, b := newHostComponent(server, 0), newHostComponent(server, 1)
				start := a.clock.wall.Unix()
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				a.network.online = false
				server.control("time", start+3600)
				server.control("unbind", nil)
				b.at(start + 3600)
				b.network.online = true
				if _, err := b.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				before := server.control("snapshot", nil)
				server.control("time", start+7200)
				a.at(start + 7200)
				a.network.online = true
				projection, err := a.host.Check(context.Background(), server.key, true)
				var failure *license.LicenseError
				if !errors.As(err, &failure) || failure.Code != license.ErrBindingRevoked ||
					projection.Status != license.StatusDenied {
					t.Fatalf("moved device denial: %v", err)
				}
				assertModes(t, a, false)
				a.network.online = false
				a.restart()
				assertModes(t, a, false)
				if !bytes.Equal(before, server.control("snapshot", nil)) {
					t.Fatal("moved device denial changed server state")
				}
			})
			t.Run("duplicate_key_device_limit", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a, b := newHostComponent(server, 0), newHostComponent(server, 1)
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				assertModes(t, a, true)
				b.network.online = true
				projection, err := b.host.Check(context.Background(), server.key, false)
				var failure *license.LicenseError
				if !errors.As(err, &failure) || failure.Code != license.ErrDeviceLimit || projection.Status != license.StatusDenied {
					t.Fatalf("duplicate key expected DEVICE_LIMIT, got proj=%+v err=%v", projection, err)
				}
				assertModes(t, b, false)
				assertModes(t, a, true)
			})
			t.Run("move_and_reauthorize_original_device", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a, b := newHostComponent(server, 0), newHostComponent(server, 1)
				start := a.clock.wall.Unix()
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				firstClaims := a.claims(t)
				jktA := firstClaims.Confirmation.JKT

				server.control("time", start+1800)
				server.control("unbind", nil)

				// Detached device A cannot self-reactivate without admin approval
				a.at(start + 1800)
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err == nil {
					t.Fatal("detached device A re-activated without next_device_jkt approval")
				}

				// Device B activates on the unbound key
				b.at(start + 1800)
				b.network.online = true
				if _, err := b.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatalf("device B activate after unbind: %v", err)
				}
				bClaims := b.claims(t)
				if bClaims.BindingEpoch != firstClaims.BindingEpoch+1 {
					t.Fatalf("expected epoch %d, got %d", firstClaims.BindingEpoch+1, bClaims.BindingEpoch)
				}

				// Admin unbinds B and explicitly authorizes A via next_device_jkt
				server.control("time", start+3600)
				server.control("unbind", jktA)

				a.at(start + 3600)
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatalf("reauthorized device A activate: %v", err)
				}
				reauthClaims := a.claims(t)
				if reauthClaims.BindingEpoch != bClaims.BindingEpoch+1 || reauthClaims.BindingID == firstClaims.BindingID {
					t.Fatalf("expected new binding at epoch %d, got %+v", bClaims.BindingEpoch+1, reauthClaims)
				}
				assertModes(t, a, true)
			})
			t.Run("tamper_copy_and_clock_rollback_protection", func(t *testing.T) {
				server := startHostServer(t, binary, backend)
				a, b := newHostComponent(server, 0), newHostComponent(server, 1)
				start := a.clock.wall.Unix()
				a.network.online = true
				if _, err := a.host.Check(context.Background(), server.key, false); err != nil {
					t.Fatal(err)
				}
				a.network.online = false
				validBlob := bytes.Clone(a.storage.data)

				// 1. Cross-device token copy: copy A's record into B's credential envelope
				credA, err := license.OpenCredentials(context.Background(), a.storage)
				if err != nil {
					t.Fatal(err)
				}
				recA, err := credA.Load(context.Background())
				if err != nil || recA == nil {
					t.Fatal("missing record A")
				}
				credB, err := license.OpenCredentials(context.Background(), b.storage)
				if err != nil {
					t.Fatal(err)
				}
				if err := credB.Save(context.Background(), recA); err != nil {
					t.Fatal(err)
				}
				b.restart()
				projB, errB := b.host.Evaluate(context.Background())
				var licErr *license.LicenseError
				if !errors.As(errB, &licErr) || licErr.Code != license.ErrInvalidEntitlement || projB.Status != license.StatusDenied {
					t.Fatalf("copied token to B must fail with INVALID_ENTITLEMENT, got proj=%+v err=%v", projB, errB)
				}
				assertModes(t, b, false)

				// 2. Clock rollback on A locks offline access until online re-validation
				a.at(start + 7200)
				assertModes(t, a, true)
				a.at(start + 3600) // 1 hour rollback behind observed wall time
				projRollback, errRollback := a.host.Evaluate(context.Background())
				if !errors.As(errRollback, &licErr) || licErr.Code != license.ErrClockUntrusted || projRollback.Status != license.StatusClockUntrusted {
					t.Fatalf("clock rollback must fail with CLOCK_UNTRUSTED, got proj=%+v err=%v", projRollback, errRollback)
				}
				assertModes(t, a, false)

				// Online validation at valid server time restores trust
				server.control("time", start+10800)
				a.at(start + 10800)
				a.network.online = true
				projRecovered, errRecovered := a.host.Check(context.Background(), server.key, true)
				if errRecovered != nil || projRecovered.Status != license.StatusValid {
					t.Fatalf("online recovery after rollback failed: proj=%+v err=%v", projRecovered, errRecovered)
				}
				assertModes(t, a, true)

				// 3. Corrupted credential blob fails closed with CREDENTIAL_STORE_FAILED
				a.network.online = false
				a.storage.data = []byte("corrupted-credential-envelope")
				a.restart()
				_, errCorrupt := a.host.Evaluate(context.Background())
				if !errors.As(errCorrupt, &licErr) || licErr.Code != license.ErrCredentialStoreFailed {
					t.Fatalf("corrupted store must fail with CREDENTIAL_STORE_FAILED, got %v", errCorrupt)
				}
				assertModes(t, a, false)
				a.storage.data = validBlob
			})
		})
	}
}
