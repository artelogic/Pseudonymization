using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Pseudonymization.App.App_Start.Startup))]
namespace Pseudonymization.App.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}