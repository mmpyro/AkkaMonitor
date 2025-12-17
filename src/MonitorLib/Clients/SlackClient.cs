using System;
using System.Collections.Generic;
using System.Net.Http;
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
        private static readonly HttpClient _httpClient = new HttpClient();

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
            var contentEntries = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("payload", payloadJson)
            };

            using (var content = new FormUrlEncodedContent(contentEntries))
            {
                var response = _httpClient.PostAsync(_uri, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
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