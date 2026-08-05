# Branching

This repository uses the QimERP org branching standard:

- **`development`** — integration branch (PR target for daily work)
- **`main`** — production releases only (PR from `development` or `hotfix/*`)

NuGet packages publish on **`main`** merges and **`v*.*.*`** tags only.

See [docs/git-branching-and-release.md](../docs/git-branching-and-release.md) in the monorepo workspace, or your org copy of that document.

**Note:** Legacy default branch `master` is retained for compatibility; new work should target **`main`** and **`development`**.
