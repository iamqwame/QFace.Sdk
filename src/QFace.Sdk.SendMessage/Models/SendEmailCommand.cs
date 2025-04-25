namespace QFace.Sdk.SendMessage.Models;

/// <summary>
    /// Command for sending emails via the email actor system
    /// </summary>
    public class SendEmailCommand
    {
        /// <summary>
        /// List of recipient email addresses
        /// </summary>
        public List<string> ToEmails { get; private set; } = [];
        
        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; private set; }
        
        /// <summary>
        /// Email body content (for non-templated emails)
        /// </summary>
        public string Body { get; private set; }
        
        /// <summary>
        /// Email template name (for templated emails)
        /// </summary>
        public string Template { get; private set; }
        
        /// <summary>
        /// Replacement values for template placeholders
        /// </summary>
        public Dictionary<string, string> Replacements { get; private set; } = new();

        // Private constructor to enforce factory methods
        private SendEmailCommand() { }

        /// <summary>
        /// Creates a simple email command for a single recipient
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <returns>SendEmailCommand instance</returns>
        public static SendEmailCommand Create(string toEmail, string subject, string body)
        {
            return new SendEmailCommand
            {
                ToEmails = [toEmail],
                Subject = subject,
                Body = body
            };
        }

        /// <summary>
        /// Creates a simple email command for multiple recipients
        /// </summary>
        /// <param name="toEmails">List of recipient email addresses</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <returns>SendEmailCommand instance</returns>
        public static SendEmailCommand Create(List<string> toEmails, string subject, string body)
        {
            return new SendEmailCommand
            {
                ToEmails = toEmails,
                Subject = subject,
                Body = body
            };
        }

        /// <summary>
        /// Creates a templated email command for a single recipient
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="template">Email template</param>
        /// <param name="replacements">Template placeholder replacements</param>
        /// <returns>SendEmailCommand instance</returns>
        public static SendEmailCommand CreateWithTemplate(string toEmail, string subject, string template, Dictionary<string, string> replacements)
        {
            return new SendEmailCommand
            {
                ToEmails = [toEmail],
                Subject = subject,
                Template = template,
                Replacements = replacements
            };
        }

        /// <summary>
        /// Creates a templated email command for multiple recipients
        /// </summary>
        /// <param name="toEmails">List of recipient email addresses</param>
        /// <param name="subject">Email subject</param>
        /// <param name="template">Email template</param>
        /// <param name="replacements">Template placeholder replacements</param>
        /// <returns>SendEmailCommand instance</returns>
        public static SendEmailCommand CreateWithTemplate(List<string> toEmails, string subject, string template, Dictionary<string, string> replacements)
        {
            return new SendEmailCommand
            {
                ToEmails = toEmails,
                Subject = subject,
                Template = template,
                Replacements = replacements
            };
        }
    }