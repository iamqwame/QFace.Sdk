# QFace standards analyzers

`QFace.Sdk.Standards.Analyzers` enforces architectural rules that can be established from C# compilation data. Add it to every QimERP service project as an analyzer:

```xml
<PackageReference Include="QFace.Sdk.Standards.Analyzers" Version="1.0.0"
                  PrivateAssets="all"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

The analyzer infers a module from the first three segments of a `QimErp.*` assembly or namespace. `QimErp.Shared.*` is intentionally allowed. A module project must not reference or import another module boundary.

| Diagnostic | QFace rule | Enforcement |
| --- | --- | --- |
| QFS001 | DATA-01 | Cross-module assembly references and namespaces |
| QFS002 | DATA-04 | Cross-module navigation properties and EF relationships |
| QFS003 | DATA-15 | DbContext calls from a request handler |
| QFS004 | DATA-02 | HttpClient calls from a request handler |
| QFS005 | DATA-20 | `IgnoreQueryFilters` tenant-filter bypass |
| QFS006 | DATA-20 | Raw SQL requiring tenant review |
| QFS007 | DATA-25 | Sensitive structured-log placeholders, including request/response bodies, credentials, and personal or financial identifiers |

Diagnostics are errors by default. A suppression needs an explicit review and QFace rule citation; do not use global `NoWarn` entries. The checks intentionally prefer a false positive over silently allowing a tenant or module boundary bypass.
