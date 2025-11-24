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
    public class BasicTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public BasicTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/v1/monitor")]
        [InlineData("/api/v1/alert")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Post_CreateHttpMonitor_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var monitor = new
            {
                Name = "TestHttpMonitor",
                Url = "http://google.com",
                Interval = 60,
                ExpectedStatusCode = 200,
                Mode = "Poke"
            };
            var content = new StringContent(JsonConvert.SerializeObject(monitor), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/v1/monitor/http", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_GetMonitorInfo_ReturnsNotFound_WhenMonitorDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/monitor/nonexistent-monitor");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_GetMonitorInfo_ReturnsOk_WhenMonitorExists()
        {
            // Arrange
            var client = _factory.CreateClient();
            var monitorName = "TestMonitorForGet";
            var monitor = new
            {
                Name = monitorName,
                Url = "http://google.com",
                Interval = 60,
                ExpectedStatusCode = 200,
                Mode = "Poke"
            };
            var content = new StringContent(JsonConvert.SerializeObject(monitor), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/v1/monitor/http", content);

            // Act
            var response = await client.GetAsync($"/api/v1/monitor/{monitorName}-Http");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains(monitorName, responseString);
        }

        [Fact]
        public async Task Delete_DeleteMonitor_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/api/v1/monitor/test-monitor-to-delete");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateDnsMonitor_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var monitor = new
            {
                Name = "TestDnsMonitor",
                Hostname = "8.8.8.8",
                Interval = 60,
                Mode = "Poke"
            };
            var content = new StringContent(JsonConvert.SerializeObject(monitor), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/v1/monitor/dns", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_GetAlertInfo_ReturnsNotFound_WhenAlertDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/alert/nonexistent-alert");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_GetAlertInfo_ReturnsOk_WhenAlertExists()
        {
            // Arrange
            var client = _factory.CreateClient();
            var alertName = "TestAlertForGet";
            var alert = new
            {
                Name = alertName,
                Url = "http://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX",
                Channel = "#general"
            };
            var content = new StringContent(JsonConvert.SerializeObject(alert), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/v1/alert/slack", content);

            // Act
            var response = await client.GetAsync($"/api/v1/alert/{alertName}-Slack");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains(alertName, responseString);
        }

        [Fact]
        public async Task Delete_DeleteAlert_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/api/v1/alert/test-alert-to-delete");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateSlackAlert_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var alert = new
            {
                Name = "TestSlackAlert",
                Url = "http://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX",
                Channel = "#general"
            };
            var content = new StringContent(JsonConvert.SerializeObject(alert), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/v1/alert/slack", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
