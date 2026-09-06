# Multi-Company — Phase 1.5 Classification

Reviewed checklist required before Phase 2. Verdicts below are a **mechanical default, not a
decision**. Each module owner signs their own section. Payroll and Accounting sign first.

Default is WIDEN because failing that way is recoverable: a wrongly-widened unique index permits a
collision that never happens, whereas a wrongly-left one blocks a legitimate insert in production.
The stamping default (§2.11 of the plan) fails the same direction and for the same reason.

## 1. Custom query filters — 17 sites, all in Accounting

All 17 share one shape and are manually wired as `new XConfiguration(this)`:

```csharp
public class VendorConfiguration(ITenantQueryFilterContext context) : IEntityTypeConfiguration<Vendor>
    builder.HasQueryFilter(e =>
        e.DataStatus == DataState.Active && (e.IsGlobal || e.TenantId == context.CurrentTenantId));
```

They use `DataStatus == Active`, stricter than the global filter's `!= Deleted`. That difference is
deliberate and must survive.

**Recommendation — combined interface, so no call site changes:**

```csharp
public interface IScopedQueryFilterContext : ITenantQueryFilterContext, ICompanyQueryFilterContext { }
```

`ApplicationDbContext<T>` implements it; the 17 configs change one word in their constructor
parameter; all 17 `new XConfiguration(this)` call sites stay byte-identical. This is preferable to
expression-tree surgery in the convention: explicit, reviewable in diff, and the Phase 2 test guard
("every filter body contains a CompanyId access") catches anyone who forgets one.

| Context | Configurations wired manually |
|---|---|
| `ApApplicationDbContext` | WhtCertificate, BillPayment, PurchaseOrder, Bill, GoodsReceipt, DebitMemo, Vendor, VendorCredit |
| `ArApplicationDbContext` | CreditNote, WriteOff, ReceiptBatch, ArRefund |
| `GlApplicationDbContext` | FixedAsset, DepreciationEntry, MoMoAccount, MoMoTransaction, MoMoReconciliation |

## 2. Unique indexes — 174 sites

`{TenantId, X}` unique on a company-scoped entity becomes `{TenantId, CompanyId, X}`. On a
tenant-only entity it stays. Widening is strictly more permissive, so existing data can never
violate the new constraint.

### 2a. SDK shared base configurations — highest leverage (~400 tables inherit these)

| Configuration | Columns | Scope | Verdict |
|---|---|---|---|
| `AppSettingConfiguration` | TenantId, Key | Override | **WIDEN** — without this two companies cannot both hold a value for one key |
| `EntityCodeConfigConfiguration` | TenantId, EntityType | Company | **WIDEN** — numbering per company |
| `EmployeeBaseConfiguration` | TenantId, Code | Employee home | **WIDEN** |
| `EmployeeBaseConfiguration` | TenantId, Email | Employee identity | **LEAVE** — locked decision |
| `LookupBaseConfiguration` | TenantId, LookupType, Code | Union, default `""` | **WIDEN** |
| `ChartOfAccountBaseConfiguration` | Code, TenantId | Union, default `""` | **WIDEN** |
| `CostCenterBaseConfiguration` | Code, TenantId | Company | **WIDEN** |
| `OrganizationalUnitBaseConfiguration` | Code, TenantId | Company | **WIDEN** |
| `StationBaseConfiguration` | Code, TenantId | Company | **WIDEN** |
| `TenantPluginFlagConfiguration` | TenantId, PluginKey | `ITenantWideEntity` | **LEAVE** |
| `KnowledgeChunkConfiguration` | TenantId, CollectionKey, DocumentId, ChunkIndex | ? | **REVIEW** — is a knowledge base per company or per tenant? Not covered by any locked decision. |

### 2b. Per-module inventory

