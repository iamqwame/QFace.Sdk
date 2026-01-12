// using QimErp.Shared.Common.Database;
//
// namespace QimErp.Shared.Common.Services;
//
// public static class SeederData
// {
//     public static async Task SeedWorkflowConfigurationsAsync<TContext>(this TContext context)
//        where TContext : ApplicationDbContext<TContext>
//     {
//         // Define the entity types we want to seed
//         var entityTypesToSeed = new[] { "Employee", "Department", "Rank" };
//
//         // Check which entity types already have configurations
//         var existingEntityTypes = await context.WorkflowConfigurations
//             .Where(wc => entityTypesToSeed.Contains(wc.EntityType))
//             .Select(wc => wc.EntityType)
//             .ToListAsync();
//
//         // Filter out entity types that already have configurations
//         var missingEntityTypes = entityTypesToSeed.Except(existingEntityTypes).ToList();
//
//         if (!missingEntityTypes.Any())
//         {
//             return; // All configurations already exist
//         }
//
//         var configurations = new List<WorkflowConfiguration>();
//
//         // Employee workflow configuration
//         if (missingEntityTypes.Contains("Employee"))
//         {
//             WorkflowConfiguration? configuration = new WorkflowConfiguration();
//             configuration.Id = Guid.NewGuid();
//             configuration.EntityType = "Employee";
//             configuration.Version = 1;
//             configuration.Description = "Employee approval workflow for create and update operations";
//             configuration.TenantId = "default";
//             configuration.Configuration = new EntityWorkflowConfig
//             {
//                 EnableWorkflowForCreate = true,
//                 EnableWorkflowForUpdate = true,
//                 EnableWorkflowForDelete = true,
//                 CreateWorkflowCode = "employee-create-approval",
//                 UpdateWorkflowCode = "employee-update-approval",
//                 DeleteWorkflowCode = "employee-delete-approval",
//                 AutoSubmitOnCreate = true,
//                 PreventDirectSaveOnCreate = false, // Allow save but as drafted
//                 SignificantFieldsForUpdate =
//                 [
//                     "FirstName",
//                     "LastName",
//                     "MiddleName",
//                     "DateOfBirth",
//                     "CurrentDepartment",
//                     "CurrentRank",
//                     "CurrentOrganizationalUnit"
//                 ],
//                 CreateTriggerConditions =
//                 [
//                     new WorkflowTriggerCondition
//                     {
//                         Field = "FirstName",
//                         Operator = WorkflowOperators.NotEquals,
//                         Value = "",
//                         Description = "Employee must have a first name"
//                     }
//                 ],
//                 UpdateTriggerConditions =
//                 [
//                     new WorkflowTriggerCondition
//                     {
//                         Field = "WorkflowStatus",
//                         Operator = WorkflowOperators.Equals,
//                         Value = "NotStarted",
//                         Description = "Only trigger workflow for entities not in workflow"
//                     }
//                 ],
//                 ExcludeRoles = ["SystemAdmin", "SuperUser"],
//                 ExcludeUsers = []
//             };
//             WorkflowConfiguration workflowConfiguration = (configuration
//
//
//                 .OnCreate("system", "system@qimerp.com", "System") as WorkflowConfiguration)!;
//             workflowConfiguration.AsActive();
//             configurations.Add(
//                 workflowConfiguration
//                 );
//         }
//
//         // Department workflow configuration
//         if (missingEntityTypes.Contains("Department"))
//         {
//             WorkflowConfiguration? configuration = new WorkflowConfiguration();
//             configuration.Id = Guid.NewGuid();
//             configuration.EntityType = "Department";
//             configuration.Version = 1;
//             configuration.Description = "Department approval workflow";
//             configuration.TenantId = "default";
//             configuration.Configuration = new EntityWorkflowConfig
//             {
//                 EnableWorkflowForCreate = true,
//                 EnableWorkflowForUpdate = true,
//                 EnableWorkflowForDelete = true,
//                 CreateWorkflowCode = "department-create-approval",
//                 UpdateWorkflowCode = "department-update-approval",
//                 DeleteWorkflowCode = "department-delete-approval",
//                 AutoSubmitOnCreate = true,
//                 PreventDirectSaveOnCreate = false
//             };
//             configurations.Add((configuration
//             .OnCreate("system", "system@qimerp.com", "System") as WorkflowConfiguration)!);
//         }
//
//         // Rank workflow configuration
//         if (missingEntityTypes.Contains("Rank"))
//         {
//             WorkflowConfiguration? configuration = new WorkflowConfiguration();
//             configuration.Id = Guid.NewGuid();
//             configuration.EntityType = "Rank";
//             configuration.Version = 1;
//             configuration.Description = "Rank approval workflow";
//             configuration.TenantId = "default";
//             configuration.Configuration = new EntityWorkflowConfig
//             {
//                 EnableWorkflowForCreate = true,
//                 EnableWorkflowForUpdate = true,
//                 EnableWorkflowForDelete = true,
//                 CreateWorkflowCode = "rank-create-approval",
//                 UpdateWorkflowCode = "rank-update-approval",
//                 DeleteWorkflowCode = "rank-delete-approval",
//                 AutoSubmitOnCreate = true,
//                 PreventDirectSaveOnCreate = false
//             };
//             configurations.Add((configuration
//             .OnCreate("system", "system@qimerp.com", "System") as WorkflowConfiguration)!);
//         }
//
//         if (configurations.Any())
//         {
//             await context.WorkflowConfigurations.AddRangeAsync(configurations);
//             await context.SaveChangesAsync(CancellationToken.None);
//         }
//     }
//     
//     public static async Task SeedWorkflowTemplatesAsync(this IWorkflowAwareContext context)
//     {
//         if (await context.WorkflowTemplates.AnyAsync())
//             return;
//
//         var templates = new List<WorkflowTemplate>
//         {
//             // Employee Create Approval Template
//             (new WorkflowTemplate
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Employee Create Approval",
//                 WorkflowCode = "employee-create-approval",
//                 Description = "Approval workflow for new employee creation",
//                 Category = "Employee",
//                 TenantId = "default",
//                 Template = new WorkflowDefinition
//                 {
//                     Steps =
//                     [
//                         new WorkflowStep
//                         {
//                             Name = "HR Review",
//                             Description = "HR Manager reviews new employee details",
//                             Order = 1,
//                             ApproverRoles = ["HR_Manager", "HR_Director"],
//                             RequiredApprovals = 1,
//                             TimeoutDays = 3,
//                             IsOptional = false,
//                             OnApproval = new WorkflowStepAction
//                             {
//                                 SendNotificationTo = ["HR_Team"],
//                                 SendEmailTo = ["hr@company.com"],
//                                 NextStep = "IT_Setup",
//                                 CompleteWorkflow = false
//                             },
//                             OnRejection = new WorkflowStepAction
//                             {
//                                 SendNotificationTo = ["Employee_Creator"],
//                                 SendEmailTo = ["requester@company.com"],
//                                 CompleteWorkflow = true
//                             }
//                         },
//
//                         new WorkflowStep
//                         {
//                             Name = "IT Setup",
//                             Description = "IT team sets up employee accounts and access",
//                             Order = 2,
//                             ApproverRoles = ["IT_Manager", "System_Admin"],
//                             RequiredApprovals = 1,
//                             TimeoutDays = 2,
//                             IsOptional = false,
//                             OnApproval = new WorkflowStepAction
//                             {
//                                 SendNotificationTo = ["All_Managers"],
//                                 SendEmailTo = ["managers@company.com"],
//                                 CompleteWorkflow = true
//                             }
//                         }
//                     ],
//                     Notifications = new WorkflowNotificationSettings
//                     {
//                         OnStart = ["HR_Manager", "Employee_Creator"],
//                         OnApproval = ["HR_Team", "IT_Team"],
//                         OnRejection = ["Employee_Creator", "HR_Manager"],
//                         OnCompletion = ["All_Managers", "HR_Team"],
//                         OnTimeout = ["HR_Director", "IT_Director"],
//                         SendEmailNotifications = true,
//                         SendSmsNotifications = false
//                     },
//                     Escalation = new WorkflowEscalationSettings
//                     {
//                         Enabled = true,
//                         EscalateAfterDays = 5,
//                         EscalateTo = ["HR_Director", "General_Manager"],
//                         EscalationMessage = "Employee approval workflow has been pending for 5 days",
//                         RepeatEscalation = true,
//                         RepeatIntervalDays = 2
//                     },
//                     AutoApproval = new WorkflowAutoApprovalSettings
//                     {
//                         Enabled = true,
//                         Conditions =
//                         [
//                             new WorkflowCondition
//                             {
//                                 Field = "CurrentRank.Name",
//                                 Operator = WorkflowOperators.Equals,
//                                 Value = "Intern",
//                                 Logic = WorkflowLogicOperator.And
//                             }
//                         ],
//                         AutoApprovalReason = "Auto-approved for intern position",
//                         NotifyOnAutoApproval = ["HR_Manager"]
//                     },
//                     Timeout = new WorkflowTimeoutSettings
//                     {
//                         DefaultTimeoutDays = 7,
//                         TimeoutAction = WorkflowTimeoutAction.Escalate,
//                         TimeoutReason = "Workflow timed out after 7 days",
//                         NotifyOnTimeout = ["HR_Director", "General_Manager"]
//                     }
//                 }
//             }
//             .OnCreate("system", "system@qimerp.com", "System") as WorkflowTemplate)!,
//
//             // Employee Update Approval Template
//             (new WorkflowTemplate
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Employee Update Approval",
//                 WorkflowCode = "employee-update-approval",
//                 Description = "Approval workflow for employee information updates",
//                 Category = "Employee",
//                 TenantId = "default",
//                 Template = new WorkflowDefinition
//                 {
//                     Steps =
//                     [
//                         new WorkflowStep
//                         {
//                             Name = "Direct Manager Review",
//                             Description = "Direct manager reviews employee changes",
//                             Order = 1,
//                             ApproverRoles = ["Manager", "Team_Lead"],
//                             RequiredApprovals = 1,
//                             TimeoutDays = 2,
//                             IsOptional = false,
//                             Conditions =
//                             [
//                                 new WorkflowCondition
//                                 {
//                                     Field = "CurrentRank.Name",
//                                     Operator = WorkflowOperators.NotEquals,
//                                     Value = "CEO",
//                                     Logic = WorkflowLogicOperator.And
//                                 }
//                             ],
//                             OnApproval = new WorkflowStepAction
//                             {
//                                 SendNotificationTo = ["HR_Team"], CompleteWorkflow = true
//                             }
//                         }
//                     ],
//                     AutoApproval = new WorkflowAutoApprovalSettings
//                     {
//                         Enabled = true,
//                         Conditions =
//                         [
//                             new WorkflowCondition
//                             {
//                                 Field = "PersonalPrimaryContact.Email",
//                                 Operator = WorkflowOperators.Contains,
//                                 Value = "@company.com",
//                                 Logic = WorkflowLogicOperator.And
//                             }
//                         ],
//                         AutoApprovalReason = "Auto-approved for minor contact updates",
//                         NotifyOnAutoApproval = ["HR_Manager"]
//                     }
//                 }
//             }
//             .OnCreate("system", "system@qimerp.com", "System") as WorkflowTemplate)!,
//
//             // Department Approval Template
//             (new WorkflowTemplate
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Department Management Approval",
//                 WorkflowCode = "department-management-approval",
//                 Description = "Approval workflow for department changes",
//                 Category = "Department",
//                 TenantId = "default",
//                 Template = new WorkflowDefinition
//                 {
//                     Steps =
//                     [
//                         new WorkflowStep
//                         {
//                             Name = "Operations Review",
//                             Description = "Operations manager reviews department changes",
//                             Order = 1,
//                             ApproverRoles = ["Operations_Manager", "General_Manager"],
//                             RequiredApprovals = 1,
//                             TimeoutDays = 3,
//                             IsOptional = false,
//                             OnApproval = new WorkflowStepAction
//                             {
//                                 SendNotificationTo = ["All_Managers"],
//                                 CompleteWorkflow = true
//                             }
//                         }
//                     ]
//                 }
//             }
//             .OnCreate("system", "system@qimerp.com", "System") as WorkflowTemplate)!
//         };
//
//         foreach (WorkflowTemplate template in templates)
//         {
//             template.AsActive();
//         }
//
//         await context.WorkflowTemplates.AddRangeAsync(templates);
//         await context.SaveChangesAsync(CancellationToken.None);
//     }
// }
