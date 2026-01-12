using Microsoft.Extensions.Hosting;

namespace QimErp.Shared.Common.Services;

public interface ITemplateService
{
    Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> replacements);
    Task<string> LoadTemplateAsync(string templatePath);
}

/// <summary>
/// Template service that works with both ASP.NET Core Web Host and Generic Host
/// Uses IHostEnvironment which is available in both
/// </summary>
public class TemplateService(IHostEnvironment hostEnvironment, ILogger<TemplateService> logger)
    : ITemplateService
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
    private readonly ILogger<TemplateService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Dictionary<string, string> _templateCache = [];

    public async Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> replacements)
    {
        try
        {
            var templatePath = Path.Combine("Templates", "Emails", $"{templateName}.html");
            var template = await LoadTemplateAsync(templatePath);
            
            return ReplaceTokens(template, replacements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error rendering email template: {TemplateName}", templateName);
            throw;
        }
    }

    public async Task<string> LoadTemplateAsync(string templatePath)
    {
        try
        {
            // Check cache first
            if (_templateCache.TryGetValue(templatePath, out var cachedTemplate))
            {
                _logger.LogDebug("📄 Template loaded from cache: {TemplatePath}", templatePath);
                return cachedTemplate;
            }

            var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, templatePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogError("❌ Template file not found: {FullPath}", fullPath);
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }

            var template = await File.ReadAllTextAsync(fullPath);
            
            // Cache the template
            _templateCache[templatePath] = template;
            
            _logger.LogInformation("✅ Template loaded successfully: {TemplatePath}", templatePath);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading template: {TemplatePath}", templatePath);
            throw;
        }
    }

    private static string ReplaceTokens(string template, Dictionary<string, string> replacements)
    {
        if (replacements == null || replacements.Count == 0)
            return template;

        var result = template;
        
        foreach (var replacement in replacements)
        {
            var token = $"{{{{{replacement.Key}}}}}"; // {{Key}}
            result = result.Replace(token, replacement.Value ?? string.Empty);
        }

        return result;
    }
}