#### QimErp.Payroll — 29 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `Claims` | TenantId, GradeId | WIDEN |
| `Compensation` | Code, TenantId | WIDEN |
| `Compensation` | GradeId, NotchNumber, TenantId | WIDEN |
| `Compensation` | Level, TenantId | WIDEN |
| `Compensation` | TenantId, BatchCode | WIDEN |
| `Compensation` | TenantId, Code | WIDEN |
| `Compensation` | TenantId, FromCurrency, ToCurrency, EffectiveDate | WIDEN |
| `DashboardSnapshot` | TenantId, Year, EndpointKey, FilterHash | WIDEN |
| `EmployeeBenefits` | TenantId, AdvanceCode | WIDEN |
| `EmployeeBenefits` | TenantId, Code | WIDEN |
| `EmployeeBenefits` | TenantId, LoanCode | WIDEN |
| `LookupsSync` | TenantId, Code | WIDEN |
| `PayrollCore` | Code, TenantId | WIDEN |
| `PayrollCore` | PayrollGroupId, EmployeeId, TenantId }) .HasFilter("\"LeftDate\" IS NULL" | WIDEN |
| `PayrollCore` | TenantId, Code | WIDEN |
| `PayrollCore` | TenantId, CoreHrHolidayId | WIDEN |
| `PayrollCore` | TenantId, DocumentNumber | WIDEN |
| `PayrollCore` | TenantId, Key | WIDEN |
| `PayrollCore` | TenantId, Name | WIDEN |
| `PayrollCore` | TenantId, PayrollCode | WIDEN |
| `PayrollCore` | TenantId, PayslipNumber | WIDEN |
| `PayrollCore` | TenantId, PeriodKey | WIDEN |
| `PayrollCore` | TenantId, ScheduleName | WIDEN |
| `StatutorySubmission` | TenantId, PeriodCode, Authority, FilingType | WIDEN |
| `TaxStatutory` | TenantId, CertificateNumber | WIDEN |
| `TaxStatutory` | TenantId, Code | WIDEN |

#### QimErp.Accounting — 30 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `BudgetLine` | BudgetId, AccountId, TenantId | WIDEN |
| `BudgetTemplate` | TenantId, Name | WIDEN |
| `Budget` | TenantId, BudgetNumber | WIDEN |
| `ChartOfAccount` | Code, TenantId | WIDEN |
| `Check` | TenantId, BankAccountId, CheckNumber | WIDEN |
| `ChequeBook` | TenantId, BankAccountId, ChequeNumber | WIDEN |
| `ChequeBook` | TenantId, Code | WIDEN |
| `CostCenter` | TenantId, Code | WIDEN |
| `CurrencyMaster` | CurrencyCode, TenantId | WIDEN |
| `DepreciationEntry` | TenantId, AssetId, FiscalPeriodId | WIDEN |
| `ExchangeRate` | FromCurrencyCode, ToCurrencyCode, EffectiveDate, RateType, TenantId | WIDEN |
| `FiscalPeriod` | PeriodCode, TenantId | WIDEN |
| `FiscalYear` | YearCode, TenantId | WIDEN |
| `GlAccountingPolicy` | TenantId | WIDEN |
| `GlSystemAccount` | TenantId, Slot | WIDEN |
| `IntercompanyEntity` | TenantId, EntityCode | WIDEN |
| `IntercompanyPartner` | TenantId, PartnerCode | WIDEN |
| `IntercompanyTransaction` | TenantId, ReferenceNumber | WIDEN |
| `JournalEntryBatch` | BatchNumber, TenantId | WIDEN |
| `JournalEntryLine` | LineNumber, JournalEntryId, TenantId | WIDEN |
| `JournalEntry` | EntryNumber, TenantId | WIDEN |
| `JournalEntry` | TenantId, FiscalPeriodId, SourceModule | WIDEN |
| `OrganizationalUnitCategoryField` | TenantId, OrganizationalUnitCategoryId, FieldCode | WIDEN |
| `OrganizationalUnitFieldValue` | TenantId, OrganizationalUnitId, OrganizationalUnitCategoryFieldId | WIDEN |
| `PettyCashBatch` | BatchNumber, TenantId | WIDEN |
| `RecurringJournalEntryLineTemplate` | TemplateId, LineNumber, TenantId | WIDEN |
| `RecurringJournalEntryTemplate` | TemplateName, TenantId | WIDEN |
| `TaxCode` | Code, TenantId | WIDEN |
| `TaxConfiguration` | TenantId | WIDEN |

#### QimErp.CoreHr — 53 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `AppraisalPeriod` | TenantId, Code | WIDEN |
| `AppraisalPeriod` | TenantId, No | WIDEN |
| `Certificate` | TenantId, Code | WIDEN |
| `Certificate` | TenantId, No | WIDEN |
| `CertificationDocument` | TenantId, Code | WIDEN |
| `CertificationDocument` | TenantId, No | WIDEN |
| `DevelopmentPlan` | TenantId, Code | WIDEN |
| `DevelopmentPlan` | TenantId, No | WIDEN |
| `EmployeeAppraisalPlan` | TenantId, Code | WIDEN |
| `EmployeeAppraisalPlan` | TenantId, No | WIDEN |
| `EmployeeGoal` | TenantId, Code | WIDEN |
| `EmployeeGoal` | TenantId, No | WIDEN |
| `Employee` | TenantId, Code | WIDEN |
| `Feedback360` | TenantId, Code | WIDEN |
| `Feedback360` | TenantId, No | WIDEN |
| `HeadcountPlan` | TenantId, Code | WIDEN |
| `HeadcountPlan` | TenantId, No | WIDEN |
| `JobStatus` | TenantId, Code | WIDEN |
| `LearningDashboardSnapshot` | TenantId | WIDEN |
| `LearningPath` | TenantId, Code | WIDEN |
| `LearningPath` | TenantId, No | WIDEN |
| `OffboardingTemplate` | TenantId, Code | WIDEN |
| `OnboardingTemplate` | TenantId, Code | WIDEN |
| `PerformanceImprovementPlan` | TenantId, Code | WIDEN |
| `PerformanceImprovementPlan` | TenantId, No | WIDEN |
| `PerformanceOutcome` | TenantId, Code | WIDEN |
| `PerformanceRatingScale` | TenantId, Code | WIDEN |
| `PerformanceReview` | TenantId, Code | WIDEN |
| `PerformanceReview` | TenantId, No | WIDEN |
| `ProfessionalBodySubscription` | TenantId, Code | WIDEN |
| `ProfessionalBodySubscription` | TenantId, No | WIDEN |
| `ReviewCalibration` | TenantId, Code | WIDEN |
| `ReviewCalibration` | TenantId, No | WIDEN |
| `Skill` | TenantId, Name | WIDEN |
| `SkillsGapAnalysis` | TenantId, Code | WIDEN |
| `SkillsGapAnalysis` | TenantId, No | WIDEN |
| `SponsoredStudy` | TenantId, Code | WIDEN |
| `SponsoredStudy` | TenantId, No | WIDEN |
| `Station` | TenantId, Name | WIDEN |
| `SuccessionPlan` | TenantId, Code | WIDEN |
| `SuccessionPlan` | TenantId, No | WIDEN |
| `TalentPipeline` | TenantId, Code | WIDEN |
| `TalentPipeline` | TenantId, No | WIDEN |
| `TalentReview` | TenantId, Code | WIDEN |
| `TalentReview` | TenantId, No | WIDEN |
| `TrainingNeedsAnalysis` | TenantId, Code | WIDEN |
| `TrainingNeedsAnalysis` | TenantId, No | WIDEN |
| `TrainingPaymentRequest` | TenantId, Code | WIDEN |
| `TrainingPaymentRequest` | TenantId, No | WIDEN |
| `TrainingRefundRequest` | TenantId, Code | WIDEN |
| `TrainingRefundRequest` | TenantId, No | WIDEN |
| `Transcript` | TenantId, Code | WIDEN |
| `Transcript` | TenantId, No | WIDEN |

#### QimErp.HROperations — 40 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `AccommodationWaitingList` | TenantId, Code | WIDEN |
| `AccommodationWaitingList` | TenantId, No | WIDEN |
| `Accommodation` | TenantId, Code | WIDEN |
| `Accommodation` | TenantId, No | WIDEN |
| `BenefitEnrollmentConfiguration` | TenantId, ConfigurationType | WIDEN |
| `BenefitEnrollment` | TenantId, Code | WIDEN |
| `BenefitEnrollment` | TenantId, No | WIDEN |
| `BenefitLoan` | TenantId, Code | WIDEN |
| `BenefitLoan` | TenantId, No | WIDEN |
| `BenefitPlan` | TenantId, Code | WIDEN |
| `BenefitPlan` | TenantId, No | WIDEN |
| `ComplianceAudit` | TenantId, Code | WIDEN |
| `ComplianceAudit` | TenantId, No | WIDEN |
| `CompliancePolicy` | TenantId, Code | WIDEN |
| `CompliancePolicy` | TenantId, No | WIDEN |
| `DisciplinaryCase` | TenantId, Code | WIDEN |
| `DisciplinaryCase` | TenantId, No | WIDEN |
| `Employee` | TenantId, No | WIDEN |
| `HealthIssue` | TenantId, Code | WIDEN |
| `HealthIssue` | TenantId, No | WIDEN |
| `MedicalClaimConsumption` | TenantId, SourceModule, SourceClaimId | WIDEN |
| `Recruitment` | TenantId, ApplicationId | WIDEN |
| `Recruitment` | TenantId, CandidateId, JobId | WIDEN |
| `Recruitment` | TenantId, Code | WIDEN |
| `Recruitment` | TenantId, No | WIDEN |
| `Recruitment` | TenantId, Priority | WIDEN |
| `Risk` | TenantId, Code | WIDEN |
| `Risk` | TenantId, No | WIDEN |
| `Survey` | TenantId, Code | WIDEN |
| `Template` | TenantId, Code }) .HasDatabaseName("IX_surv_template_TenantId_Code" | WIDEN |
| `TenancyAgreement` | TenantId, Code | WIDEN |
| `TenancyAgreement` | TenantId, No | WIDEN |
| `TravelPermission` | TenantId, Code | WIDEN |
| `TravelPermission` | TenantId, No | WIDEN |

#### QimErp.Operations — 8 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `Customer` | TenantId, Code | WIDEN |
| `FiscalPeriod` | TenantId, FiscalYearId, PeriodCode | WIDEN |
| `FiscalYear` | TenantId, YearCode | WIDEN |
| `ProjectBudget` | TenantId, ProjectId, BudgetNumber, Version | WIDEN |
| `ProjectContract` | TenantId, ContractNumber | WIDEN |
| `ProjectExpenditure` | TenantId, ProjectId, ExpenditureNumber | WIDEN |
| `ProjectTask` | TenantId, ProjectId, TaskNumber | WIDEN |
| `Project` | TenantId, ProjectNumber | WIDEN |

#### QimErp.Platform — 2 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `ReportDefinition` | TenantId, Key | WIDEN |
| `ReportRun` | TenantId, IdempotencyKey | WIDEN |

#### QimErp.IAM — 1 sites   ☐ signed off by ______

| Entity | Columns | Default |
|---|---|---|
| `Permissions` | TenantId, RoleId, PermissionId | WIDEN |


## Downstream migration constraint

The `CompanyId` column MUST be created as `NOT NULL DEFAULT ''`.

A nullable column makes the query filter's `AllowedCompanyIds.Contains(e.CompanyId)` evaluate to
NULL rather than true or false for every legacy row, so those rows disappear from every query the
moment the company filter goes active.

## Downstream blockers found during SDK implementation

These are NOT fixed in the SDK and must be handled when each module adopts the new version.

1. **Employee sync carries no company — highest priority.** `EmployeeChangedEvent` and the
   employee-sync Temporal activities have no `CompanyId`, so `TenantContextActivityInterceptor`
   seeds `FilterActive: false` and synced employee rows are written with no company. Until that
   event contract gains `CompanyId`, company scoping is a no-op for every synced employee row.
   Spans CoreHr, Payroll, HROperations, Operations and the Temporal event schemas.

2. **`GetByCodeAsync(code)` can now resolve the wrong company's employee.** Employee code
   uniqueness widened from `{TenantId, Code}` to `{TenantId, CompanyId, Code}`, so any lookup by
   code that does not also constrain company is ambiguous. Known sites: `QimErp.Payroll`
   `EmployeeRepository`, two import processors, and CoreHr WorkforcePlanning/Learning.

3. **Detached `Update()` with blank original and current `CompanyId` slips every guard.**
   Pre-existing shape, not introduced by this work — EF has no snapshot for a detached entity.
   Belongs on the security backlog, not this program.

4. **Every consuming solution needs its own migration** once the SDK version is packed and
   adopted. `QFace.Sdk` ships as a library and generates none.
