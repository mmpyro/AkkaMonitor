using FluentAssertions;
using MonitorLib.Factories;
using Xunit;

namespace MonitorLib.Tests.Factories
{
    public class RequestFactoryTests
    {
        private readonly RequestFactory _factory;

        public RequestFactoryTests()
        {
            _factory = new RequestFactory();
        }

        [Fact]
        public void CreateRequestClient_ShouldReturnRequestInstance()
        {
            // Act
            var result = _factory.CreateRequestClient();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<Clients.IRequest>();
        }

        [Fact]
        public void CreateDnsRequestClient_ShouldReturnDnsRequestInstance()
        {
            // Act
            var result = _factory.CreateDnsRequestClient();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<Clients.IDnsRequest>();
        }

        [Fact]
        public void CreateRequestClient_MultipleCalls_ShouldReturnNewInstances()
        {
            // Act
            var result1 = _factory.CreateRequestClient();
            var result2 = _factory.CreateRequestClient();

            // Assert
            result1.Should().NotBeSameAs(result2);
        }

        [Fact]
        public void CreateDnsRequestClient_MultipleCalls_ShouldReturnNewInstances()
        {
            // Act
            var result1 = _factory.CreateDnsRequestClient();
            var result2 = _factory.CreateDnsRequestClient();

            // Assert
            result1.Should().NotBeSameAs(result2);
        }
    }
}
