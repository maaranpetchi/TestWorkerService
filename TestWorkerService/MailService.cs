using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TestWorkerService.Models;
using static System.Net.WebRequestMethods;

namespace TestWorkerService
{
   

    public class EmailService:IDisposable
    {
        private bool disposedValue;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        public EmailService(IConfiguration configuration,ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendEmailAsync(
            StoredProc_Result data=null,bool IsSuccess=false,TimeSpan? exectime=null ,string ExMessage="")
        {
            try
            {
               var from= _configuration.GetSection("Smtp").GetValue<string>("From");
               var subject= _configuration.GetSection("Smtp").GetValue<string>("Subject");
               var password= _configuration.GetSection("Smtp").GetValue<string>("Password");
               var host= _configuration.GetSection("Smtp").GetValue<string>("Host");
               var port= _configuration.GetSection("Smtp").GetValue<int>("Port");
               var Ssl= _configuration.GetSection("Smtp").GetValue<bool>("Ssl");
                var notify = _configuration.GetSection("Report").GetValue<string>("Notify");
                var CC = _configuration.GetSection("Report").GetValue<string>("CC");

               
                string body = getMailTemplate(IsSuccess, data?.RowAffected??0, data?.NewEmployees??0, DateTime.Now.ToString(), exectime??TimeSpan.Zero, ExMessage);
                var message = new MailMessage();
                message.To.Add(new MailAddress(notify)); // Recipient's email address
                message.From = new MailAddress(from); // Sender's email address
                foreach (var item in CC.Split('|'))
                {
                    message.CC.Add(item);
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = from, // Sender's email address
                        Password = password// Sender's email password
                    };

                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = credential;
                    smtp.Host = host; // SMTP server address
                    smtp.Port = port; // Port for SMTP server
                    smtp.EnableSsl = Ssl;

                    await smtp.SendMailAsync(message);
                }
                _logger.LogWarning("Sucessfully send a mail to : {0}",notify);
            }
            catch (Exception ex)
            {
                // Handle exception
                _logger.LogWarning("An error occured while sending the mail : {0}", ex.Message);
            }
        }

        private string getMailTemplate(bool isSuccess, int rowAffected, int newEmployees, string now, TimeSpan exectime ,string Exceptiondetails="")
        {
            if(isSuccess)
            {
                return  $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Owner Master Excel Report</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        background-color: #f4f4f4;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }}

                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        padding: 20px;
                        background-color: #fff;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}

                    h1, h2 {{
                        color: #333;
                    }}

                    .section {{
                        margin-bottom: 20px;
                    }}

                    .section h2 {{
                        margin-bottom: 10px;
                        font-size: 20px;
                    }}

                    .content {{
                        background-color: #f9f9f9;
                        padding: 10px;
                        border-radius: 4px;
                    }}

                    .highlight {{
                        color: #007bff;
                        font-weight: bold;
                    }}

                    .footer {{
                        margin-top: 20px;
                        font-size: 12px;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <h1>Data Report</h1>

                    <div class=""section"">
                        <h2>Summary</h2>
                        <div class=""content"">
                <h3>The Excel file has been meticulously maintained and updated up to the latest revision</h1>
            <p><span class=""highlight"">No of records :</span> {rowAffected}</p>
                            <p><span class=""highlight""> Records updated at:</span> {now}</p>
                        </div>
                    </div>

                    <div class=""footer"">
                        This email was generated automatically. Please do not reply.
                    </div>
                </div>
            </body>
            </html>";
            }
            return $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Email Report</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        background-color: #f4f4f4;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }}

                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        padding: 20px;
                        background-color: #fff;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}

                    h1, h2 {{
                        color: #333;
                    }}

                    .section {{
                        margin-bottom: 20px;
                    }}

                    .section h2 {{
                        margin-bottom: 10px;
                        font-size: 20px;
                    }}

                    .content {{
                        background-color: #f9f9f9;
                        padding: 10px;
                        border-radius: 4px;
                    }}

                    .highlight {{
                        color: #007bff;
                        font-weight: bold;
                    }}

                    .footer {{
                        margin-top: 20px;
                        font-size: 12px;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <h1>Proces Failed</h1>

                    <div class=""section"">
                        <h2>Summary</h2>
                        <div class=""content"">
                            <p><span class=""highlight"">An Error Occured</span> {Exceptiondetails}</p>
                      
                        </div>
                    </div>

                    <div class=""footer"">
                        This email was generated automatically. Please do not reply.
                    </div>
                </div>
            </body>
            </html>";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EmailService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal Task SendEmailAsync(object data, bool IsSuccess, string ExMessage)
        {
            throw new NotImplementedException();
        }
    }

}
