namespace Worker.Models
{
    public class SignalROptions
    {
        public string AuthDomain { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public string Id { get; set; }
        public string HubUri { get; set; }
    }
}
