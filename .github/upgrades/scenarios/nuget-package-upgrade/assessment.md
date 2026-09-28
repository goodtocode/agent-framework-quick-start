# NuGet package upgrade assessment

_Mode: **full scan** — exact breaking-change call sites were located across all scoped projects._

## Recommended versions

- **Microsoft.FluentUI.AspNetCore.Components**: **5.0.0** (unified across 1 project(s)).
- **Microsoft.FluentUI.AspNetCore.Components.Emoji**: **5.0.0** (unified across 1 project(s)).
- **Microsoft.FluentUI.AspNetCore.Components.Icons**: **5.0.0** (unified across 1 project(s)).

## Public API changes

> **Types moved (namespace changed) are not removals.** A moved type keeps its name and members;
> the fix is a `using`-directive change, not a rewrite. Do not treat a moved type as deleted.

- **Microsoft.FluentUI.AspNetCore.Components**: 323 type(s) removed, 4 type(s) moved (namespace changed), 1071 member(s) removed, 124 signature(s) changed — see [`apidiff/Microsoft.FluentUI.AspNetCore.Components.apidiff.md`](apidiff/Microsoft.FluentUI.AspNetCore.Components.apidiff.md).
- **Microsoft.FluentUI.AspNetCore.Components.Emoji**: no source-breaking public API changes detected — see [`apidiff/Microsoft.FluentUI.AspNetCore.Components.Emoji.apidiff.md`](apidiff/Microsoft.FluentUI.AspNetCore.Components.Emoji.apidiff.md).
- **Microsoft.FluentUI.AspNetCore.Components.Icons**: no source-breaking public API changes detected — see [`apidiff/Microsoft.FluentUI.AspNetCore.Components.Icons.apidiff.md`](apidiff/Microsoft.FluentUI.AspNetCore.Components.Icons.apidiff.md).

## Breaking-change findings

- Usages of removed API (PkgApi.0001): 186
- Usages of signature-changed API (PkgApi.0002): 55
- Version divergence findings (Pkg.0003): 0
- Requested-version-unsupported findings (Pkg.0002): 0

Exact call-site locations are available via `query_dotnet_assessment` (list issues per project/file).

## Next steps

1. Proceed to planning to triage the API changes above and plan the code fixes (if any).

