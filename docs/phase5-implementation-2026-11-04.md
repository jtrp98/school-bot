# Phase 5 implementation and local verification — 2026-11-04

This records implemented source and local checks. It is not a production acceptance verdict or permission to close existing gates. The canonical task records remain in `knowledge/school-bot/build/plan/tasks-001.json`.

**Code-only continuation:** The user subsequently instructed “ไม่ต้องทดสอบ ทำแค่ code ก็พอ”. The authority/PGGrade adapters, calculator/document codec, skipped-cell handling, overlap guard, worker principal resolver and key-file setup below were added after the verification table. No build, tests, lint, database connection, key generation, server launch or deployment was run for these changes. Earlier passing counts are historical evidence only.

## Implemented behavior

| Area | Implementation |
| --- | --- |
| Lessons and submissions | Published curriculum/mission/criteria catalog, immutable project revision ownership, three modes, durable submission/result/job records, student progress and authorized teacher reads. Opening a lesson does not mark it passed. |
| Code/Block grading | Server interprets a bounded command format against the approved criteria revision; grading jobs recover after restart. The client supplies program evidence, never an authoritative score. |
| Arm review | Awaiting-teacher state, saved pose evidence, full rubric review, ETag, audit, and append-only corrections with reason/superseded assessment. |
| Attempts | Stored question/option UUID order, bank/curriculum/policy/evaluator snapshots, server deadline, save/submit ETag, atomic idempotency, automatic choice grading, and bounded expiry sweep/read recovery. |
| Expired submissions | At or after the deadline, only answers already saved before expiry count. Answers attached to a late submit are ignored; retries return the stored result. Unsupported evaluator versions are deferred rather than awarded zero. |
| Teacher controls | Run selection, time/window override, learner retakes, scoring weights totaling 10,000 basis points, activation freeze, normalized totals, and whole-class level proposals with snapshot recheck. |
| Grade export coordinator | Existing-slot candidate selection, ownership/mapping, decimal conversion, expiring preview, confirmation/outbox/audit, worker lease, conflict stop, receipt recovery, explicit skipped cells and overlapping-export rejection. |
| School authority | PostgreSQL adapter intersects current teacher assignments, subject/room/term and explicit learner enrollments. It requires unique active status-0 history; movement/non-active/duplicate histories fail closed. Whole-class resolution never silently omits an unresolved learner. |
| PGGrade adapter | Added source for existing-cell discovery, read/lock checks, full-document hash/xmin CAS, per-learner PG transaction, existing-cell JSON merge, derived fields and receipt insertion. Writes remain gated; the new source has not been executed. |
| Releases | Release metadata now uses the repository-backed service and survives backing-store restart. BE-018 remains classified as P3 in the plan. |
| Offline desktop | Bounded per-account queue, cached lesson catalog, durable project/submission keys, reconnect drain, result refresh, native Gateway JWT/entitlement, and monotonic server-based exam timer. An offline result becomes authoritative only after server evaluation. |

PostgreSQL repositories participate in the existing atomic unit of work. Memory/file adapters support local tests only. Curriculum file replacement retries bounded Windows sharing violations. The development project-file adapter still has a process-crash window between project storage and replay-ledger publication; it is not a substitute for the PostgreSQL transaction.

## UI locations and login

- Student desktop: `target/school-bot/app/frontend/src/learning/`.
- Teacher web: `target/sb-tech-upgrade-web-system/src/app/[locale]/pages/academic/school-bot/`; URL `/{locale}/pages/academic/school-bot`, existing teacher web session and BFF.
- Central content Admin: `target/sb-web-helper/src/app/pages/admin/school-bot/`; URL `/pages/admin/school-bot`, sidebar item **บทเรียนและข้อสอบ**, existing `ADMIN_ACCESS` permission.

The Admin BFF and Gateway both use `/api/service-school-bot/admin/...`, without a version segment. Desktop device proofs use the same versionless path as their HTTP request; the SchoolBot verifier requires that exact path. Client and server must be updated together. This route correction was not built or tested.

The Admin uses the existing sb-helper login. Its server signs an ES256 assertion lasting 60 seconds with `sub=helper user.id`, `admin_id2=session.user.admin_id`, issuer `sb-web-helper`, audience `school-bot-admin`, and scope `schoolbot:content`. The JWT travels server-to-server in `X-SchoolBot-Staff-Assertion`; it is not returned to the browser. Gateway validates it and resolves exactly one active `TAdmin.Id2` to canonical `TAdmin.Id`. Four-eyes audit uses that canonical person. No service account, employee-code fallback, or permission grant was added.

