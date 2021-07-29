using System.Net.Http;

namespace Monitor.Clients
{
    public interface IRequest
    {
        int Get(string url);
    }

    public class Request : IRequest
    {
        private readonly HttpClient _client = new HttpClient();

        public int Get(string url)
        {
            var response = _client.GetAsync(url).Result;
            return (int)response.StatusCode;
        }
    }
}