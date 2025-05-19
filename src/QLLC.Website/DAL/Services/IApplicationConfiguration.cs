namespace Tasin.Website.DAL.Services
{
    public interface IApplicationConfiguration
    {
        string ChatHubUrl { get; set; }
        ELASTICSConfiguration ELASTICS { get; set; }
        WebsiteInfoConfiguration WebsiteInfo { get; set; }
        HostUsingConfiguration HostUsing { get; set; }
    }
}
