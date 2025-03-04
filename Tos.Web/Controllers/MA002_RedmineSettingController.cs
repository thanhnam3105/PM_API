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

namespace TOS.Web.Controllers
{
    public class MA002_RedmineSettingController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        public MA002_RedmineSettingController()
        {
            _context = new ProjectManagementEntities();
            _context.Configuration.ProxyCreationEnabled = false;
        }

        #region API

        /// <summary>
        /// Get data Employee
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            try
            {
                var data = (from rdm in _context.ma_employee_rdm

                             join em in _context.ma_employee 
                             on rdm.cd_employee equals em.cd_employee into leftEmployee
                             from em in leftEmployee.DefaultIfEmpty()

                             orderby rdm.cd_employee
                             select new
                             {
                                 rdm.cd_employee,
                                 rdm.cd_redmine,
                                 em.nm_employee
                             }).ToList();
                var json = resultModel.CreateSuccessJsonResult(data);
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
       
        #endregion
    }
}
