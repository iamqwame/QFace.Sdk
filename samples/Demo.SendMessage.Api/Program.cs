using QFace.Sdk.ActorSystems;
using QFace.Sdk.SendMessage.Extensions;
using QFace.Sdk.SendMessage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMessagingServices(builder.Configuration, config => {
    config.SystemName = "MessagingDemo";
});
builder.Services.AddLogging(logging => {
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging Demo API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseActorSystem();
app.MapControllers();

// ---------------- EMAIL ----------------

// Send simple email
app.MapPost("/api/email/simple", async (SimpleEmailRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.Subject))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailAsync(
        new List<string> { request.ToEmail },
        request.Subject,
        request.Body ?? "Empty email body"
    );

    return Results.Ok(new { message = "Email sending in progress" });
});

// Send templated email
app.MapPost("/api/email/template", async (TemplatedEmailRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || 
        string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Template))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailWithTemplateAsync(
        new List<string> { request.ToEmail },
        request.Subject,
        request.Template,
        request.Replacements ?? new Dictionary<string, string>()
    );

    return Results.Ok(new { message = "Templated email sending in progress" });
});

// Send bulk email
app.MapPost("/api/email/bulk", async (BulkEmailRequest request, IMessageService messageService) =>
{
    if (request?.ToEmails == null || request.ToEmails.Count == 0 || string.IsNullOrEmpty(request.Subject))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailAsync(
        request.ToEmails,
        request.Subject,
        request.Body ?? "Empty email body"
    );

    return Results.Ok(new { message = "Bulk email sending in progress" });
});

// ---------------- SMS ----------------

// Send single SMS
app.MapPost("/api/sms/send", async (SmsRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendSmsAsync(
        new List<string> { request.PhoneNumber },
        request.Message
    );

    return Results.Ok(new { message = "SMS sending in progress" });
});

// Send bulk SMS
app.MapPost("/api/sms/bulk", async (BulkSmsRequest request, IMessageService messageService) =>
{
    if (request?.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendSmsAsync(
        request.PhoneNumbers,
        request.Message
    );

    return Results.Ok(new { message = "Bulk SMS sending in progress" });
});

// ---------------- UMAT ADMISSION ----------------

// Send UMaT application acknowledgment email
app.MapPost("/api/email/application-acknowledgment", async (UMatApplicationAcknowledgmentRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.ApplicantEmail) || string.IsNullOrEmpty(request.ApplicantName) ||
        string.IsNullOrEmpty(request.ApplicationReference) || string.IsNullOrEmpty(request.ProgrammeApplied))
    {
        return Results.BadRequest(new { error = "Required fields: ApplicantEmail, ApplicantName, ApplicationReference, ProgrammeApplied" });
    }

    // Read the UMaT application acknowledgment template
    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "umat-application-acknowledgment-template.html");
    if (!File.Exists(templatePath))
    {
        return Results.Problem("Application acknowledgment email template not found");
    }

    var template = await File.ReadAllTextAsync(templatePath);

    // Prepare replacements with default values for optional fields
    var replacements = new Dictionary<string, string>
    {
        { "ApplicantName", request.ApplicantName },
        { "ApplicantEmail", request.ApplicantEmail },
        { "ApplicantPhone", request.ApplicantPhone ?? "Not provided" },
        { "ApplicationReference", request.ApplicationReference },
        { "ProgrammeApplied", request.ProgrammeApplied },
        { "ApplicationType", request.ApplicationType ?? "Undergraduate" },
        { "SubmissionDate", request.SubmissionDate ?? DateTime.Now.ToString("MMMM dd, yyyy") },
        { "ApplicationPortalLink", request.ApplicationPortalLink ?? "https://admissions.umat.edu.gh/portal" },
        { "RequirementsLink", request.RequirementsLink ?? "https://umat.edu.gh/admissions/requirements" },
        { "AdmissionsPhone", "+233 (0) 312 2004" },
        { "AdmissionsEmail", "admissions@umat.edu.gh" },
        { "WhatsAppNumber", "+233 50 123 4567" },
        { "FacebookLink", "https://facebook.com/UMatGhana" },
        { "TwitterLink", "https://twitter.com/UMatGhana" },
        { "LinkedInLink", "https://linkedin.com/school/umat-ghana" },
        { "WebsiteLink", "https://umat.edu.gh" },
        { "CurrentYear", DateTime.Now.Year.ToString() }
    };

    await messageService.SendEmailWithTemplateAsync(
        new List<string> { request.ApplicantEmail },
        $"📋 Application Received - UMaT Reference: {request.ApplicationReference}",
        template,
        replacements
    );

    return Results.Ok(new { 
        message = "Application acknowledgment email sent successfully",
        applicationReference = request.ApplicationReference,
        applicantName = request.ApplicantName
    });
});

