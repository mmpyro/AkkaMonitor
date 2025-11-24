using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monitor;
using MonitorLib.Enums;
using MonitorLib.Messages;
using Moq;
using Xunit;

namespace Monitor.Tests
{
    public class ConfigurationParserTests
    {
        private readonly Mock<ILogger<ConfigurationParser>> _loggerMock;

        public ConfigurationParserTests()
        {
            _loggerMock = new Mock<ILogger<ConfigurationParser>>();
        }

        [Fact]
        public void Monitors_WithHttpMonitor_ShouldParseCorrectly()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Monitors:0:Type"] = "Http",
                ["Monitors:0:Name"] = "TestHttpMonitor",
                ["Monitors:0:Url"] = "https://example.com",
                ["Monitors:0:ExpectedStatusCode"] = "200",
                ["Monitors:0:Interval"] = "60",
                ["Monitors:0:Mode"] = "Poke"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            monitors.Should().HaveCount(1);
            var monitor = monitors[0] as CreateHttpMonitorMessage;
            monitor.Should().NotBeNull();
            monitor.Name.Should().Be("TestHttpMonitor-Http");
            monitor.Identifier.Should().Be("https://example.com");
            monitor.ExpectedStatusCode.Should().Be(200);
            monitor.Interval.Should().Be(60);
            monitor.Mode.Should().Be(MonitorMode.Poke);
        }

        [Fact]
        public void Monitors_WithDnsMonitor_ShouldParseCorrectly()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Monitors:0:Type"] = "DNS",
                ["Monitors:0:Name"] = "TestDnsMonitor",
                ["Monitors:0:Hostname"] = "8.8.8.8",
                ["Monitors:0:Interval"] = "30",
                ["Monitors:0:Mode"] = "Reschedule"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            monitors.Should().HaveCount(1);
            var monitor = monitors[0] as CreateDnsMonitorMessage;
            monitor.Should().NotBeNull();
            monitor.Name.Should().Be("TestDnsMonitor-DNS");
            monitor.Identifier.Should().Be("8.8.8.8");
            monitor.Interval.Should().Be(30);
            monitor.Mode.Should().Be(MonitorMode.Reschedule);
        }

        [Fact]
        public void Monitors_WithMultipleMonitors_ShouldParseAll()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Monitors:0:Type"] = "Http",
                ["Monitors:0:Name"] = "HttpMonitor",
                ["Monitors:0:Url"] = "https://example.com",
                ["Monitors:0:ExpectedStatusCode"] = "200",
                ["Monitors:0:Interval"] = "60",
                ["Monitors:0:Mode"] = "Poke",
                
                ["Monitors:1:Type"] = "DNS",
                ["Monitors:1:Name"] = "DnsMonitor",
                ["Monitors:1:Hostname"] = "8.8.8.8",
                ["Monitors:1:Interval"] = "30",
                ["Monitors:1:Mode"] = "Poke"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            monitors.Should().HaveCount(2);
        }

        [Fact]
        public void Monitors_WithInvalidInterval_ShouldUseDefaultInterval()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Monitors:0:Type"] = "Http",
                ["Monitors:0:Name"] = "TestMonitor",
                ["Monitors:0:Url"] = "https://example.com",
                ["Monitors:0:ExpectedStatusCode"] = "200",
                ["Monitors:0:Interval"] = "invalid",
                ["Monitors:0:Mode"] = "Poke"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            var monitor = monitors[0] as CreateHttpMonitorMessage;
            monitor.Interval.Should().Be(15); // DEFAULT_INTERVAL
        }

        [Fact]
        public void Monitors_WithZeroInterval_ShouldUseDefaultInterval()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Monitors:0:Type"] = "Http",
                ["Monitors:0:Name"] = "TestMonitor",
                ["Monitors:0:Url"] = "https://example.com",
                ["Monitors:0:ExpectedStatusCode"] = "200",
                ["Monitors:0:Interval"] = "0",
                ["Monitors:0:Mode"] = "Poke"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            var monitor = monitors[0] as CreateHttpMonitorMessage;
            monitor.Interval.Should().Be(15); // DEFAULT_INTERVAL
        }

        [Fact]
        public void Alerts_WithSlackAlert_ShouldParseCorrectly()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Alerts:0:Type"] = "Slack",
                ["Alerts:0:Name"] = "TestSlackAlert",
                ["Alerts:0:Url"] = "https://hooks.slack.com/services/XXX",
                ["Alerts:0:Channel"] = "#general"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var alerts = parser.Alerts.ToList();

            // Assert
            alerts.Should().HaveCount(1);
            var alert = alerts[0] as CreateSlackAlertMessage;
            alert.Should().NotBeNull();
            alert.Name.Should().Be("TestSlackAlert-Slack");
            alert.Url.Should().Be("https://hooks.slack.com/services/XXX");
            alert.Channel.Should().Be("#general");
        }

        [Fact]
        public void Alerts_WithMultipleAlerts_ShouldParseAll()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["Alerts:0:Type"] = "Slack",
                ["Alerts:0:Name"] = "Alert1",
                ["Alerts:0:Url"] = "https://hooks.slack.com/services/XXX",
                ["Alerts:0:Channel"] = "#general",
                
                ["Alerts:1:Type"] = "Slack",
                ["Alerts:1:Name"] = "Alert2",
                ["Alerts:1:Url"] = "https://hooks.slack.com/services/YYY",
                ["Alerts:1:Channel"] = "#alerts"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var alerts = parser.Alerts.ToList();

            // Assert
            alerts.Should().HaveCount(2);
        }

        [Fact]
        public void Monitors_WithEmptyConfiguration_ShouldReturnEmpty()
        {
            // Arrange
            var configData = new Dictionary<string, string>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var monitors = parser.Monitors.ToList();

            // Assert
            monitors.Should().BeEmpty();
        }

        [Fact]
        public void Alerts_WithEmptyConfiguration_ShouldReturnEmpty()
        {
            // Arrange
            var configData = new Dictionary<string, string>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var parser = new ConfigurationParser(configuration, _loggerMock.Object);

            // Act
            var alerts = parser.Alerts.ToList();

            // Assert
            alerts.Should().BeEmpty();
        }
    }
}
