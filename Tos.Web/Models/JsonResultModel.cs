using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace TOS.Web.Models
{
    public class JsonResultModel<T> : Controller
    {
        private readonly string _titleStatus    = "Status";
        private readonly string _titleMessage   = "Message";
        private readonly string _successMessage = "Successfully";

        #region "Create success json"
        public JObject CreateSuccessJsonResult(string nameJson = "Items")
        {
            return CreateJsonResult(HttpStatusCode.OK, _successMessage, nameJson);
        }

        public JObject CreateSuccessJsonResult(IEnumerable<T> value, string nameJson = "Items")
        {
            return CreateJsonResult(HttpStatusCode.OK, _successMessage, value, nameJson);
        }

        public JObject CreateSuccessJsonResult(T value, string nameJson = "Items")
        {
            return CreateJsonResult(HttpStatusCode.OK, _successMessage, value, nameJson);
        }

        public JObject CreateSuccessJsonResult(string message, string nameJson = "Items")
        {
            return CreateJsonResult(HttpStatusCode.OK, message, nameJson);
        }
        #endregion

        #region "Create error json"
        public JObject CreateErrorJsonResult(string message, string nameJson = "Items")
        {
            var status = HttpStatusCode.BadRequest;
            return CreateJsonResult(status, message, nameJson);
        }

        public JObject CreateErrorJsonResult(HttpStatusCode status, string message, string nameJson = "Items")
        {
            return CreateJsonResult(status, message, nameJson);
        }

        public JObject CreateErrorJsonResult(HttpStatusCode status, string message, IEnumerable<T> value, string nameJson = "Items")
        {
            return CreateJsonResult(status, message, value, nameJson);
        }

        public JObject CreateErrorJsonResult(HttpStatusCode status, string message, T value, string nameJson = "Items")
        {
            return CreateJsonResult(status, message, value, nameJson);
        }
        #endregion

        //Create json with status and empty data
        private JObject CreateJsonResult(HttpStatusCode status, string message, string nameJson = "Items")
        {
            List<string> temp = new List<string>();
            JArray jarr = JArray.FromObject(temp);
            JProperty jData = new JProperty(nameJson, jarr);

            JProperty jStatus = new JProperty(_titleStatus, status);
            JProperty jMessage = new JProperty(_titleMessage, message);
            var result = new JObject(jData, jStatus, jMessage);

            return result;
        }

        //Create json with status and normal data
        private JObject CreateJsonResult(HttpStatusCode status, string message, IEnumerable<T> value, string nameJson = "Items")
        {
            List<string> temp = new List<string>();
            JArray jarr;
            if (value != null)
            {
                jarr = JArray.FromObject(value);
            }
            else
            {
                jarr = JArray.FromObject(temp);
            }
            JProperty jData = new JProperty(nameJson, jarr);

            JProperty jStatus = new JProperty(_titleStatus, status);
            JProperty jMessage = new JProperty(_titleMessage, message);
            var result = new JObject(jData, jStatus, jMessage);

            return result;
        }

        //Create json with status and custom data
        private JObject CreateJsonResult(HttpStatusCode status, string message, T value, string nameJson = "Items")
        {
            JArray jarr;
            JProperty jData, jStatus, jMessage;
            JObject result = new JObject();

            if (value == null)
            {
                List<string> temp = new List<string>();
                jarr = JArray.FromObject(temp);
                jData = new JProperty(nameJson, jarr);
                result.Add(jData);
            }
            else
            {
                var isObject = false;
                Type typeParameterType = typeof(T);
                var listDataResponse = typeParameterType.GetProperties();
                foreach (var item in listDataResponse)
                {
                    var name = item.Name;
                    var dataItem = item.GetValue(value);
                    if (dataItem != null)
                    {
                        if (dataItem.GetType().Namespace == "System.Collections.Generic")
                        {
                            isObject = true;
                            jarr = new JArray();
                            jarr = JArray.FromObject(dataItem);

                            jData = new JProperty(name, jarr);
                            result.Add(jData);
                        }
                        else
                        {
                            isObject = false;
                            result = new JObject();
                            break;
                        }
                    }
                }

                if(!isObject)
                {
                    List<T> temp = new List<T>();
                    temp.Add(value);
                    jarr = JArray.FromObject(temp);
                    jData = new JProperty(nameJson, jarr);
                    result.Add(jData);
                }
            }

            jStatus = new JProperty(_titleStatus, status);
            jMessage = new JProperty(_titleMessage, message);
            result.Add(jStatus);
            result.Add(jMessage);

            return result;
        }

        #region "function with exist jobject"
        //Add new property for exist json with string data
        public void AddJProperty(ref JObject jOb, string value, string nameJson = "Items")
        {
            JProperty jData = new JProperty(nameJson, value);
            jOb.Add(jData);
        }

        //Add new property for exist json with object data
        public void AddJProperty(ref JObject jOb, IEnumerable<T> value, string nameJson = "Items")
        {
            List<string> temp = new List<string>();
            JArray jarr;
            if (value != null)
            {
                jarr = JArray.FromObject(value);
            }
            else
            {
                jarr = JArray.FromObject(temp);
            }

            JProperty jData = new JProperty(nameJson, jarr);
            jOb.Add(jData);
        }

        //Add new property for exist json with object data
        public void AddJProperty(ref JObject jOb, T value, string nameJson = "Items")
        {
            List<string> temp = new List<string>();
            JArray jarr;
            if (value != null)
            {
                jarr = JArray.FromObject(value);
            }
            else
            {
                jarr = JArray.FromObject(temp);
            }

            JProperty jData = new JProperty(nameJson, jarr);
            jOb.Add(jData);
        }
        #endregion
    }
}