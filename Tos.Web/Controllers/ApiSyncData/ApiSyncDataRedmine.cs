using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using TOS.Web.Properties;

namespace Tos.Web.Controllers.APISyncData
{
    public partial class ApiSyncDataRedmine : ApiSyncData
    {
        public string _url;
        public List<dynamic> _datas;
        public string _category;

        public ApiSyncDataRedmine()
        {
            this._username = Settings.Default.API_Redmine_User;
            this._password = Settings.Default.API_Redmine_Pass;
            this._baseUrl = Settings.Default.API_Redmine_Url;

            // Init Http
            initAPI(this._username, this._password);

            _datas = new List<dynamic>();
        }

        public virtual void getDataProject()
        {
            this._url = String.Format("{0}/{1}", this._baseUrl, "projects.json");
            this._category = "projects";
        }

        public virtual void getDataSpenTime()
        {
            this._url = String.Format("{0}/{1}", this._baseUrl, "time_entries.json");
            this._category = "time_entries";
        }

        public virtual void getDataSpenTime(string id_project)
        {
            this._url = String.Format("{0}/{1}/{2}", this._baseUrl, id_project, "time_entries.json");
            //this._category = "projects";
        }

        // Set url get all data bug request
        public virtual void getDataBugRequest()
        {
            this._url = String.Format("{0}/{1}", this._baseUrl, "issues.json?query_id=14");
            this._category = "issues";
        }

        // Set url get all data membership request
        public virtual void getDataMembership(string id_project)
        {
            this._url = String.Format("{0}/{1}/{2}/{3}", this._baseUrl, "projects", id_project, "memberships.json");
            this._category = "memberships";
        }

        // Set url get all data users request
        public virtual void getDataUsers()
        {
            this._url = String.Format("{0}/{1}", this._baseUrl, "users.json");
            this._category = "users";
        }

        public override List<dynamic> getDataAPI()
        {
            int offset = 0;
            int limit = 100;
            bool hasMoreData = true;
            //List<dynamic> datas = new List<dynamic>();
            while (hasMoreData)
            {
                try
                {
                    // Construct the request URL with offset and limit for pagination
                    string requestUrl = String.Format("{0}?&offset={1}&limit={2}", this._url, offset, limit);

                    // Send a GET request to the Redmine API and wait for the response
                    HttpResponseMessage response = this._httpClient.GetAsync(requestUrl).Result;

                    // Ensure the response status code indicates success; throw an exception otherwise
                    response.EnsureSuccessStatusCode();

                    // Read the response body as a string
                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the response JSON into a dynamic object
                    dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    // Add the current batch of projects to the data list
                    this._datas.Add(jsonObject[this._category]);

                    // Check if there is more data to fetch
                    if (jsonObject.total_count <= (offset + limit)) // Simple check for empty project list
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        offset += limit; // Increase offset to fetch the next set of data
                    }
                }
                catch (HttpRequestException e)
                {
                    break; // Stop the loop if an error occurs
                }
            }

            return this._datas;
        }
    }
}