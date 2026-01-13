namespace EMSSolution.Models
{
    public class EmailConfigurationModel
    {
        public int? id { get; set; }
        public string EmailType { get; set; } = "smtp"; // "smtp" or "outlook"

        // SMTP fields
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool SmtpSsl { get; set; }

        // Outlook fields
        public string? outlookEmail { get; set; }
        public string? outlookPassword { get; set; }
       
    }

}
