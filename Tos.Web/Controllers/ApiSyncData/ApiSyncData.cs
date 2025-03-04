using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Tos.Web.Controllers.APISyncData
{
    public abstract class ApiSyncData
    {
        public HttpClient _httpClient;
        public string _baseUrl;
        public string _username;
        public string _password;

        public void initAPI(string user, string password){
            //Trust all certificates
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            // Init Http
            _httpClient = new HttpClient();

            // Create a byte array containing the username and password, encoded in ASCII
            var byteArray = new System.Text.ASCIIEncoding().GetBytes(user + ":" + password);

            // Set the Authorization header with the encoded credentials using Basic Authentication
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        // Method to retrieve data from the API.
        public abstract List<dynamic> getDataAPI();
    }
}