Deployment configuration, without committing private material:

| Process | Setting | Value |
| --- | --- | --- |
| sb-helper server | `SCHOOL_BOT_GATEWAY_ORIGIN` | Fixed HTTPS Gateway origin; HTTP allowed only for loopback development |
| sb-helper server | `SCHOOL_BOT_STAFF_SIGNING_KEY_PEM` | P-256 private key, PEM PKCS#8 |
| Gateway server | `SchoolBot__HelperPublicKeyPem` | Corresponding public key, PEM SubjectPublicKeyInfo |
| sb-helper server | `SCHOOL_BOT_STAFF_SIGNING_KEY_FILE` | Alternative bounded private-key file/secret mount; inline PEM takes precedence |
| Gateway server | `SchoolBot__HelperPublicKeyFile` | Alternative bounded public-key file/secret mount; inline PEM takes precedence |
| SchoolBot server | `SchoolBot__LegacyAuthority__TimeZoneId` | Explicit time zone for School DB timestamps; no implicit timestamp-zone assumption |

Missing configuration fails closed. Normal Gateway JWT authentication continues to serve existing clients; the assertion scheme applies only to explicitly authorized SchoolBot content Admin actions. Mutation requests also bind the expected helper user so a pending form cannot silently save under a newly switched account.

`scripts/api/with-helper-auth.ps1` supplies source for protected local P-256 key preparation and Helper/Gateway launch environments. `Configure` prepares files only; `Helper`/`Gateway` launch the selected process with file paths. It preserves an existing private key, serializes preparation, rejects links, restricts the signing directory to its owner, and prints no key/token. These scripts were written but not executed in the code-only continuation.

PostgreSQL DI now registers `SchoolAssessmentAuthority` and `PostgresGradeDestination`; Memory retains the unconfigured defaults unless an explicit fixture supplies authority. `GatewayExportPrincipalResolver` reconstructs the original canonical school/employee identity and rechecks current profile/license/assignment without replaying a stored JWT. Preview roster lookup is independent of next-term lookup; promotion still requires a unique registered future term.

The grade writer has no configuration boolean that approves parity or coexistence. `GradeExportWriteAuthorization` must be supplied by a reviewed deployment composition with current approving identity, expiry, matching calculator version and writer/calculator/coexistence/target/authz/database evidence references. No such authorization is registered here, so confirm/apply remain closed. `LegacyGradeCalculator` is an unverified, numeric-cell-only port of the four-period legacy calculation profile (academic number year >= 2563); its version constant is not parity evidence. Duplicate JSON keys/cell identifiers, unknown scores, special/manual overrides, missing detail rows and ambiguous templates do not permit a write.

Source references for the port are `sb-web-academic/jabjaiuniversity/SchoolBright.DataAccess/Repository/GradeDetailReadRepository.Corrections.Part3.Recalculate.cs`, `GradeDetailCorrectionHelper.cs`, and `GradeRepository.cs` (`SafeRoundScore` and `GetScoreIdentifierName`), plus the academic teacher/enrollment repositories and existing SchoolBot EF mappings. Source inspection does not establish runtime parity, timestamp conventions, or compatibility with every legacy calculation profile.

Missing/ungraded/error sources are represented in `skipped_cells` with old values retained. Entirely skipped learners receive `skipped` outbox state and no grade receipt; a completed batch with skipped cells is `completed_with_skips`. Existing real zero scores remain numeric sources. Unresolved exports for the same physical destination/learner block a second confirmation with `EXPORT_IN_PROGRESS`, including across run IDs.

Conflict resolution now has source across Application, typed gRPC, Gateway and teacher UI: `POST /api/service-school-bot/score-exports/{id}/resolutions` accepts `decisions: [{learner_id, action: "keep" | "replace"}]`. It creates a fresh expiring preview; the ordinary hash/ETag confirmation step is still required. Each unresolved learner needs exactly one decision. Already applied/skipped learners are retained, and old pending/conflict outbox rows become `blocked` with `SUPERSEDED_BY_PREVIEW` atomically with the new export. Original mapping/ownership history remains locked. Active/unknown writes cannot be superseded; receipt recovery must resolve them first. A changed class roster still requires separate reconciliation.

