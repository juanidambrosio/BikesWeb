using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BikeSharing_DashBoardSite.Startup))]
namespace BikeSharing_DashBoardSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
