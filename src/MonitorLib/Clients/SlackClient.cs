using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MonitorLib.Clients
{
    public interface ISlackClient
    {
        void PostMessage(string text, string username = null, string channel = null);
        void PostMessage(Payload payload);
    }

    public class SlackClient : ISlackClient
    {
        private readonly Uri _uri;
        private readonly Encoding _encoding = new UTF8Encoding();

        public SlackClient(string urlWithAccessToken)
        {
            _uri = new Uri(urlWithAccessToken);
        }

        public void PostMessage(string text, string username = null, string channel = null)
        {
            var payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }

        public void PostMessage(Payload payload)
        {
            var payloadJson = JsonConvert.SerializeObject(payload);
            using (WebClient client = new WebClient())
            {
                var data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(_uri, "POST", data);
                var responseText = _encoding.GetString(response);
            }
        }
    }

    public class Payload  
    {  
        [JsonProperty("channel")]  
        public string Channel { get; set; }  
    
        [JsonProperty("username")]  
        public string Username { get; set; }  
    
        [JsonProperty("text")]  
        public string Text {get; set;} 
    }
}