using System.Net.Http;
using System.Threading.Tasks;

namespace MonitorLib.Clients
{
    public interface IRequest
    {
        Task<int> Get(string url);
    }

    public class Request : IRequest
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<int> Get(string url)
        {
            var response = await _client.GetAsync(url);
            return (int)response.StatusCode;
        }
    }
}