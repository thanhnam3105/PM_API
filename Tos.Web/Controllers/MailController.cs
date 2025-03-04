using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using TOS.Web.Utilities;

namespace TOS.Web.Controllers
{
    [Authorize]
    public class MailController : ApiController
    {
        /*
        * Api send mail
        */
        [HttpPost]
        //[Route("SendMail")]
        public HttpResponseMessage SendMail(MailRequest model)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(Properties.Settings.Default.SMTPServerName);
                mail.From = new MailAddress(Properties.Settings.Default.SMTPServerUser, Properties.Settings.Default.mailFromName);
                mail.To.Add(model.toMail);
                if (model.ccMail != "")
                {
                    mail.CC.Add(model.ccMail);
                }
                mail.Subject = model.subject;
                mail.Body = model.body;
                mail.IsBodyHtml = true;
                SmtpServer.Port = Properties.Settings.Default.SMTPServerPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Properties.Settings.Default.SMTPServerUser, Properties.Settings.Default.SMTPServerPassword);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
            catch (Exception ex)
            {
                Logger.App.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        public class MailRequest
        {
            public string toMail { get; set; }
            public string subject { get; set; }
            public string body { get; set; }
            public string ccMail { get; set; }
        }
    }
}
