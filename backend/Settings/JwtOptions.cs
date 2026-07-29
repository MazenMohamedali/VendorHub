namespace VendorHub.Settings
{
    public class JwtOptions
    {
        public string SecritKey { get; set; } = string.Empty;
        public string AudienceIP { get; set; } = string.Empty;
        public string IssuerIP { get; set; } = string.Empty;
        public string BaseImagesUrl => $"{IssuerIP.TrimEnd('/')}/Images";
    }
}
