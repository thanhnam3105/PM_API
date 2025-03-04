using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Tos.Web.Data;
using TOS.Web.Models;
using Tos.Web.Utilities;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;
using System.Data.SqlClient;
using System.Web.Http.Results;
using System.Globalization;
using static TOS.Web.Controllers.PM001_ProjectResultController;
using TOS.Web.Controllers.Helpers;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class PM000_DashboardController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        private CommonController _common;

        public PM000_DashboardController()
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
        public HttpResponseMessage GetInit([FromUri] DashboardRequest request)
        {
            try
            {
                CommonModelResponseExtend result = new CommonModelResponseExtend();
                result.lstDeparment = _context.ma_employee.Select(x => new DepartmentModelResponse { value = x.nm_position_en, name = x.nm_position_en }).Distinct().ToList();
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
        public HttpResponseMessage GetPersonPerformance([FromUri] DashboardRequest request)
        {
            try
            {
                JsonResultModel<object> resultModel = new JsonResultModel<object>();
                var json = new JObject();

                var resultMonth = _context.Database.SqlQuery<FiscalMonth>(
                    "SELECT * FROM dbo.fn_get_month_in_fiscal_year(@year)",
                    new SqlParameter("@year", request.yy_fiscal)
                ).ToList();
                resultModel.AddJProperty(ref json, resultMonth, "Month");

                var resultData = _context.Database.SqlQuery<sp_Dashboard_PersonalPerformance_select_Result>(
                    "EXEC dbo.sp_Dashboard_PersonalPerformance_select @cd_employee, @fiscal_year"
                    , CommonController.GetParam("cd_employee", request.cd_employee)
                    , CommonController.GetParam("fiscal_year", request.yy_fiscal)).ToList();
                resultModel.AddJProperty(ref json, resultData, "Data");

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }


        /// <summary>
        /// Search data Cost Price
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage GetDataCostPrice([FromUri] DashboardRequest request)
        {
            try
            {
                JsonResultModel<object> resultModel = new JsonResultModel<object>();
                var json = new JObject();

                var resultMonth = _context.Database.SqlQuery<FiscalMonth>(
                    "SELECT * FROM dbo.fn_get_month_in_fiscal_year(@year)",
                    new SqlParameter("@year", request.yy_fiscal)
                ).ToList();
                resultModel.AddJProperty(ref json, resultMonth, "Month");

                var resultData = _context.Database.SqlQuery<sp_Dashboard_AverageCostPrice_select_Result>(
                    "EXEC dbo.sp_Dashboard_AverageCostPrice_select @fiscal_year"
                    , CommonController.GetParam("fiscal_year", request.yy_fiscal)).ToList();
                resultModel.AddJProperty(ref json, resultData, "Data");

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Search data High Cost Project
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage GetDataHighCostProject([FromUri] DashboardRequest request)
        {
            try
            {
                JsonResultModel<object> resultModel = new JsonResultModel<object>();
                var json = new JObject();

                var resultData = _context.Database.SqlQuery<sp_Dashboard_HighCostProject_select_Result>(
                    "EXEC dbo.sp_Dashboard_HighCostProject_select @cd_employee"
                    , CommonController.GetParam("cd_employee", request.cd_employee)).ToList();
                resultModel.AddJProperty(ref json, resultData, "Data");

                return Request.CreateResponse(HttpStatusCode.OK, json);
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
        public HttpResponseMessage GetWeekPerformance([FromUri] DashboardRequest request)
        {
            try
            {
                DateTime today = DateTime.Now;
                CultureInfo culture = CultureInfo.CurrentCulture;
                int weekNumber = culture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var resultData = _context.Database.SqlQuery<sp_Dashboard_WeekPerformance_select_Result>(
                    "EXEC dbo.sp_Dashboard_WeekPerformance_select @week, @year"
                    , CommonController.GetParam("week", weekNumber)
                    , CommonController.GetParam("year", today.Year)).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<List<sp_Dashboard_WeekPerformance_select_Result>>(resultData));
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Search data project on going
        /// </summary>
        /// <param name="request">args</param>
        /// <returns>json</returns>
        [HttpGet]
        public HttpResponseMessage GetProjectOnGoing()
        {
            try
            {
                var result = _context.Database.SqlQuery<sp_ProjectResult_select_Result>(
                    "EXEC dbo.sp_ProjectResult_select @cd_nm_project, @cd_status, @cd_type, @yy_fiscal, @mm_sale_month_from, @mm_sale_month_to"
                    , CommonController.GetParam("cd_nm_project", null)
                    , CommonController.GetParam("cd_status", Contants.ProjectStatus.OnGoing)
                    , CommonController.GetParam("cd_type", null)
                    , CommonController.GetParam("yy_fiscal", DateTime.Now.Year)
                    , CommonController.GetParam("mm_sale_month_from", null)
                    , CommonController.GetParam("mm_sale_month_to", null)).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<List<sp_ProjectResult_select_Result>>(result));
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }
        #endregion

        #region "Model API"
        public class DashboardRequest
        {
            public string cd_employee { get; set; }
            public Nullable<decimal> yy_fiscal { get; set; }
        }

        public class CommonModelResponseExtend
        {
            public List<DepartmentModelResponse> lstDeparment { get; set; }
        }

        public class FiscalMonth
        {
            public string mm_month { get; set; }
        }

        public class DepartmentModelResponse
        {
            public string value { get; set; }
            public string name { get; set; }
            #endregion
        }
    }
}
