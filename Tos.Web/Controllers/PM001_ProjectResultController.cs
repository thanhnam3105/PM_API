using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Results;
using Tos.Web.Data;
using Tos.Web.Utilities;
using TOS.Web.Models;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class PM001_ProjectResultController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        private CommonController _common;

        public PM001_ProjectResultController()
        {
            _context = new ProjectManagementEntities();
            ((IObjectContextAdapter)_context).ObjectContext.CommandTimeout = 0;
            _common = new CommonController();
        }

        #region "API"
        /// <summary>
        /// Get data init
        /// </summary>
        /// <param name="resultModel">resultModel</param>
        /// <param name="json">json</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetInit([FromUri] ProjectResultRequest request)
        {
            try
            {
                CommonModelResponseExtend result = GetData(request);
                return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<CommonModelResponseExtend>(result));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.App.Error(ex.Message, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Search data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage Get([FromUri] ProjectResultRequest request)
        {
            try
            {
                var result = _context.Database.SqlQuery<sp_ProjectResult_select_Result>(
                    "EXEC dbo.sp_ProjectResult_select @cd_nm_project, @cd_status, @cd_type, @yy_fiscal, @mm_sale_month_from, @mm_sale_month_to"
                    , CommonController.GetParam("cd_nm_project", request.cd_nm_project)
                    , CommonController.GetParam("cd_status", request.cd_status)
                    , CommonController.GetParam("cd_type", request.cd_type)
                    , CommonController.GetParam("yy_fiscal", request.yy_fiscal)
                    , CommonController.GetParam("mm_sale_month_from", request.mm_sale_month_from)
                    , CommonController.GetParam("mm_sale_month_to", request.mm_sale_month_to)).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<List<sp_ProjectResult_select_Result>>(result));
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Search data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage GetDetail([FromUri] ProjectResultRequest request)
        {
            try
            {
                JsonResultModel<object> resultModel = new JsonResultModel<object>();
                var json = new JObject();

                CommonModelResponseExtend resultCommon = GetData(request);
                resultModel.AddJProperty(ref json, resultCommon.lstProjectStatus, "lstProjectStatus");

                var resultDetail = _context.Database.SqlQuery<sp_ProjectResult_Detail_select_Result>(
                "EXEC dbo.sp_ProjectResult_Detail_select @cd_project, @cm_currency"
                , CommonController.GetParam("cd_project", request.cd_project)
                , CommonController.GetParam("cm_currency", Category.Currency)).ToList();
                resultModel.AddJProperty(ref json, resultDetail, "Detail");

                var resultCost = _context.Database.SqlQuery<sp_ProjectResult_Cost_select_Result>(
                    "EXEC dbo.sp_ProjectResult_Cost_select @cd_project"
                    , CommonController.GetParam("cd_project", request.cd_project)).ToList();
                resultModel.AddJProperty(ref json, resultCost, "Costs");

                return Request.CreateResponse(HttpStatusCode.OK, json);
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
        public HttpResponseMessage Post([FromBody] ProjectResultRequest request)
        {
            try
            {
                var identity = (ClaimsIdentity)User.Identity;
                ChangeSet<tr_project_cost> changeSetCost = new ChangeSet<tr_project_cost>();

                if (request.cost.ts == null || request.cost.ts.Length == 0)
                {
                    changeSetCost.Created.Add(request.cost);
                }
                else 
                {
                    changeSetCost.Updated.Add(request.cost);
                }
                changeSetCost.SetDataSaveInfo(identity);
                changeSetCost.AttachTo(_context);
                _context.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.Conflict, new ApiErrorResult<string>(Properties.Resource.DbUpdateConcurrencyError));
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Update data
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody] ProjectResultRequest request)
        {
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

                    changeSetProject.Updated.Add(request.project);
                    changeSetContract.Updated.Add(request.contract);

                    changeSetProject.SetDataSaveInfo(identity);
                    changeSetProject.AttachTo(_context);

                    changeSetContract.SetDataSaveInfo(identity);
                    changeSetContract.AttachTo(_context);

                    _context.SaveChanges();
                    transaction.Commit();

                    return GetDetail(request);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.App.Error(ex.Message, ex);
                    transaction.Rollback();
                    return Request.CreateResponse(HttpStatusCode.Conflict, new ApiErrorResult<string>(Properties.Resource.DbUpdateConcurrencyError));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
                }
            }
        }

        public CommonModelResponseExtend GetData(ProjectResultRequest request)
        {
            var result = new CommonModelResponseExtend();
            if (!request.isEdit)
            {
                // list project status 
                result.lstProjectStatus = _common.GetCommonData(Category.ProjectStatus, null);

                // list project type 
                result.lstProjectType = _common.GetCommonData(Category.ProjectType, null);

                //list currentcy
                result.lstCurrency = _common.GetCommonData(Category.Currency, null);
            }
            else 
            {
                // list project status 
                result.lstProjectStatus = _common.GetCommonDataWithDisabled(Category.ProjectStatus, null);

                // list project type 
                result.lstProjectType = _common.GetCommonDataWithDisabled(Category.ProjectType, null);

                //list currentcy
                result.lstCurrency = _common.GetCommonDataWithDisabled(Category.Currency, null);
            }
            return result;
        }
        #endregion


        #region "Model API"
        public class ProjectResultRequest
        {
            public Boolean isEdit { get; set; }
            public string cd_project { get; set; }
            public string cd_nm_project { get; set; }
            public Nullable<int> cd_type { get; set; }
            public string cd_status { get; set; }
            public string mm_sale_month_from { get; set; }
            public string mm_sale_month_to { get; set; }
            public Nullable<decimal> yy_fiscal { get; set; }
            public ma_project project { get; set; }
            public tr_project_contract contract { get; set; }
            public tr_project_cost cost { get; set; }
        }

        public class CommonModelResponseExtend
        {
            public List<CommonModelResponse> lstProjectStatus { get; set; }
            public List<CommonModelResponse> lstProjectType { get; set; }
            public List<CommonModelResponse> lstCurrency { get; set; }
        }
        #endregion
    }
}