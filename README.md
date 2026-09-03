# specsync-demo

A Proof of Concept demonstrating automated BDD test case synchronisation from
Gherkin feature files to Azure DevOps Test Cases using SpecSync and GitHub Actions.

> **POC Status:** Three test cases successfully created in ADO project
> `CII-Surveillance` — IDs `#483377`, `#483378`, `#483379`.

---

## What This Does

Every time a `.feature` file changes, a GitHub Actions workflow:

1. Validates connectivity to Azure DevOps (dry run)
2. Pushes Gherkin scenarios to ADO as **Test Cases**
3. Writes `@TestCaseId_<id>` tags back into the feature file
4. Commits the updated tags to the branch automatically

New scenarios (no `@TestCaseId_*` tag) get a new ADO Test Case created.
Existing scenarios (already tagged) are updated in ADO. The process is
**idempotent** — re-running on already-tagged scenarios is safe.

---

## Repository Layout

```
.github/workflows/
  update-feature-files.yml               # Caller workflow (PR + workflow_dispatch)
  specsync-update-feature-files.yml      # Reusable workflow — all SpecSync logic

tests/Features/
  Login.feature                          # Sample feature file with 3 scenarios
```

---

## Feature File Tag Conventions

The SpecSync filter only picks up scenarios that satisfy **all three** of:

```
(@manual or @automated) and @system_requirement_* and @brite_*
```

`Login.feature` uses:

| Tag | Level | Purpose |
|-----|-------|---------|
| `@automated` | Feature | Marks all scenarios as automated tests |
| `@system_requirement_253764` | Feature | Links to ADO System Requirement work item #253764 (mirrored from Polarion) |
| `@brite_login` | Feature | Domain tag required by the filter |
| `@brite_login_*` | Scenario | Scenario-specific domain tag |
| `@TestCaseId_*` | Scenario | Written back by SpecSync after first sync |

### Polarion & ADO Requirements

Requirements are managed in **Polarion** and mirrored into Azure DevOps as
`System Requirement` work items. Two approaches for the `@system_requirement_*`
tag are available:

| Approach | Tag format | ADO link created? |
|----------|------------|-------------------|
| Use ADO-mirrored ID *(used in this POC)* | `@system_requirement_253764` | ✅ Yes — `Tests` relationship |
| Use Polarion ID directly | `@system_requirement_REQ-1234` | ❌ No — documentation only |

---

## Triggering the Workflow

### Option A — Pull Request (automatic)

Open a PR targeting `main` that touches any `tests/**/*.feature` file.
The workflow fires automatically.

### Option B — Manual (`workflow_dispatch`)

```bash
gh workflow run update-feature-files.yml \
  --repo DurgaRepoGit/specsync-demo \
  --field mock=false
```

Watch the run:

```bash
gh run watch --repo DurgaRepoGit/specsync-demo
```

---

## Required GitHub Secrets

Add these under **Settings → Secrets and variables → Actions**:

| Secret | Description |
|--------|-------------|
| `SPECSYNC_PERSONAL_ACCESS_TOKEN` | ADO Personal Access Token — scope: **Test Management: Read & Write** |
| `SPECSYNC_LICENSE_KEY` | SpecSync license file contents *(optional — Free mode works without it)* |

> **Free mode note:** SpecSync runs in Free mode when no license is provided.
> Test case creation and tag writeback work fully. Automated work item linking
> (`Tests` relationship to System Requirements) requires a full license.
> Contact the `philips-internal/aci-shared-workflows` team for the Philips
> SpecSync license (support code: `C12CD`).

Generate an ADO PAT at:
`https://dev.azure.com/ADSP-Org-A03` → avatar → **Personal access tokens**

---

## Mock Mode

Pass `mock=true` to run the entire pipeline without contacting Azure DevOps or
requiring any secrets. A Python script injects synthetic `@TestCaseId_9001`,
`9002`, … tags to simulate SpecSync output, and the commit-back and PR-comment
steps still run normally.

```bash
gh workflow run update-feature-files.yml \
  --repo DurgaRepoGit/specsync-demo \
  --field mock=true
```

Useful for testing the workflow wiring before secrets are available.

---

## SpecSync Configuration

The config file `specsync_create_tests.json` is generated at runtime inside
the `tests/` directory and is git-ignored. Key settings:

```json
{
  "remote": { "projectUrl": "https://dev.azure.com/ADSP-Org-A03/CII-Surveillance" },
  "local":  { "tags": "(@manual or @automated) and @system_requirement_* and @brite_*" },
  "synchronization": {
    "testCaseTagPrefix": "TestCaseId",
    "tagPrefixSeparators": ["_"]
  }
}
```

All values are injected from workflow inputs — nothing is hardcoded.

---

## Test Cases Created (POC)

| ADO ID | Scenario | State |
|--------|----------|-------|
| [#483377](https://dev.azure.com/ADSP-Org-A03/CII-Surveillance/_workitems/edit/483377) | Successful login with valid credentials | Design |
| [#483378](https://dev.azure.com/ADSP-Org-A03/CII-Surveillance/_workitems/edit/483378) | Failed login with invalid password | Design |
| [#483379](https://dev.azure.com/ADSP-Org-A03/CII-Surveillance/_workitems/edit/483379) | Successful logout after login | Design |

All three carry a **Tests** relationship to System Requirement
[#253764](https://dev.azure.com/ADSP-Org-A03/CII-Surveillance/_workitems/edit/253764)
(*Test System Requirement*).

---

## Next Steps

- [ ] Obtain a renewed SpecSync license to fully automate work item linking
- [ ] Enable the workflow as a required PR status check on `main`
- [ ] Rotate the ADO PAT before moving to a shared environment
- [ ] Add test cases to a Test Suite inside a Test Plan to surface them in `_testPlans`
