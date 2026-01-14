using System.Collections.Concurrent;
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
/// Loads templates from embedded resources first, then falls back to file system
/// </summary>
public class TemplateService(IHostEnvironment hostEnvironment, ILogger<TemplateService> logger)
    : ITemplateService
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
    private readonly ILogger<TemplateService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentDictionary<string, string> _templateCache = [];
    private static readonly Assembly _assembly = typeof(TemplateService).Assembly;

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

    /// <summary>
    /// Loads a template from embedded resources first, then falls back to file system.
    /// Templates are cached after first load regardless of source.
    /// </summary>
    /// <param name="templatePath">The relative path to the template (e.g., "Templates/Emails/WorkflowStarted.html")</param>
    /// <returns>The template content as a string</returns>
    public async Task<string> LoadTemplateAsync(string templatePath)
    {
        try
        {
            if (_templateCache.TryGetValue(templatePath, out var cachedTemplate))
            {
                _logger.LogDebug("📄 Template loaded from cache: {TemplatePath}", templatePath);
                return cachedTemplate;
            }

            string? template = null;
            string? source = null;

            var resourceNameCandidates = BuildResourceNameCandidates(templatePath);
            
            foreach (var resourceName in resourceNameCandidates)
            {
                var stream = _assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    template = await reader.ReadToEndAsync();
                    source = "embedded resource";
                    _logger.LogInformation("✅ Template loaded from embedded resource: {ResourceName} ({TemplatePath})", resourceName, templatePath);
                    break;
                }
            }

            if (template == null)
            {
                var availableResources = string.Join(", ", _assembly.GetManifestResourceNames());
                _logger.LogWarning("⚠️ Embedded resource not found for {TemplatePath}. Tried: {Candidates}. Available resources: {AvailableResources}", 
                    templatePath, string.Join(", ", resourceNameCandidates), availableResources);
                
                var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, templatePath);
                
                if (File.Exists(fullPath))
                {
                    template = await File.ReadAllTextAsync(fullPath);
                    source = "file system";
                    _logger.LogInformation("✅ Template loaded from file system: {FullPath}", fullPath);
                }
                else
                {
                    _logger.LogError("❌ Template not found in embedded resources or file system: {TemplatePath}. Full path: {FullPath}", templatePath, fullPath);
                    throw new FileNotFoundException($"Template file not found: {templatePath}");
                }
            }

            _templateCache[templatePath] = template;
            _logger.LogDebug("📦 Template cached: {TemplatePath} (source: {Source})", templatePath, source);
            
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading template: {TemplatePath}", templatePath);
            throw;
        }
    }

    /// <summary>
    /// Builds candidate resource names for embedded resource lookup.
    /// Tries multiple formats to handle different namespace configurations.
    /// </summary>
    private static string[] BuildResourceNameCandidates(string templatePath)
    {
        var defaultNamespace = _assembly.GetName().Name ?? "QimErp.Shared.Common";
        var normalizedPath = templatePath.Replace('\\', '.').Replace('/', '.');
        
        return new[]
        {
            $"{defaultNamespace}.{normalizedPath}",
            normalizedPath
        };
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