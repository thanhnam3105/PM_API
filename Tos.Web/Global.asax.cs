using DocumentFormat.OpenXml.Office2016.Excel;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TOS.Web.Controllers;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // アプリケーションのスタートアップで実行するコードです
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler();

            // Declare Scheduler
            scheduler.Start();

            // Declare JobDetail
            IJobDetail jobSyncData = JobBuilder.Create<JobSyncData>()
                .WithIdentity("dailyJobSyncData", "groupSyncData")
                .Build();

            // Declare trigger daily from 0h
            ITrigger triggerSyncData = TriggerBuilder.Create()
              .WithIdentity("dailyTriggerSyncData", "groupSyncData")
              .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(Properties.Settings.Default.Auto_SyncAllData_hours, Properties.Settings.Default.Auto_SyncAllData_minute))
              //.WithCronSchedule("0/30 * 8-17 * * ?")
              .Build();

            // connect Trigger
            scheduler.ScheduleJob(jobSyncData, triggerSyncData);
            Logger.App.Info("dailyTriggerSyncData OK");

        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            string culture = "en-US";
            if (Request.UserLanguages != null)
            {
                culture = Request.UserLanguages[0];
            }
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        }

        public class JobSyncData : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var ctl = new PM003_DataSynchronizationController();

                DataSyncRequest requestATD = new DataSyncRequest();
                requestATD.flg_run_batch = true;
                requestATD.cd_type_data = DataSyncType.Attendance;
                ctl.SyncData(requestATD);

                DataSyncRequest requestRDM = new DataSyncRequest();
                requestRDM.flg_run_batch = true;
                requestRDM.cd_type_data = DataSyncType.RedmineIdentifier;
                ctl.SyncData(requestRDM);
            }
        }
    }
}
