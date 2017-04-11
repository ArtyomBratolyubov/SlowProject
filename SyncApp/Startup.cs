using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SyncApp.Startup))]
namespace SyncApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
