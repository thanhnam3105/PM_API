using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tos.Web.Data;
using TOS.Web.Models;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.MA004_ProjectMasterController;

namespace TOS.Web.Controllers
{
    public class PM002_WorkingPerformanceController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        public PM002_WorkingPerformanceController()
        {
            _context = new ProjectManagementEntities();
            _context.Configuration.ProxyCreationEnabled = false;
        }

        #region API

        /// <summary>
        /// Get data Working Performance
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get([FromUri] WorkingPerformanceRequest request)
        {
            JsonResultModel<sp_WorkingPerformance_select_Result> resultModel = new JsonResultModel<sp_WorkingPerformance_select_Result>();
            
            try
            {

                var json = resultModel.CreateSuccessJsonResult();
                List<sp_WorkingPerformance_select_Result> dataDetail = new List<sp_WorkingPerformance_select_Result>();
                dataDetail = _context.Database.SqlQuery<sp_WorkingPerformance_select_Result>(
                                   "EXEC dbo.sp_WorkingPerformance_select " +
                                        "@yy_fiscal" +
                                        ", @cd_nm_employee",
                                   CommonController.GetParam("yy_fiscal", request.yy_fiscal),
                                   CommonController.GetParam("cd_nm_employee", request.cd_nm_employee)
                               ).ToList();
                resultModel.AddJProperty(ref json, dataDetail, "Data");
                return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        #endregion

        #region ModelAPI
        public class WorkingPerformanceRequest
        {
            public decimal? yy_fiscal { get; set; }
            public string cd_nm_employee { get; set; }
        }
        #endregion
    }
}
