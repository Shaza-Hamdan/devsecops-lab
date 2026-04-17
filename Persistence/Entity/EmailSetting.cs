namespace Registration.Persistence
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }          //This is the port number used for connecting to the SMTP server.
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; } //This specifies the email address that will appear as the sender when the email is received.
    }
}