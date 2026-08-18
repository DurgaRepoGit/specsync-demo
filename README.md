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

## Setup before first run

1. **Edit the caller** `./.github/workflows/update-feature-files.yml` and
   replace `<YOUR_ADO_PROJECT_URL>` with your Azure DevOps project URL, e.g.
   `https://dev.azure.com/<org>/<project>`.
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
