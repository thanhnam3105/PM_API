using DocumentFormat.OpenXml.Office2016.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Tos.Web.Data;
using Tos.Web.Utilities;
using TOS.Web.Models;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    public class MA003_CustomerMasterController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        private CommonController _common;
        const string messageConflict = "commonMsg.MS0009";
        const string messageDuplicate = "commonMsg.MS0013";

        public MA003_CustomerMasterController()
        {
            _context = new ProjectManagementEntities();
            _context.Configuration.ProxyCreationEnabled = false;
            _common = new CommonController();
        }

        #region API

        /// <summary>
        /// Get data Customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get([FromUri] CustomerRequest request)
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            try
            {
                var data = (from customer in _context.ma_customer
                            where (request.nm_customer == null || customer.nm_customer.Contains(request.nm_customer))
                            orderby customer.cd_customer
                            select customer).ToList();
                var json = resultModel.CreateSuccessJsonResult(data);
                return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }

        /// <summary>
        /// Get data init
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetInit([FromUri] CustomerRequest request)
        {
            var resultModel = new JsonResultModel<CustomerInitResponse>();
            try
            {
                CustomerInitResponse result = GetData(request);
                var json = resultModel.CreateSuccessJsonResult(result);
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                var json = resultModel.CreateErrorJsonResult(ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Update Entity Properties If Exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void UpdateEntityProperties<T>(T source, T target)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var newValue = property.GetValue(source);
                property.SetValue(target, newValue);
            }
        }

        /// <summary>
        /// Save Data
        /// </summary>
        /// <param name="changeSet"></param>
        private void SaveData(ChangeSet<ma_customer> changeSet)
        {
            var identity = (ClaimsIdentity)User.Identity;

            changeSet.SetDataSaveInfo(identity);

            foreach (var item in changeSet.Created)
            {
                UpdateEntityProperties(item, item);
                _context.ma_customer.Attach(item);
                _context.Entry(item).State = EntityState.Added;
            }

            foreach (var itemChange in changeSet.Updated)
            {
                var entity = _context.ma_customer.Where(n => n.cd_customer == itemChange.cd_customer).FirstOrDefault();
                if (entity != null)
                {
                    UpdateEntityProperties(itemChange, entity);
                    _context.ma_customer.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Create data
        /// </summary>
        /// <param name="changeSet"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ma_customer data)
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            var json = resultModel.CreateSuccessJsonResult();

            IObjectContextAdapter adapter = _context as IObjectContextAdapter;
            DbConnection connection = adapter.ObjectContext.Connection;
            connection.Open();
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    ChangeSet<ma_customer> changeSet = new ChangeSet<ma_customer>();
                    changeSet.Created.Add(data);
                    InvalidationSet<ma_customer> invalidations = IsAlreadyExists(changeSet);
                    if (invalidations.Count > 0)
                    {
                        json = resultModel.CreateErrorJsonResult(messageDuplicate, "cd_customer");
                        return Request.CreateResponse(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
                    }
                    SaveData(changeSet);
                    transaction.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "");
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
                    json = resultModel.CreateErrorJsonResult(ex.Message);
                    return Request.CreateResponse<JObject>(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
                }
            }
        }

        /// <summary>
        /// Update data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody] ma_customer data)
        {
            JsonResultModel<object> resultModel = new JsonResultModel<object>();
            var json = resultModel.CreateSuccessJsonResult();

            IObjectContextAdapter adapter = _context as IObjectContextAdapter;
            DbConnection connection = adapter.ObjectContext.Connection;
            connection.Open();
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    ChangeSet<ma_customer> changeSet = new ChangeSet<ma_customer>();
                    changeSet.Updated.Add(data);
                    SaveData(changeSet);
                    transaction.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "");
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
                    json = resultModel.CreateErrorJsonResult(ex.Message);
                    return Request.CreateResponse<JObject>(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
                }
            }
        }

        /// <summary>
        /// IsAlreadyExists
        /// </summary>
        /// <param name="value">changeset</param>
        /// <returns></returns>
        private InvalidationSet<ma_customer> IsAlreadyExists(ChangeSet<ma_customer> value)
        {
            InvalidationSet<ma_customer> result = new InvalidationSet<ma_customer>();

            foreach (var item in value.Created)
            {
                bool isDepulicate = false;

                var createdCount = value.Created.Count(target => target.cd_customer == item.cd_customer);
                var isDatabaseExists = (_context.ma_customer.Where(x => x.cd_customer == item.cd_customer).Count() > 0);

                isDepulicate |= (createdCount > 1);
                isDepulicate |= (createdCount == 1 && isDatabaseExists);

                if (isDepulicate)
                {
                    result.Add(new Invalidation<ma_customer>(messageDuplicate, item, "cd_customer"));
                }
            }

            return result;
        }

        public CustomerInitResponse GetData(CustomerRequest request)
        {
            var result = new CustomerInitResponse();
            {
                if (!request.isEdit)
                {
                    // list country
                    result.lstCountry = _common.GetCommonData(Category.Country, true);

                    // list customer type
                    result.lstTypeCustomer = _common.GetCommonData(Category.TypeCustomer, true);
                    
                }
                else
                {
                    // list country
                    result.lstCountry = _common.GetCommonDataWithDisabled(Category.Country, null);

                    // list customer type
                    result.lstTypeCustomer = _common.GetCommonDataWithDisabled(Category.TypeCustomer, null);
                }
                if(request.cd_customer != null)
                {
                    result.DataItem = _context.ma_customer.Where(n => n.cd_customer == request.cd_customer).FirstOrDefault();
                }

                return result;
            }
        }
        #endregion

        #region ModelAPI
        public class CustomerRequest
        {
            public string cd_customer { get; set; }
            public string nm_customer { get; set; }
            public bool isEdit { get; set; }
        }

        public class CustomerInitResponse
        {
            public List<CommonModelResponse> lstCountry { get; set; }
            public List<CommonModelResponse> lstTypeCustomer { get; set; }
            public ma_customer DataItem { get; set; }

            public CustomerInitResponse() {
                lstCountry = new List<CommonModelResponse>();
                lstTypeCustomer = new List<CommonModelResponse>();
                DataItem = new ma_customer();
            }
        }
        #endregion
    }
}