An ambiguous PG commit is recorded as `unknown`, retaining its original operation key/hash. Receipt lookup obtains the same PG advisory operation lock before interpreting a missing receipt, so an in-flight transaction finishes before a retry decision. A conflicted parent can reconcile unknown/expired-processing items without applying new grades. These recovery paths are source-only and unverified in this continuation.

The Admin supports draft metadata, lesson/mission/rubric editing, question-bank revisions, policies, independent criteria approval, and publication. Sheet input is a **normalized JSON export**, not a direct Google Sheet/XLSX connector. New project activities use permanent `mission_key` references. Cloning a legacy curriculum containing only numeric `mission_id` references is rejected before creating a new draft until those references are migrated. Server-side import still requires the existing `ImportSource` migration/re-scaffold.

## Earlier local verification — before the code-only continuation

| Check | Result |
| --- | --- |
| `dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1 --no-restore` from `school-bot/web` | 4,445 passed, 0 failed, 0 skipped |
| Gateway platform tests filtered to `FullyQualifiedName~SchoolBot` | 34 passed, including assertion validation and authentication-handler route isolation |
| Desktop `go test ./...` | All six packages passed |
| Desktop `go vet ./...` | Passed |
| Desktop frontend i18n + Phase 3 project/entry + Phase 5 offline tests | 24 passed; the Vite test helpers logged an existing HMR port collision warning but tests completed successfully |
| Desktop frontend `npm run build` | Typecheck/build passed; release asset check: 128 files, no original source/symbols/source maps |
| Teacher web learning + page-registry Vitest tests | 8 passed |
| Teacher web targeted source TypeScript check, including imported dependencies | 0 diagnostics |
| Teacher web targeted ESLint | Passed |
| Admin Bun signing/schema/store/payload tests | 8 passed |
| Admin BFF route tests, separate mocked process | 5 passed; no real auth/DB/HTTP calls |
| Admin targeted source TypeScript check | 0 feature diagnostics; 33 existing dependency diagnostics, so this is **not** a clean full-repository typecheck |
| Admin targeted ESLint | Passed, 0 errors and 0 warnings |
| SchoolBot/Gateway proto contract | Matching normalized hash `4bdf483ebc1c6a86e879392936b2c12e497e29c757d44ac928b7ae026680fa63` |

Admin component browser checks use real form components and a local fixture, without a production session or data. They exercise metadata/lesson save callbacks, keyboard focus, loading disable state and a 390px viewport. Screenshots and the canonical implementation record are under `knowledge/school-bot/build/plan/`. They do not prove the deployed app-shell/login/database flow. The temporary browser fixture lives in the helper's ignored `.claude/tmp/phase5-preview/` directory.

The fixture also renders the helper theme provider. Vite reports the existing stylesheet import order and Next styled-jsx/script-provider compatibility warnings; no browser page exception occurred. This fixture does not replace verification inside the deployed Next app.

## Required before production acceptance

1. Connect and verify trusted Gateway-to-gRPC transport and enable the existing module gate through the deployment process. No permissive transport verifier was added.
2. Verify `SchoolAssessmentAuthority` against the actual school timestamp convention and roster policy. The implemented strict subset requires explicit course enrollment and rejects moved/non-active/duplicate histories; the general legacy movement/history resolver remains unresolved. `knowledge/school-bot/build/design/teacher-authority-001.md` forbids granting membership by greatest history ID or guessed status/time rules.
3. Deploy the helper signing pair/origin and verify the active `TAdmin.Id2 → Id` relation and account flags with actual login sessions. No live account or database was modified here.
4. Verify the new `PostgresGradeDestination`, `GatewayExportPrincipalResolver`, document codec, legacy calculation port and conflict-resolution flow, including target shape, formula parity, coexistence, locks, receipt retention/privileges and approved deployment evidence. Resolve movement/roster changes and unsupported legacy calculation profiles through the authoritative owner policy. No real grade cell was written and no write authorization is installed.
5. Run the migration/re-scaffold and production-like database/restart/concurrency checks, plus the complete QA-011..QA-016 acceptance matrix and signed content inventory. The passing local fixtures do not close these requirements.
