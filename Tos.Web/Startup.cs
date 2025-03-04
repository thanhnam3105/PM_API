using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Owin;
using Owin;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TOS.Web.Controllers;
using TOS.Web.Utilities;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(TOS.Web.Startup))]

namespace TOS.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            ConfigureAuth(app);
        }
        
    }
}
