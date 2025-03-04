using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using TOS.Web.Models;
using TOS.Web.Utilities;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        /// <summary>
        /// Get user info
        /// </summary>
        /// <param name="cd_user"></param>
        /// <returns>user's info</returns>
        [HttpGet]
        public HttpResponseMessage GetUserInfo([FromUri] string cd_user)
        {
            try
            {
                using (ProjectManagementEntities context = new ProjectManagementEntities())
                {
                    context.Configuration.ProxyCreationEnabled = false;

                    UserInfo user = new UserInfo(cd_user);
                    var responses = user;
                    return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<object>(responses));
                }
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<string>(ex.Message));
            }
        }
    }

    public class UserInfo
    {
        public string cd_role { get; set; }
        public string cd_page { get; set; }
        public List<string> cd_permission { get; set; }
        public UserInfo() { }
        public UserInfo(string cd_user)
        {
            this.cd_role = GetRole(cd_user);
        }

        private string GetRole(string cd_user)
        {
            string result = "";

            using (ProjectManagementEntities context = new ProjectManagementEntities())
            {
                result = context.ma_employee.Join(
                    context.ma_employee,
                    user => user.cd_employee,
                    role => role.kbn_role,
                    (user, role) => new { user, role }
                )
                .Where(x => x.user.cd_employee == cd_user)
                .Select(x => x.role.kbn_role)
                .FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// ユーザー名を取得します
        /// </summary>
        /// <param name="identity">実行ユーザー情報</param>
        /// <returns>ユーザー名</returns>
        public static string GetUserNameFromIdentity(IIdentity identity)
        {
            string name = identity.Name;
            int separator = name.IndexOf("\\");
            return (separator > -1) ? name.Substring(separator + 1, name.Length - separator - 1) : name;
        }
    }
}
