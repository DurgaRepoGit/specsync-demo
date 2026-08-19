# specsync-demo

Minimal repo to exercise the reusable SpecSync "update feature files" workflow.

## Layout

```
.github/workflows/
  specsync-update-feature-files.yml   # reusable workflow (workflow_call)
  update-feature-files.yml            # caller: runs on PRs to main
tests/Features/
  Login.feature                       # sample feature with SpecSync tags
```

## How it works

1. Open a PR that touches `tests/**/*.feature`.
2. The **Update Feature Files (Caller)** workflow triggers and calls the
   reusable workflow at `./.github/workflows/specsync-update-feature-files.yml`.
3. The reusable workflow:
   - installs the `SpecSync.AzureDevOps` dotnet tool,
   - dry-runs a push against your Azure DevOps project,
   - if valid, pushes for real — creating/updating test cases in ADO,
   - commits the resulting `@TestCaseId_*` tag additions back onto the PR
     branch,
   - comments on the PR with the outcome.

## Mock mode (no secrets required)

The reusable workflow takes a `mock` boolean input. When `true` it:

- skips the license file, the `setup-dotnet` step and the `SpecSync.AzureDevOps`
  tool install,
- skips both `dotnet specsync push` calls (dry-run and real), so nothing
  contacts Azure DevOps,
- still generates and prints `specsync_create_tests.json` so you can inspect
  exactly what would have been sent,
- injects synthetic `@TestCaseId_9001`, `@TestCaseId_9002`, ... tags onto any
  scenario that doesn't already have one,
- then runs the normal commit-back and PR-comment steps.

This lets you exercise the whole pipeline without a SpecSync license or an ADO
PAT. The tag injection is idempotent - re-running leaves already-tagged
scenarios alone.

The caller currently defaults to mock mode:

```yaml
mock: ${{ github.event_name != 'workflow_dispatch' || inputs.mock }}
```

i.e. PRs always run mocked, and `workflow_dispatch` exposes a checkbox
(defaulting to on). Once you have both secrets, hardcode `mock: false`.

Trigger a mock run:

```powershell
gh workflow run "Update Feature Files (Caller)" --repo <owner>/specsync-demo --ref main -f mock=true
gh run watch --repo <owner>/specsync-demo
```

## Setup before a real (non-mock) run

1. **Set `mock: false`** in `./.github/workflows/update-feature-files.yml`.
2. **Add repo secrets** (Settings -> Secrets and variables -> Actions):
   - `SPECSYNC_PERSONAL_ACCESS_TOKEN` - ADO PAT with test plan write access
   - `SPECSYNC_LICENSE_KEY` - your SpecSync license key contents
3. **Allow workflows to write and to create PR comments**
   (Settings -> Actions -> General -> Workflow permissions:
   "Read and write permissions" + "Allow GitHub Actions to create and approve
   pull requests").

## Tag conventions used by the sample

The reusable workflow's default `local_tags` filter is:

```
(@manual or @automated) and @system_requirement_* and @brite_*
```

`Login.feature` satisfies this with `@automated`, `@system_requirement_1234`
and `@brite_login`. After the first successful run, SpecSync will add a
`@TestCaseId_<n>` tag next to each scenario.

## Pushing this repo to GitHub

From this folder:

```powershell
git init -b main
git add .
git commit -m "chore: initial SpecSync demo"
gh repo create specsync-demo --private --source . --remote origin --push
```

(or create the repo via the web UI and `git push` manually).
