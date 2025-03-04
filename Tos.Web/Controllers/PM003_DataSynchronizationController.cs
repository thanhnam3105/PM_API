using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Tos.Web.Controllers.APISyncData;
using Tos.Web.Utilities;
using TOS.Web.Controllers.Helpers;
using TOS.Web.Models;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    [AllowAnonymous]
    public class PM003_DataSynchronizationController : ApiController
    {

        #region "API Data Sync"

        /// <summary>
        /// Get data from tosvnredmine on clic button Sync
        /// </summary>
        /// <param name="request">request</param>
        /// <returns> </returns>
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] DataSyncRequest request)
        {
            SyncData(request);
            return Get();
        }

        /// <summary>
        /// Sync data
        /// </summary>
        /// <param name="request"></param>
        [AllowAnonymous]
        public void SyncData(DataSyncRequest request)
        {
            using (ProjectManagementEntities context = new ProjectManagementEntities())
            {
                if (request.flg_run_batch == true)
                {
                    Logger.App.Info("Start sync data with batch at: " + DateTime.Now);
                }

                string errorList = "";
                context.Configuration.LazyLoadingEnabled = false;
                IObjectContextAdapter adapter = context as IObjectContextAdapter;
                DbConnection connection = adapter.ObjectContext.Connection;
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        bool isRedmine = request.cd_type_data == Contants.DataSyncType.RedmineTime ||
                                            request.cd_type_data == Contants.DataSyncType.RedmineIdentifier ||
                                            request.cd_type_data == Contants.DataSyncType.RedmineBugRequest;

                        if (request.cd_type_data == Contants.DataSyncType.Attendance)
                        {
                            ProcessAttendance(request, context, ref errorList);
                        }
                        else if (isRedmine)
                        {
                            ProcessAllRedmine(request, context, ref errorList);
                        }

                        if (request.flg_run_batch == true)
                        {
                            Logger.App.Info("Finished sync data with batch at: " + DateTime.Now);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.App.Error(ex.Message, ex);
                        APIWriteLog writeLog = new APIWriteLog();
                        writeLog.InsertLog(Contants.APIStatus.Failed, ex.Message, request.cd_type_data);
                        transaction.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Sync all data Attendance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        public void ProcessAttendance(DataSyncRequest request, ProjectManagementEntities context, ref string errorList)
        {
            APIWriteLog writeLog = new APIWriteLog();
            // Process each API individually with error handling

            try
            {
                SyncAttendanceData(request, writeLog);
            }
            catch (Exception ex)
            {
                // Log the error for Attendance API
                string msg = "Attendance Time Sync Failed: " + ex.Message;
                writeLog.InsertLog(Contants.APIStatus.Failed, msg, request.cd_type_data);
                errorList += "<br/>" + msg;
                SendMailNotify(errorList);
            }
        }

        /// <summary>
        /// Sync all data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        public void ProcessAllRedmine(DataSyncRequest request, ProjectManagementEntities context, ref string errorList)
        {
            // Initialize necessary objects
            ChangeSet<tr_api_data> changeset = new ChangeSet<tr_api_data>();
            APIWriteLog writeLog = new APIWriteLog();
            ApiSync api = new ApiSync();

            try
            {
                SyncRedmineBugRequest(request, context, writeLog, api);
                SyncRedmineUsersRequest(request, context, writeLog, api);
                SyncRedmineProjects(request, context, writeLog, api);
                SyncRedmineMemberShipRequest(request, context, writeLog, api);
                SyncRedmineTimeLog(request, context, writeLog, api);
                // Log the success of the operation
                writeLog.InsertLog(Contants.APIStatus.Successed, "Redmine sync data success", request.cd_type_data);
            }
            catch (Exception ex)
            {
                // Log the error for Redmine Time Log API
                string msg = "Time Log Sync Failed: " + ex.Message;
                writeLog.InsertLog(Contants.APIStatus.Failed, msg, request.cd_type_data);
                errorList += "<br/>" + msg;
                //Send mail if error
                SendMailNotify(errorList);
            }
        }

        /// <summary>
        /// Sync Attendance Data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="writeLog"></param>
        private void SyncAttendanceData(DataSyncRequest request, APIWriteLog writeLog)
        {
            if (Properties.Settings.Default.SyncAllData == "1")
            {
                // Process all months from 2022 to the current year
                for (int year = 2022; year <= DateTime.Now.Year; year++)
                {
                    Parallel.For(1, 13, month =>
                    {
                        GetDataAttendance(year, month.ToString("D2"));
                    });
                }
            }
            else
            {
                // Process current and previous months
                var currentDate = DateTime.Now;
                GetDataAttendance(currentDate.Year, currentDate.Month.ToString("D2"));

                var lastMonth = currentDate.AddMonths(-1);
                GetDataAttendance(lastMonth.Year, lastMonth.Month.ToString("D2"));
            }

            // Log the success of the operation
            writeLog.InsertLog(Contants.APIStatus.Successed, "Attendance sync data success", request.cd_type_data);
        }

        /// <summary>
        /// Sync Redmine TimeLog
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        /// <param name="writeLog"></param>
        /// <param name="api"></param>
        private int SyncRedmineTimeLog(DataSyncRequest request, ProjectManagementEntities context, APIWriteLog writeLog, ApiSync api)
        {
            // Fetch data from Redmine
            api.getDataSpenTime();
            var data = api.FetchDataFromApiAsync(context, request);
            request.cd_type_data = Contants.DataSyncType.RedmineTime;
            // Insert data into the database
            SaveApiData(data, request, context);
            // Update the log
            context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_TimeLog_insert @employee, @cd_project_rdm"
                                                , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity))
                                                , CommonController.GetParam("@cd_project_rdm", request.cd_project_rdm));
            context.SaveChanges();
            return 1;
        }

        /// <summary>
        /// Sync Redmine Projects
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        /// <param name="writeLog"></param>
        /// <param name="api"></param>
        private int SyncRedmineProjects(DataSyncRequest request, ProjectManagementEntities context, APIWriteLog writeLog, ApiSync api)
        {
            // Fetch project data from Redmine
            api.getDataProject();
            var data = api.FetchDataFromApiAsync(context, request);
            request.cd_type_data = Contants.DataSyncType.RedmineIdentifier;
            // Insert data into the database
            SaveApiData(data, request, context);
            context.SaveChanges();
            // Update the log
            context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_Project_insert @employee, @cd_project_rdm"
                                                , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity))
                                                , CommonController.GetParam("@cd_project_rdm", request.cd_project_rdm));
            // Fetch project data from Redmine
            api.getDataProject();
            request.isSyncPJclosed = true;
            var dataClosed = api.FetchDataFromApiAsync(context, request);
            request.cd_type_data = Contants.DataSyncType.RedmineIdentifier;
            // Insert data into the database
            SaveApiData(dataClosed, request, context);
            context.SaveChanges();
            // Update the log
            context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_Project_insert @employee, @cd_project_rdm"
                                                , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity))
                                                , CommonController.GetParam("@cd_project_rdm", request.cd_project_rdm));
            return 1;
        }

        /// <summary>
        /// Sync Redmine Bug Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        /// <param name="writeLog"></param>
        /// <param name="api"></param>
        private int SyncRedmineBugRequest(DataSyncRequest request, ProjectManagementEntities context, APIWriteLog writeLog, ApiSync api)
        {
            // Fetch bug request data from Redmine
            api.getDataBugRequest();
            var data = api.getDataAPI();
            request.cd_type_data = Contants.DataSyncType.RedmineBugRequest;
            // Insert data into the database
            SaveApiData(data, request, context);
            // Update the log
            context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_BugRequest_insert @employee, @type_bug_request, @cd_project_rdm"
                                                , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity))
                                                , CommonController.GetParam("@type_bug_request", DataSyncType.RedmineBugRequest)
                                                , CommonController.GetParam("@cd_project_rdm", request.cd_project_rdm));
            context.SaveChanges();
            return 1;
        }

        /// <summary>
        /// Sync Redmine MemberShip
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        /// <param name="writeLog"></param>
        /// <param name="api"></param>
        private int SyncRedmineMemberShipRequest(DataSyncRequest request, ProjectManagementEntities context, APIWriteLog writeLog, ApiSync api)
        {
            // Fetch membership request data from Redmine
            var projects = context.ma_project_rdm.Where(x => (Properties.Settings.Default.SyncAllData == "5")
                                                            || (Properties.Settings.Default.SyncAllData != "5"
                                                                && (request.cd_project_rdm == null || request.cd_project_rdm == x.cd_project_rdm))).ToList();
            foreach (var project in projects)
            {
                api.getDataMembership(project.cd_identifier);
                var data = api.getDataAPI();
                request.cd_type_data = Contants.DataSyncType.RedmineMemberShip;
                // Insert data into the database
                SaveApiData(data, request, context);
                // Update the log
                context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_MemberShip_insert @cd_project_rdm"
                                                    , CommonController.GetParam("@cd_project_rdm", request.cd_project_rdm));
                context.SaveChanges();
            }
            return 1;
        }

        /// <summary>
        /// Sync Redmine Users
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        /// <param name="writeLog"></param>
        /// <param name="api"></param>
        private int SyncRedmineUsersRequest(DataSyncRequest request, ProjectManagementEntities context, APIWriteLog writeLog, ApiSync api)
        {
            // Fetch users request data from Redmine
            api.getDataUsers();
            var data = api.getDataAPI();
            request.cd_type_data = Contants.DataSyncType.RedmineUsers;
            // Insert data into the database
            SaveApiData(data, request, context);
            // Update the log
            context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_Users_insert @employee"
                                                , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity)));
            context.SaveChanges();
            return 1;
        }

        /// <summary>
        /// Save Api Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="changeset"></param>
        private void SaveApiData(List<object> data, DataSyncRequest request, ProjectManagementEntities context)
        {
            // Prepare and attach data to the database context
            ChangeSet<tr_api_data> changeset = new ChangeSet<tr_api_data>();
            changeset.Created.AddRange(data.Select(x => new tr_api_data
            {
                cd_type_data = request.cd_type_data,
                dt_active = DateTime.Now,
                nm_data = x.ToString()
            }));
            changeset.AttachTo(context);

            // Commit changes to the database
            context.SaveChanges();
        }

        private void SaveApiData(string data, DataSyncRequest request, ProjectManagementEntities context)
        {
            // Prepare and attach data to the database context
            ChangeSet<tr_api_data> changeset = new ChangeSet<tr_api_data>();
            changeset.Created.Add(new tr_api_data
            {
                cd_type_data = request.cd_type_data,
                dt_active = DateTime.Now,
                nm_data = data
            });
            changeset.AttachTo(context);

            // Commit changes to the database
            context.SaveChanges();
        }

        /// <summary>
        /// Get data history table
        /// </summary>
        /// <param name="request">request</param>
        /// <returns> </returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            JsonResultModel<ApiLogResult> resultModel = new JsonResultModel<ApiLogResult>();
            try
            {
                using (ProjectManagementEntities context = new ProjectManagementEntities())
                {
                    var result = new ApiLogResult
                    {
                        Attendance = context.vw_api_log.AsEnumerable()
                            .Where(x => x.cd_type_data == Contants.DataSyncType.Attendance)
                            .OrderByDescending(x => x.dt_update)
                            .ToList(),
                        Redmine = context.vw_api_log.AsEnumerable()
                            .Where(x => x.cd_type_data != Contants.DataSyncType.Attendance)
                            .OrderByDescending(x => x.dt_update)
                            .ToList()
                    };
                    var json = resultModel.CreateSuccessJsonResult(result);
                    return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
                }
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        #endregion

        #region "Call API get data from project Attendance"
        /// <summary>
        /// Get data timelog in attendace
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public void GetDataAttendance(int yy_year, string mm_month)
        {
            using (ProjectManagementEntities context = new ProjectManagementEntities())
            {
                APIWriteLog writeLog = new APIWriteLog();
                IObjectContextAdapter adapter = context as IObjectContextAdapter;
                DbConnection connection = adapter.ObjectContext.Connection;
                connection.Open();
                
                var url = Properties.Settings.Default.API_Attendace_Url + "/api/APIAttendance?mm_month=" + mm_month + "&yy_year=" + yy_year;
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    DateTime dt_from = new DateTime(yy_year, int.Parse(mm_month), 1);
                    DateTime dt_to = new DateTime(yy_year, int.Parse(mm_month), 1).AddMonths(1).AddDays(-1);

                    DataSyncRequest request = new DataSyncRequest();
                    request.cd_type_data = Contants.DataSyncType.Attendance;
                    // Insert data into the database
                    SaveApiData(response.Content.ReadAsStringAsync().Result, request, context);

                    // Update the log
                    context.Database.ExecuteSqlCommand("EXEC sp_DataSynchronization_AttendanceLog_insert @employee, @dt_from, @dt_to"
                                            , CommonController.GetParam("@employee", UserInfo.GetUserNameFromIdentity(this.User.Identity))
                                            , CommonController.GetParam("@dt_from", dt_from)
                                            , CommonController.GetParam("@dt_to", dt_to));
                    context.SaveChanges();
                }
                else
                {
                    writeLog.InsertLog(Contants.APIStatus.Failed, "Connection fail", "1");
                }
            }
        }

        /// <summary>
        /// SendMailNotify
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public void SendMailNotify(string nm_content)
        {

            if (Properties.Settings.Default.SytemAdminEmail == null || nm_content == "")
            {
                return;
            }
            try
            {
                MailController sendMail = new MailController();
                MailController.MailRequest itemMail = new MailController.MailRequest();
                itemMail.toMail = Properties.Settings.Default.SytemAdminEmail;
                itemMail.subject = "【Project management system】Data sync new notification";
                itemMail.body = nm_content + "<br/> Please check the log to resolve.";
                sendMail.SendMail(itemMail);
            }
            catch (Exception ex)
            {
                Logger.App.Error("SEND MAIL ERROR " + ex.Message);

            }
        }

        #endregion
    }

    #region "ControllerのAPIで利用するリクエスト・レスポンスクラスです"
    public class ApiLogResult
    {
        public List<vw_api_log> Attendance { get; set; }
        public List<vw_api_log> Redmine { get; set; }
    }
    public class DataSyncRequest
    {
        public string cd_type_data { get; set; }
        public string cd_project_rdm { get; set; }
        public string dd_sync { get; set; }
        public string tm_sync { get; set; }
        public string cd_type_time { get; set; }
        public bool? flg_run_batch { get; set; }
        public bool isSyncPJclosed { get; set; }
    }

    public class ApiSync : ApiSyncDataRedmine
    {
        public List<dynamic> FetchDataFromApiAsync_VIP(ProjectManagementEntities context, ChangeSet<tr_api_data> changeset, DataSyncRequest request)
        {
            List<dynamic> allData = new List<dynamic>();
            return allData;
        }


        public List<dynamic> FetchDataFromApiAsync(ProjectManagementEntities context, DataSyncRequest request)
        {
            int offset = 0;
            int limit = 100;
            bool hasMoreData = true;
            //DateTime now = DateTime.Now;
            //string dt_from = new DateTime(now.Year, now.AddMonths(-1).Month, 1).ToString("yyyy-MM-dd");

            DateTime before = DateTime.Now.AddMonths(-1);
            string dt_from = new DateTime(before.Year, before.Month, 1).ToString("yyyy-MM-dd");
            string dt_to = DateTime.Now.ToString("yyyy-MM-dd");
            string filter = "";
            string stt = "";
            List<dynamic> allData = new List<dynamic>();

            if (Properties.Settings.Default.SyncAllData == "1")
            {
                dt_from = "";
                dt_to = "";
                if (this._category == "projects")
                {
                    filter = "";
                    if (request.isSyncPJclosed)
                    {
                        filter = "&status=5";
                    }
                }
            }
            if (this._category != "projects")
            {
                filter = String.Format("&from={0}&to={1}{2}", dt_from, dt_to, stt);
            }

            while (hasMoreData)
            {
                string requestUrl = String.Format("{0}?offset={1}&limit={2}{3}", this._url, offset, limit, filter);

                HttpResponseMessage response = _httpClient.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                {
                    hasMoreData = false;
                }

                string responseBody = response.Content.ReadAsStringAsync().Result;
                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);

                var currentBatch = jsonObject[this._category];
                if (currentBatch != null)
                {
                    allData.Add(currentBatch);
                }

                if (jsonObject.total_count != null && jsonObject.total_count <= (offset + limit))
                {
                    hasMoreData = false;
                }
                else
                {
                    offset += limit;
                }
            }

            return allData;
        }
    }

    public class sp_GetTimeWorkDayByDate_Result
    {
        public Nullable<int> cd_employee { get; set; }
        public string nm_employee { get; set; }
        public Nullable<System.DateTime> dt_working { get; set; }
        public Nullable<decimal> tm_working { get; set; }
        public Nullable<decimal> tm_overtime { get; set; }
        public Nullable<decimal> tm_paidholiday { get; set; }
    }
    #endregion
}

public partial class APIWriteLog : ApiController
{
    public APIWriteLog()
    {
    }

    public virtual void InsertLog(string cd_status, string nm_result, string cd_type_data)
    {
        using (ProjectManagementEntities context = new ProjectManagementEntities())
        {
            var identity = (ClaimsIdentity)User.Identity;

            ChangeSet<tr_api_log> changeset = new ChangeSet<tr_api_log>();
            tr_api_log itemLog = new tr_api_log();
            itemLog.cd_type_data = cd_type_data;
            itemLog.cd_status = cd_status;
            itemLog.nm_result = nm_result;

            changeset.Created.Add(itemLog);
            changeset.SetDataSaveInfo(identity);
            changeset.AttachTo(context);
            context.SaveChanges();
        }
    }
}
