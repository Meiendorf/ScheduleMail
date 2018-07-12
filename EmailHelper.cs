using MessageContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleMail
{
    public static class EmailHelper
    {
        public static async Task<string> SendMailToAdmin(EmailContract cont)
        {
            var emailBody = new EmailBody()
            {
                subject = cont.Subject,
                content = cont.Content,
                project = "6",
                to = cont.Email
            };

            var client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Program.Configuration["snsEndPoint"]);
            var jsonBody = JsonConvert.SerializeObject(emailBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.Headers.Add("Authorization", Program.Configuration["snsAuthHeader"]);

            HttpResponseMessage res;

            res = await client.SendAsync(request);

            return await res.Content.ReadAsStringAsync();
        }
    }

    public class EmailBody
    {
        public string to { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public string project { get; set; }
    }
}
