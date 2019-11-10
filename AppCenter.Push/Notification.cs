using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AppCenter.Push
{
    public class Notification
    {

        private string _accessToken { get; set; }
        private string _gateWay { get; set; }


        public Notification(string OwnerName, string AppName, string AccessToken)
        {
            _accessToken = AccessToken;
            _gateWay = $"https://api.appcenter.ms/v0.1/apps/{OwnerName}/{AppName}/push/notifications";

        }


       


        /// <summary>
        /// ارسال نوتیفیکشن همگانی
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Body"></param>
        /// <param name="PayLoads"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendBroadCastAsync(string Title, string Body, Dictionary<string, string> PayLoads = null)
        {

            StringContent content = ConvertToStringContent(Title, Body, PayLoads, NotificationType.broadcast_target);
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), _gateWay))
                {
                    AddHeaders(request);
                    request.Content = content;
                    var response = await httpClient.SendAsync(request);
                    return response;
                }
            }
        }



        /// <summary>
        /// ارسال نوتیفیکشن به کاربران مشخص
        /// </summary>
        /// <param name="PushIds"></param>
        /// <param name="Title"></param>
        /// <param name="Body"></param>
        /// <param name="PayLoads"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendToUsersAsync(List<string> PushIds, string Title, string Body, Dictionary<string, string> PayLoads = null)
        {
            if (PushIds.Count == 0) return null;
            var data = ConvertToStringContent(Title, Body, PayLoads, NotificationType.devices_target, PushIds);
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), _gateWay))
                {
                    AddHeaders(request);
                    request.Content = data;
                    var response = await httpClient.SendAsync(request);
                    return response;
                }
            }
        }



        private void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("X-API-Token", _accessToken);
        }

        private static StringContent ConvertToStringContent(string title, string body, Dictionary<string, string> customdata, NotificationType Type)
        {
            string name = RandomeName();
            string CustomDatas = ConvertCustomData(customdata);
            string data = $"{{ \"notification_target\": {{\"type\": \"{Type.ToString()}\",}},\"notification_content\": {{\"name\":\"{name}\",\"title\": \"{title}\",\"body\": \"{body}\",\"custom_data\":{CustomDatas} }}}}";
            return new StringContent(data, Encoding.UTF8, "application/json");
        }

        private static StringContent ConvertToStringContent(string title, string body, Dictionary<string, string> customdata, NotificationType Type, List<string> PushIds)
        {
            string name = RandomeName();
            string Pushes = ConvertPushes(PushIds);
            string CustomDatas = ConvertCustomData(customdata);
            string data = $"{{  \"notification_target\": {{\"type\": \"{Type.ToString()}\",    \"devices\": {Pushes}   }},  \"notification_content\": {{    \"name\": \"{name}\",    \"title\": \"{title}\",    \"body\": \"{body}\" ,   \"custom_data\":{CustomDatas} }}}}";
            return new StringContent(data, Encoding.UTF8, "application/json");
        }

        private static string ConvertCustomData(Dictionary<string, string> customdata)
        {
            if (customdata != null)
            {
                string data = "{";
                foreach (var item in customdata)
                {
                    data += $"\"{item.Key}\":";
                    data += $"\"{item.Value}\",";


                }
                data = data.TrimEnd(',');
                data += "}";
                return data;
            }
            return "";
        }

        private static string ConvertPushes(List<string> pushIds)
        {
            string Pushes = "[";
            foreach (var item in pushIds)
            {
                Pushes += $"\"{item}\",";
            }
            Pushes = Pushes.TrimEnd(',');
            Pushes += "]";
            return Pushes;
        }

        private static string RandomeName()
        {
            string name = "Transaction";
            Random r = new Random();
            name += $"_{r.Next(123, 987)}";
            return name;
        }



    }

    public enum NotificationType
    {
        /// <summary>
        /// همگانی
        /// </summary>
        broadcast_target,
        /// <summary>
        /// دیوایس های مشخص شده با آیدی
        /// </summary>
        devices_target,



    }
}
