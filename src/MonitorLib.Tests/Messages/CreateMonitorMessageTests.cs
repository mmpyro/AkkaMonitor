using FluentAssertions;
using MonitorLib.Enums;
using MonitorLib.Messages;
using Xunit;

namespace MonitorLib.Tests.Messages
{
    public class CreateMonitorMessageTests
    {
        [Fact]
        public void CreateHttpMonitorMessage_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Assert
            message.Name.Should().Be("TestMonitor-Http");
            message.Identifier.Should().Be("https://example.com");
            message.ExpectedStatusCode.Should().Be(200);
            message.Interval.Should().Be(60);
            message.Mode.Should().Be(MonitorMode.Poke);
            message.Type.Should().Be(MonitorType.Http);
        }

        [Fact]
        public void CreateDnsMonitorMessage_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var message = new CreateDnsMonitorMessage(
                name: "TestDnsMonitor",
                hostname: "8.8.8.8",
                interval: 30,
                mode: MonitorMode.Reschedule
            );

            // Assert
            message.Name.Should().Be("TestDnsMonitor-DNS");
            message.Identifier.Should().Be("8.8.8.8");
            message.Interval.Should().Be(30);
            message.Mode.Should().Be(MonitorMode.Reschedule);
            message.Type.Should().Be(MonitorType.DNS);
        }

        [Fact]
        public void CreateHttpMonitorMessage_DefaultMode_ShouldBePoke()
        {
            // Arrange & Act
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            // Assert
            message.Mode.Should().Be(MonitorMode.Poke);
        }

        [Fact]
        public void CreateDnsMonitorMessage_DefaultMode_ShouldBePoke()
        {
            // Arrange & Act
            var message = new CreateDnsMonitorMessage(
                name: "TestDnsMonitor",
                hostname: "8.8.8.8",
                interval: 30
            );

            // Assert
            message.Mode.Should().Be(MonitorMode.Poke);
        }

        [Fact]
        public void CreateMonitorMessage_Id_ShouldBeHashCodeOfName()
        {
            // Arrange & Act
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            // Assert
            message.Id.Should().Be(message.Name.GetHashCode());
        }

        [Fact]
        public void CreateMonitorMessage_Equals_WithSameName_ShouldReturnTrue()
        {
            // Arrange
            var message1 = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            var message2 = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://different.com",
                expectedStatusCode: 404,
                interval: 30
            );

            // Act & Assert
            message1.Equals(message2).Should().BeTrue();
        }

        [Fact]
        public void CreateMonitorMessage_Equals_WithDifferentName_ShouldReturnFalse()
        {
            // Arrange
            var message1 = new CreateHttpMonitorMessage(
                name: "TestMonitor1",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            var message2 = new CreateHttpMonitorMessage(
                name: "TestMonitor2",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            // Act & Assert
            message1.Equals(message2).Should().BeFalse();
        }

        [Fact]
        public void CreateMonitorMessage_ToString_ShouldReturnName()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://example.com",
                expectedStatusCode: 200,
                interval: 60
            );

            // Act
            var result = message.ToString();

            // Assert
            result.Should().Be("TestMonitor-Http");
        }
    }
}
