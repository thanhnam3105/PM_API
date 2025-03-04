using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TOS.Web.Controllers.Helpers;
using Tos.Web.Utilities;
using TOS.Web.Utilities;
using TOS.Web.Models;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Tos.Web.Data;
using System.Linq;
using System.Web.Http.Results;
using System.Security.Claims;
using System.Data.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Threading.Tasks;
using System.IO;
using Tos.Web.Controllers.Helpers;
using Newtonsoft.Json;
using System.Web;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Excel;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Web.Helpers;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class MA004_ProjectMasterController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        private CommonController _common;
        const string messageConflict = "commonMsg.MS0009";
        const string messageDuplicateFile = "commonMsg.fileexist";
        const string messageDuplicate = "commonMsg.MS0013";

        public MA004_ProjectMasterController()
        {
            _context = new ProjectManagementEntities();
            ((IObjectContextAdapter)_context).ObjectContext.CommandTimeout = 0;
            _common = new CommonController();
            //_account.IsValidLogin();
        }

        #region "API"
        /// <summary>
        /// Search data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage Get([FromUri] ResponseRequest request)
        {
            try
            {
                JsonResultModel<object> resultModel = new JsonResultModel<object>();
                var json = new JObject();
                getDataInit(ref resultModel, ref json, request);

                // click search || edit item
                if(request.isSearch || request.isEdit)
                {
                    List<sp_ProjectMaster_select_Result> dataDetail = findData(request);
                    resultModel.AddJProperty(ref json, dataDetail, "dataDetail");
                }
                return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Create data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ResponseRequest request)
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            var json = new JObject();
            IObjectContextAdapter adapter = _context as IObjectContextAdapter;
            DbConnection connection = adapter.ObjectContext.Connection;
            connection.Open();
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    ChangeSet<ma_project> changeSetProject = new ChangeSet<ma_project>();
                    ChangeSet<tr_project_contract> changeSetContract = new ChangeSet<tr_project_contract>();

                    bool isDatabaseExists = _context.ma_project.AsNoTracking().Where(p => p.cd_project_rdm == request.projectItem.cd_project_rdm).ToList().Count > 0;

                    if (isDatabaseExists)
                    {
                        // exist PM
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(messageDuplicate));
                    }
                    else
                    {
                        changeSetProject.Created.Add(request.projectItem);
                        changeSetProject.SetDataSaveInfo(identity);
                        changeSetProject.AttachTo(_context);
                        _context.SaveChanges();

                        var projectAfterSave = _context.ma_project.Local.ToList();
                        request.contractItem.cd_project = projectAfterSave[0].cd_project;

                        changeSetContract.Created.Add(request.contractItem);
                        changeSetContract.SetDataSaveInfo(identity);
                        changeSetContract.AttachTo(_context);
                        _context.SaveChanges();

                        // set key param search
                        request.project_cd_project = projectAfterSave[0].cd_project;
                        List<sp_ProjectMaster_select_Result> dataDetail = findData(request);
                        resultModel.AddJProperty(ref json, dataDetail, "dataDetail");
                    }
                    transaction.Commit();
                    return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.App.Error(ex.Message, ex);
                    transaction.Rollback();
                    json = resultModel.CreateErrorJsonResult(messageConflict);
                    return Request.CreateResponse(HttpStatusCode.Conflict, json, Configuration.Formatters.JsonFormatter);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
                }
            }
        }

        /// <summary>
        /// Update data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody] ResponseRequest request)
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            var json = new JObject();
            IObjectContextAdapter adapter = _context as IObjectContextAdapter;
            DbConnection connection = adapter.ObjectContext.Connection;
            connection.Open();
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {

                    var identity = (ClaimsIdentity)User.Identity;
                    ChangeSet<ma_project> changeSetProject = new ChangeSet<ma_project>();
                    ChangeSet<tr_project_contract> changeSetContract = new ChangeSet<tr_project_contract>();

                    // check exist PM 
                    bool isDatabaseExists = _context.ma_project.AsNoTracking().Count(p => p.cd_project_rdm == request.projectItem.cd_project_rdm 
                                                                                          && p.cd_project != request.projectItem.cd_project) > 0;
                    if (isDatabaseExists)
                    {
                        // exist PM
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>("commonMsg.MS0013"));
                    }
                    else
                    {
                        // set data projetct item
                        changeSetProject.Updated.Add(request.projectItem);
                        changeSetProject.SetDataSaveInfo(identity);
                        changeSetProject.AttachTo(_context);

                        // set data contract item
                        changeSetContract.Updated.Add(request.contractItem);
                        changeSetContract.SetDataSaveInfo(identity);
                        changeSetContract.AttachTo(_context);

                        _context.SaveChanges();
                    }
                    transaction.Commit();

                    // Search data after save
                    List<sp_ProjectMaster_select_Result> dataDetail = findData(request);
                    resultModel.AddJProperty(ref json, dataDetail, "dataDetail");
                    return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.App.Error(ex.Message, ex);
                    transaction.Rollback();
                    json = resultModel.CreateErrorJsonResult(messageConflict);
                    return Request.CreateResponse(HttpStatusCode.Conflict, json, Configuration.Formatters.JsonFormatter);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
                }
            }
        }


        /// <summary>
        /// Get data init
        /// </summary>
        /// <param name="resultModel">resultModel</param>
        /// <param name="json">json</param>
        /// <returns></returns>
        public void getDataInit(ref JsonResultModel<object> resultModel, ref JObject json, ResponseRequest request)
        {
            var lstProjectStatus = new List<CommonModelResponse>(); // list project status 
            var lstProjectType = new List<CommonModelResponse>(); // list project type 
            var lstCurrentcy = new List<CommonModelResponse>(); //list currentcy
            var lstCustomer = new List<lstCustomer>(); // list customer
            // mode search
            if (request.isSearch)
            {
                lstProjectStatus = _common.GetCommonData(Contants.Category.ProjectStatus);
                lstProjectType = _common.GetCommonData(Contants.Category.ProjectType);
                lstCurrentcy = _common.GetCommonData(Contants.Category.Currency);
            }
            else if (request.isEdit)
            {
                // mode edit
                lstProjectStatus = _common.GetCommonDataWithDisabled(Contants.Category.ProjectStatus);
                lstProjectType = _common.GetCommonDataWithDisabled(Contants.Category.ProjectType);
                lstCurrentcy = _common.GetCommonDataWithDisabled(Contants.Category.Currency);
                lstCustomer = _context.ma_customer.Select(s => new lstCustomer
                {
                    value = s.cd_customer,
                    name = s.nm_customer,
                    isDisabled = s.flg_active != FlgUse.InUse
                }).ToList();
            }
            else
            {
                // mode new
                lstProjectStatus = _common.GetCommonData(Contants.Category.ProjectStatus, true);
                lstProjectType = _common.GetCommonData(Contants.Category.ProjectType, true);
                lstCurrentcy = _common.GetCommonData(Contants.Category.Currency, true);
                lstCustomer = _context.ma_customer.Select(s => new lstCustomer
                {
                    value = s.cd_customer,
                    name = s.nm_customer,
                    flg_active = s.flg_active
                }).Where(p=>p.flg_active == FlgUse.InUse).ToList();

            }

            // list project status 
            var lstLeader = _context.ma_employee.Select(p => new { value = p.cd_employee, name = p.nm_employee }).ToList();

            // list project rdm 
            var lstProjectRDM = _context.ma_project_rdm.Select(s => new {value=s.cd_project_rdm, name = s.nm_project}).ToList();

            var lstMember = (from projectMember in _context.tr_project_member
                             join project in _context.ma_project
                             on projectMember.cd_project_rdm equals project.cd_project_rdm
                             join employee in _context.ma_employee
                             on projectMember.cd_employee equals employee.cd_employee
                             select new
                             {
                                 project.cd_project,
                                 projectMember.cd_employee,
                                 projectMember.cd_project_rdm,
                                 employee.nm_employee,
                                 projectMember.nm_role,
                                 projectMember.flg_leader
                             }).Where(x => x.cd_project == request.project_cd_project).ToList();

            var lstFile = _context.tr_file.Select(s=> new {
                                                    name = s.nm_file
                                                    , url = s.nm_path
                                                    , cd_project = s.cd_project
                                                    , no_seq = s.no_seq
                                                    , nm_file = s.nm_file
                                                    , nm_path = s.nm_path
                                                }).Where(x => x.cd_project == request.project_cd_project).ToList();
            
            resultModel.AddJProperty(ref json, lstProjectStatus, "lstProjectStatus");
            resultModel.AddJProperty(ref json, lstLeader, "lstLeader");
            resultModel.AddJProperty(ref json, lstProjectType, "lstProjectType");
            resultModel.AddJProperty(ref json, lstCustomer, "lstCustomer");
            resultModel.AddJProperty(ref json, lstProjectRDM, "lstProjectRDM");
            resultModel.AddJProperty(ref json, lstCurrentcy, "lstCurrentcy");
            resultModel.AddJProperty(ref json, lstMember, "lstMember");
            resultModel.AddJProperty(ref json, lstFile, "lstFile");
        }

        /// <summary>
        /// Get data init
        /// </summary>
        /// <param name="request">ProjectMasterRequest</param>
        /// <returns></returns>
        public List<sp_ProjectMaster_select_Result> findData(ResponseRequest request) {
            return _context.Database.SqlQuery<sp_ProjectMaster_select_Result>(
                                       "EXEC dbo.sp_ProjectMaster_select " +
                                            "@cd_project" +
                                            ", @nm_project" +
                                            ", @nm_customer" +
                                            ", @cd_type" +
                                            ", @cd_leader" +
                                            ", @yy_fiscal" +
                                            ", @sale_month_from" +
                                            ", @sale_month_to" +
                                            ", @cd_status",
                                       CommonController.GetParam("cd_project", request.project_cd_project),
                                       CommonController.GetParam("nm_project", request.nm_project),
                                       CommonController.GetParam("nm_customer", request.cd_nm_customer),
                                       CommonController.GetParam("cd_type", request.cd_type),
                                       CommonController.GetParam("cd_leader", request.cd_leader_search),
                                       CommonController.GetParam("yy_fiscal", request.yy_fiscal),
                                       CommonController.GetParam("sale_month_from", request.sale_month_from),
                                       CommonController.GetParam("sale_month_to", request.sale_month_to),
                                       CommonController.GetParam("cd_status", request.cd_status)
                                   ).ToList();
        }



        [HttpPost]
        public HttpResponseMessage UploadFile()
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            IObjectContextAdapter adapter = _context as IObjectContextAdapter;
            DbConnection connection = adapter.ObjectContext.Connection;
            connection.Open();
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    var json = new JObject();

                    // get params to Request return
                    var httpContext = HttpContext.Current.Request;
                    var someValueFromPost = httpContext.Form["requestJson"];
                    var request = JsonConvert.DeserializeObject<ResponseRequest>(someValueFromPost);
                    HttpFileCollection uploads = httpContext.Files;

                    // create path to cd_project
                    string mapPath = HttpContext.Current.Server.MapPath(Properties.Settings.Default.UploadFileFolder + request.project_cd_project);

                    ChangeSet<tr_file> changeSet = new ChangeSet<tr_file>();
                    changeSet.Deleted.AddRange(request.lstFileDelete);
                    changeSet.AttachTo(_context);
                    _context.SaveChanges();

                    // Delete file to path
                    foreach (var item in changeSet.Deleted)
                    {
                        string pathDelete = mapPath + "\\" + item.nm_file;
                        if (File.Exists(pathDelete))
                        {
                            File.Delete(pathDelete);
                        }
                    }

                    // Create file to path
                    if (uploads.Count > 0)
                    {
                        for (var i = 0; i < uploads.Count; i++)
                        {
                            HttpPostedFile file = uploads[i];
                            string filename = file.FileName;
                            _common.CreateFolder(mapPath);
                            var filePath = mapPath + "\\" + filename;
                            file.SaveAs(filePath);

                            tr_file item = new tr_file();
                            item.cd_project = (long)request.project_cd_project;
                            item.nm_path = Properties.Settings.Default.UploadFileFolder + request.project_cd_project + "/" + filename; ;
                            item.nm_file = filename;
                            changeSet.Created.Add(item);

                            // check duplicate file name
                            bool isDepulicate = false;
                            var createdCount = changeSet.Created.Count(p => p.nm_file == filename);
                            var deletedCount = changeSet.Deleted.Count(p => p.nm_file == filename);
                            var isDatabaseExists = (_context.tr_file.Where(p => p.cd_project == (long)request.project_cd_project && p.nm_file == filename).ToList().Count == 1);

                            isDepulicate |= (createdCount > 1);
                            isDepulicate |= (createdCount == 1 && deletedCount == 0 && isDatabaseExists);
                            if (isDepulicate)
                            {
                                json = resultModel.CreateErrorJsonResult(messageDuplicateFile);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
                            }
                        }
                    }
                    var identity = (ClaimsIdentity)User.Identity;
                    changeSet.Deleted.Clear();
                    changeSet.SetDataSaveInfo(identity);
                    changeSet.AttachTo(_context);
                    _context.SaveChanges();
                    transaction.Commit();

                    // Search data after save
                    getDataInit(ref resultModel, ref json, request);
                    return Request.CreateResponse(HttpStatusCode.OK, json);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
                }
            }
        }

        #endregion


        #region "Model API"
        public class ProjectMasterRequest
        {
            public Nullable<long> project_cd_project { get; set; }
            public string cd_leader_search { get; set; }
            public string cd_nm_customer { get; set; }
            public string nm_project { get; set; }
            public Nullable<int> cd_type { get; set; }
            public string cd_status { get; set; }
            public Nullable<DateTime> sale_month_from { get; set; }
            public Nullable<DateTime> sale_month_to { get; set; }
            public Nullable<decimal> yy_fiscal { get; set; }
        }

        public class ResponseRequest
        {
            public ma_project projectItem { get; set; }
            public tr_project_contract contractItem { get; set; }
            public List<tr_file> lstFileDelete { get; set; }
            public bool isSearch { get; set; }
            public bool isEdit { get; set; }
            public Nullable<long> project_cd_project { get; set; }
            public string cd_leader_search { get; set; }
            public string cd_nm_customer { get; set; }
            public string nm_project { get; set; }
            public Nullable<int> cd_type { get; set; }
            public string cd_status { get; set; }
            public string sale_month_from { get; set; }
            public string sale_month_to { get; set; }
            public Nullable<decimal> yy_fiscal { get; set; }
            public ResponseRequest()
            {
                lstFileDelete = new List<tr_file>();
            }
        }

        public class lstCustomer
        {
            public string value { get; set; }
            public string name { get; set; }
            public bool? isDisabled { get; set; }
            public bool? flg_active { get; set; }
        }
        #endregion
    }
}