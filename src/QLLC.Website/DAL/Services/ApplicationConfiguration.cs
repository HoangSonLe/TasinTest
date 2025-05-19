namespace Tasin.Website.DAL.Services
{
    public class ApplicationConfiguration : IApplicationConfiguration
    {
        public string ChatHubUrl { get; set; }
        public ELASTICSConfiguration ELASTICS { get; set; }
        public WebsiteInfoConfiguration WebsiteInfo { get; set; }
        public HostUsingConfiguration HostUsing { get; set; }

    }

    public class ELASTICSConfiguration
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class WebsiteInfoConfiguration
    {
        public string NameWebsite { get; set; }
        public string SiteUITitleFooter { get; set; }
        public string SiteUILoginBackgroundUrl { get; set; }
        public string SiteUILogoUrl { get; set; }
        public string SiteUILogin { get; set; }
    }
    public class HostUsingConfiguration
    {
        public string MediaFTP { get; set; }
    }
}
