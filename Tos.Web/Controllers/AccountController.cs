using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using TOS.Web.Controllers.Helpers;
using TOS.Web.Models;
using TOS.Web.Utilities;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {
        #region "Global parameter"
        private readonly ProjectManagementEntities _context;
        CommonController _common;

        public AccountController()
        {
            _context = new ProjectManagementEntities();
            _common = new CommonController();
            _context.Configuration.ProxyCreationEnabled = false;
        }

        private ApplicationUserManager _userManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }
        #endregion

        #region "API"
        /// <summary>
        /// Login request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Login([FromBody] LoginRequestVM request)
        {
            try
            {
                var resultModel = new JsonResultModel<object>();

                //// Check Authenticate user login
                //// Get info user login
                //MembershipUser user = Membership.GetUser(request.userName);
                //if (user != null)
                //{
                //    if (!Membership.ValidateUser(request.userName, request.password)) {
                //        var message = "InvalidUserIdOrPassword";
                //        if (user.IsLockedOut)
                //        {
                //            message = "IsLockedOut";
                //        }
                //        return Request.CreateResponse(HttpStatusCode.BadRequest, new Exception(message));
                //    }
                //}
                //else 
                //{
                //    var message = "InvalidUserIdOrPassword";
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, new Exception(message));
                //}

                // Get user info
                var userInfo = _context.ma_employee.Where(x => x.cd_employee == request.userName && x.flg_leaved == FlgLeave.NotLeave).FirstOrDefault();
                if (userInfo == null)
                {
                    var message = "InvalidUserIdOrPassword";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new Exception(message));
                }

                // Get list claim
                var lstClaim = CreateListClaim(request.userName);
                var userToken = CreateToken(lstClaim);

                // Save token to db
                SaveToken(request.userName, userToken, request.rememberMe);

                // Update info login account
                //user.UnlockUser();

                object dataJson = null;
                dataJson = new
                {
                    userData = new
                    {
                        userToken = userToken,
                        userCode = userInfo.cd_employee,
                        userName = userInfo.nm_employee,
                        userRole = userInfo.kbn_role,
                        userPosition = userInfo.nm_position_en,
                        userDepartment = userInfo.cd_department,
                        userNameDepartment = userInfo.nm_department_en
                    }
                };

                var json = resultModel.CreateSuccessJsonResult(dataJson);
                return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<LoginResponseVM>(ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ChangePassword([FromBody] ChangePasswordVM request)
        {
            ApplicationUser user = _userManager.Find(User.Identity.Name, request.OldPassword);

            if(user != null)
            {
                try
                {
                    IdentityResult result = _userManager.ChangePassword(user.Id, request.OldPassword, request.NewPassword);
                    if (result.Succeeded)
                    {
                        UpdateLastPasswordChanged(user.Id);
                        return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<ChangePasswordVM>());
                    }
                    else
                    {
                        var message = "InvalidUserIdOrPassword";
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new Exception(message));
                    }
                }
                catch (Exception ex)
                {
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<LoginResponseVM>(ex.Message));
                }
            } else
            {
                var message = "InvalidUserIdOrPassword";
                return Request.CreateResponse(HttpStatusCode.BadRequest, new Exception(message));
            }
        }

        /// <summary>
        /// Check valid token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage reCheckLogin([FromBody] LoginRequestVM request)
        {
            var token = Request.Headers.Authorization?.Parameter?.ToString();
            ma_employee result = IsCheckToken(token);
            if (result != null)
            {
                try
                {
                    var dataJson = reGetInfo();
                    if(dataJson != null)
                    {
                        var resultModel = new JsonResultModel<object>();
                        var json = resultModel.CreateSuccessJsonResult(dataJson);
                        return Request.CreateResponse<JObject>(HttpStatusCode.OK, json);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiSuccessResult<object>(result));
                    }
                }
                catch (Exception ex)
                {
                    Logger.App.Error(ex.Message, ex);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<LoginResponseVM>(ex.Message));
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiSuccessResult<object>(result));
        }

        /// <summary>
        /// reGet data Info user
        /// </summary>
        /// <returns></returns>
        public object reGetInfo()
        {
            var token = "";
            if (Request != null)
            {
                token = Request.Headers.Authorization?.Parameter?.ToString();
            }
            
            var cd_user = HttpContext.Current.User.Identity.Name;
            object dataJson = null;

            // Get user info
            var userInfo = _context.ma_employee.Where(x => x.cd_employee == cd_user).FirstOrDefault();

            dataJson = new
            {
                userData = new
                {
                    userToken = token,
                    userCode = userInfo.cd_employee,
                    userName = userInfo.nm_employee,
                    userRole = userInfo.kbn_role,
                    userPosition = userInfo.nm_position_en,
                    userDepartment = userInfo.cd_department,
                    userNameDepartment = userInfo.nm_department_en
                }
            };
            return dataJson;
        }

        /// <summary>
        /// Check valid token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage IsValidLogin()
        {
            var token = Request.Headers.Authorization?.Parameter?.ToString();
            var result = IsValidToken(token);
            if (!result)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiErrorResult<bool>(HttpContext.Current.User.Identity.Name));
            }
            return Request.CreateResponse(HttpStatusCode.OK, new ApiSuccessResult<bool>(result));
        }
        #endregion

        #region "Function"
        /// <summary>
        /// Get info of user in master table
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public AspNetUsers GetAspUser(string userCode)
        {
          return _context.AspNetUsers.FirstOrDefault(x => x.UserName == userCode);
        }

        public void UpdateLastPasswordChanged(string id)
        {
            var user = _context.AspNetUsers.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                user.LastPasswordChangedDate = DateTime.Now.AddMonths(Properties.Settings.Default.NumMonthChangePassword);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Create list claim
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Claim> CreateListClaim(string userId)
        {
            //Create a List of Claims, Keep claims name short    
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim("CUSTOM", "CUSTOM")
            };

            return claims;
        }

        /// <summary>
        /// Create token
        /// </summary>
        /// <param name="lstClaim"></param>
        /// <returns></returns>
        public string CreateToken(List<Claim> lstClaim)
        {
            var key = Startup.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create Security Token object by giving required parameters    
            var jwtToken = new JwtSecurityToken(
                issuer: Startup.GetIssuer(),
                audience: Startup.GetAudience(),
                expires: DateTime.Now.AddDays(1),
                claims: lstClaim,
                signingCredentials: credentials
            );

            var jwt_token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return jwt_token;
        }

        /// <summary>
        /// Save token to DB
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="userToken"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        public void SaveToken(string userCode, string userToken, bool rememberMe)
        {
            var data = _context.ma_token_key.Where(n => n.cd_employee == userCode).FirstOrDefault();
            var timeLogin = rememberMe ? Contants.LoginRemember.Remember : Contants.LoginRemember.Default;
            if (data == null)
            {
                ma_token_key value = new ma_token_key();
                value.nm_token_key = userToken;
                value.tm_end_user = timeLogin;
                value.dt_login = DateTime.Now;
                value.cd_employee = userCode;
                _context.ma_token_key.Add(value);
            }
            else
            {
                if (data.cd_employee == userCode)
                {
                    data.nm_token_key = userToken;
                    data.tm_end_user = timeLogin;
                    data.dt_login = DateTime.Now;
                }
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Check token is valid
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public bool IsValidToken(string userToken)
        {
            using (ProjectManagementEntities context = new ProjectManagementEntities())
            {
                bool result = false;
                string userName = HttpContext.Current.User.Identity.Name;

                DateTime sysDate = DateTime.Today;

                var data = (from user in context.ma_employee
                            join token in context.ma_token_key
                            on user.cd_employee equals token.cd_employee
                            where token.cd_employee == userName
                            && user.flg_leaved == FlgLeave.NotLeave
                            && token.nm_token_key == userToken
                            select token
                           ).FirstOrDefault();

                if (data != null)
                {
                    //DateTime dt_expires = DateTime.Parse(data.dt_login.ToString()).AddDays(int.Parse(data.tm_end_user.ToString())).Date;
                    //if (DateTime.Now.Date < dt_expires)
                    //{
                    //    result = true;
                    //}
                    result = true;
                }
                return result;
            }
        }

        /// <summary>
        /// Check token is valid
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public ma_employee IsCheckToken(string userToken)
        {
            using (ProjectManagementEntities context = new ProjectManagementEntities())
            {
                string userName = HttpContext.Current.User.Identity.Name;

                DateTime sysDate = DateTime.Today;

                var data = (from user in context.ma_employee
                            join token in context.ma_token_key
                            on user.cd_employee equals token.cd_employee
                            where token.cd_employee == userName
                            && user.flg_leaved == FlgLeave.NotLeave
                            && token.nm_token_key == userToken
                            select user
                           ).FirstOrDefault();
                return data;
            }
        }
        #endregion
    }

    #region "View/Model request/response"
    public class TokenClass
    {
        public string token { get; set; }
    }

    public class LoginRequestVM
    {
        public string userName { get; set; }
        public string password { get; set; }
        public bool rememberMe { get; set; }
    }

    public class LoginResponseVM
    {
        public string userToken { get; set; }
        public string userCode { get; set; }
        public string userName { get; set; }
        public string roleCode { get; set; }
        public string userImage { get; set; }
        public string userDepartment { get; set; }
        public string userNameDepartment { get; set; }
        public string userGroupInCharge { get; set; }
        public List<object> userFarm { get; set; }
        public int? LastPasswordChangedDay { get; set; }
  }

    public class ChangePasswordVM
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
    #endregion
}
