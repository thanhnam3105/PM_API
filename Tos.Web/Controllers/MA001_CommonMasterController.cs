using ClosedXML;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Http;
using System.Windows.Interop;
using Tos.Web.Utilities;
using TOS.Web.Controllers.Helpers;
using TOS.Web.Models;
using TOS.Web.Utilities;

namespace TOS.Web.Controllers
{
    public class MA001_CommonMasterController : ApiController
    {
        private readonly ProjectManagementEntities _context;
        const string messageConflict = "commonMsg.MS0009";
        const string messageDuplicate = "commonMsg.MS0013";

        public MA001_CommonMasterController()
        {
            _context = new ProjectManagementEntities();
            _context.Configuration.ProxyCreationEnabled = false;
        }

        #region API

        /// <summary>
        /// Get data 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            JsonResultModel<CommonMasterResponse> resultModel = new JsonResultModel<CommonMasterResponse>();
            var json = resultModel.CreateSuccessJsonResult();

            try
            {
                CommonMasterResponse data = new CommonMasterResponse();
                data.Header = _context.vw_CommonMaster_Header.OrderBy(n => n.cd_category).ToList();
                data.Body = _context.ma_common.Where(n => n.cd_category != 0).OrderBy(n => n.cd_sort).ToList();
                json = resultModel.CreateSuccessJsonResult(data);
                return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                json = resultModel.CreateErrorJsonResult(ex.Message);
                return Request.CreateResponse<JObject>(HttpStatusCode.BadRequest, json, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Create data
        /// </summary>
        /// <param name="changeSet"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ma_common data)
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
                    ChangeSet<ma_common> changeSet = new ChangeSet<ma_common>();
                    changeSet.Created.Add(data);
                    InvalidationSet<ma_common> invalidations = IsAlreadyExists(changeSet);
                    if (invalidations.Count > 0)
                    {
                        json = resultModel.CreateErrorJsonResult(messageDuplicate, "cd_common");
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
        public HttpResponseMessage Put([FromBody] ma_common data)
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
                    ChangeSet<ma_common> changeSet = new ChangeSet<ma_common>();
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
        /// Update data sort
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage UpdateSort([FromBody] ma_common data)
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
                    ChangeSet<ma_common> changeSet = new ChangeSet<ma_common>();
                    changeSet =  UpdateSortOrder(data);
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
        /// Update Sort
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private ChangeSet<ma_common> UpdateSortOrder(ma_common input)
        {
            ChangeSet<ma_common> changeSet = new ChangeSet<ma_common>();

            var items = _context.ma_common
                .Where(x => x.cd_category == input.cd_category && x.cd_common != input.cd_common)
                .OrderBy(x => x.cd_sort)
                .ToList();

            items.RemoveAll(x => x.cd_common == input.cd_common);
            items.Insert(input.cd_sort - 1, input);

            short sortOrder = 1;
            foreach (var item in items)
            {
                item.cd_sort = sortOrder++;
                changeSet.Updated.Add(item);
            }

            return changeSet;
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
                if (newValue != null)
                {
                    property.SetValue(target, newValue);
                }
            }
        }

        /// <summary>
        /// Save Data
        /// </summary>
        /// <param name="changeSet"></param>
        private void SaveData(ChangeSet<ma_common> changeSet)
        {
            var identity = (ClaimsIdentity)User.Identity;

            changeSet.SetDataSaveInfo(identity);

            foreach (var item in changeSet.Created)
            {
                short maxSort = 0;
                var cntData = _context.ma_common.Where(n => n.cd_category == item.cd_category).OrderByDescending(n => n.cd_sort).FirstOrDefault();
                if (cntData == null)
                {
                    maxSort = 1;
                }
                else
                {
                    maxSort = cntData.cd_sort;
                    maxSort++;
                }
               
                item.cd_sort = maxSort;
                UpdateEntityProperties(item, item);
                _context.ma_common.Attach(item);
                _context.Entry(item).State = EntityState.Added;
            }

            foreach (var itemChange in changeSet.Updated)
            {
                var entity = _context.ma_common.Where(n => n.cd_category == itemChange.cd_category && n.cd_common == itemChange.cd_common).FirstOrDefault();
                if (entity != null)
                {
                    UpdateEntityProperties(itemChange, entity);
                    _context.ma_common.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// IsAlreadyExists
        /// </summary>
        /// <param name="value">changeset</param>
        /// <returns></returns>
        private InvalidationSet<ma_common> IsAlreadyExists(ChangeSet<ma_common> value)
        {
            InvalidationSet<ma_common> result = new InvalidationSet<ma_common>();

            foreach (var item in value.Created)
            {
                bool isDepulicate = false;

                var createdCount = value.Created.Count(target => target.cd_category == item.cd_category && target.cd_common == item.cd_common);
                var isDatabaseExists = (_context.ma_common.Where(x => x.cd_category == item.cd_category && x.cd_common == item.cd_common).Count() > 0);

                isDepulicate |= (createdCount > 1);
                isDepulicate |= (createdCount == 1 && isDatabaseExists);

                if (isDepulicate)
                {
                    result.Add(new Invalidation<ma_common>(messageDuplicate, item, "cd_common"));
                }
            }

            return result;
        }

    #endregion

    #region ModelAPI
    public class CommonMasterResponse
        {
            public List<vw_CommonMaster_Header> Header { get; set; }
            public List<ma_common> Body { get; set; }

            public CommonMasterResponse()
            {
                Header = new List<vw_CommonMaster_Header>();
                Body = new List<ma_common>();
            }
        }
        #endregion
    }
}
