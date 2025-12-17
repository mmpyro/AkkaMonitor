using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Monitor;
using Newtonsoft.Json;
using Xunit;

namespace Monitor.IntegrationTests
{
    public class NameValidatorTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public NameValidatorTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_CreateHttpMonitor_WithInvalidName_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var monitor = new
            {
                Name = "https://wp.pl",
                Url = "http://google.com",
                Interval = 60,
                ExpectedStatusCode = 200,
                Mode = "Poke"
            };
            var content = new StringContent(JsonConvert.SerializeObject(monitor), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/v1/monitor/http", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateSlackAlert_WithInvalidName_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var alert = new
            {
                Name = "https://wp.pl",
                Url = "http://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX",
                Channel = "#general"
            };
            var content = new StringContent(JsonConvert.SerializeObject(alert), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/v1/alert/slack", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