// Send UMaT admission confirmation email
app.MapPost("/api/email/admission", async (UMatAdmissionRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.StudentEmail) || string.IsNullOrEmpty(request.StudentName) ||
        string.IsNullOrEmpty(request.StudentId) || string.IsNullOrEmpty(request.Programme))
    {
        return Results.BadRequest(new { error = "Required fields: StudentEmail, StudentName, StudentId, Programme" });
    }

    // Read the UMaT admission template
    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "umat-admission-template.html");
    if (!File.Exists(templatePath))
    {
        return Results.Problem("Admission email template not found");
    }

    var template = await File.ReadAllTextAsync(templatePath);

    // Prepare replacements with default values for optional fields
    var replacements = new Dictionary<string, string>
    {
        { "StudentName", request.StudentName },
        { "StudentId", request.StudentId },
        { "Programme", request.Programme },
        { "Department", request.Department ?? "To be assigned" },
        { "AcademicYear", request.AcademicYear ?? DateTime.Now.Year + "/" + (DateTime.Now.Year + 1) },
        { "AdmissionDate", request.AdmissionDate ?? DateTime.Now.ToString("MMMM dd, yyyy") },
        { "RegistrationDeadline", request.RegistrationDeadline ?? DateTime.Now.AddDays(30).ToString("MMMM dd, yyyy") },
        { "OrientationDate", request.OrientationDate ?? DateTime.Now.AddDays(45).ToString("MMMM dd, yyyy") },
        { "StudentPortalLink", request.StudentPortalLink ?? "https://portal.umat.edu.gh" },
        { "AdmissionsPhone", "+233 (0) 312 2004" },
        { "AdmissionsEmail", "admissions@umat.edu.gh" },
        { "StudentAffairsPhone", "+233 (0) 312 2010" },
        { "StudentAffairsEmail", "studentaffairs@umat.edu.gh" },
        { "FacebookLink", "https://facebook.com/UMatGhana" },
        { "TwitterLink", "https://twitter.com/UMatGhana" },
        { "LinkedInLink", "https://linkedin.com/school/umat-ghana" },
        { "WebsiteLink", "https://umat.edu.gh" },
        { "CurrentYear", DateTime.Now.Year.ToString() }
    };

    await messageService.SendEmailWithTemplateAsync(
        new List<string> { request.StudentEmail },
        $"🎉 Admission Confirmed - Welcome to UMaT, {request.StudentName}!",
        template,
        replacements
    );

    return Results.Ok(new { 
        message = "UMaT admission confirmation email sent successfully",
        studentId = request.StudentId,
        studentName = request.StudentName
    });
});

// ---------------- BOTH ----------------

// Send both email and SMS
app.MapPost("/api/message/both", async (CombinedMessageRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.PhoneNumber) ||
        string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendBothAsync(
        new List<string> { request.Email },
        new List<string> { request.PhoneNumber },
        request.Subject,
        request.Message
    );

    return Results.Ok(new { message = "Combined message sending in progress" });
});

// Run app
app.Run();

// ---------------- Request DTOs ----------------

public class SimpleEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class TemplatedEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Template { get; set; }
    public Dictionary<string, string> Replacements { get; set; }
}

public class BulkEmailRequest
{
    public List<string> ToEmails { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class SmsRequest
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
}

public class BulkSmsRequest
{
    public List<string> PhoneNumbers { get; set; }
    public string Message { get; set; }
}

public class CombinedMessageRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}

public class UMatApplicationAcknowledgmentRequest
{
    // Required fields
    public string ApplicantEmail { get; set; }
    public string ApplicantName { get; set; }
    public string ApplicationReference { get; set; }
    public string ProgrammeApplied { get; set; }
    
    // Optional fields with defaults
    public string ApplicantPhone { get; set; }
    public string ApplicationType { get; set; }
    public string SubmissionDate { get; set; }
    public string ApplicationPortalLink { get; set; }
    public string RequirementsLink { get; set; }
}

public class UMatAdmissionRequest
{
    // Required fields
    public string StudentEmail { get; set; }
    public string StudentName { get; set; }
    public string StudentId { get; set; }
    public string Programme { get; set; }
    
    // Optional fields with defaults
    public string Department { get; set; }
    public string AcademicYear { get; set; }
    public string AdmissionDate { get; set; }
    public string RegistrationDeadline { get; set; }
    public string OrientationDate { get; set; }
    public string StudentPortalLink { get; set; }
}



