using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Text;
using TOS.Web.Models;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;

namespace TOS.Web
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            app.UseJwtBearerAuthentication(
            new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = GetIssuer(), //some string, normally web url,  
                    ValidAudience = GetAudience(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecurityKey()))
                }
            });
        }

        /// <summary>
        /// Get issuer
        /// </summary>
        /// <returns></returns>
        public static string GetIssuer()
        {
            return Properties.Settings.Default.JwtIssuer;
        }

        /// <summary>
        /// Get audience
        /// </summary>
        /// <returns></returns>
        public static string GetAudience()
        {
            return Properties.Settings.Default.JwtIssuer;
        }

        /// <summary>
        /// Get security key
        /// </summary>
        /// <returns></returns>
        public static string GetSecurityKey()
        {
            return Properties.Settings.Default.JwtKey;
        }

        /// <summary>
        /// Hash key
        /// </summary>
        /// <returns></returns>
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            var issuerSigningKey = GetSecurityKey();
            byte[] data = Encoding.UTF8.GetBytes(issuerSigningKey);

            var result = new SymmetricSecurityKey(data);
            return result;
        }
    }
}
