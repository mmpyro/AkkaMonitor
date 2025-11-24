using FluentAssertions;
using MonitorLib.Factories;
using Xunit;

namespace MonitorLib.Tests.Factories
{
    public class SlackClientFactoryTests
    {
        private readonly SlackClientFactory _factory;

        public SlackClientFactoryTests()
        {
            _factory = new SlackClientFactory();
        }

        [Fact]
        public void Create_WithValidUrl_ShouldReturnSlackClient()
        {
            // Arrange
            var url = "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXX";

            // Act
            var result = _factory.Create(url);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<Clients.ISlackClient>();
        }

        [Fact]
        public void Create_MultipleCalls_ShouldReturnNewInstances()
        {
            // Arrange
            var url = "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXX";

            // Act
            var result1 = _factory.Create(url);
            var result2 = _factory.Create(url);

            // Assert
            result1.Should().NotBeSameAs(result2);
        }
    }
}
