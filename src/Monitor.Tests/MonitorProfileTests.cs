using AutoMapper;
using FluentAssertions;
using Monitor;
using MonitorLib.Dtos;
using MonitorLib.Enums;
using MonitorLib.Messages;
using Xunit;

namespace Monitor.Tests
{
    public class MonitorProfileTests
    {
        private readonly IMapper _mapper;

        public MonitorProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MonitorProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_HttpMonitor_ToCreateHttpMonitorMessage_ShouldMapCorrectly()
        {
            // Arrange
            var httpMonitor = new HttpMonitor
            {
                Name = "TestMonitor",
                Url = "https://example.com",
                ExpectedStatusCode = 200,
                Interval = 60,
                Mode = MonitorMode.Poke
            };

            // Act
            var result = _mapper.Map<CreateHttpMonitorMessage>(httpMonitor);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestMonitor-Http");
            result.Identifier.Should().Be("https://example.com");
            result.ExpectedStatusCode.Should().Be(200);
            result.Interval.Should().Be(60);
            result.Mode.Should().Be(MonitorMode.Poke);
            result.Type.Should().Be(MonitorType.Http);
        }

        [Fact]
        public void Map_DnsMonitor_ToCreateDnsMonitorMessage_ShouldMapCorrectly()
        {
            // Arrange
            var dnsMonitor = new DnsMonitor
            {
                Name = "TestDnsMonitor",
                Hostname = "8.8.8.8",
                Interval = 30,
                Mode = MonitorMode.Reschedule
            };

            // Act
            var result = _mapper.Map<CreateDnsMonitorMessage>(dnsMonitor);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestDnsMonitor-DNS");
            result.Identifier.Should().Be("8.8.8.8");
            result.Interval.Should().Be(30);
            result.Mode.Should().Be(MonitorMode.Reschedule);
            result.Type.Should().Be(MonitorType.DNS);
        }

        [Fact]
        public void Map_SlackAlert_ToCreateSlackAlertMessage_ShouldMapCorrectly()
        {
            // Arrange
            var slackAlert = new SlackAlert
            {
                Name = "TestAlert",
                Url = "https://hooks.slack.com/services/XXX",
                Channel = "#general"
            };

            // Act
            var result = _mapper.Map<CreateSlackAlertMessage>(slackAlert);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestAlert-Slack");
            result.Url.Should().Be("https://hooks.slack.com/services/XXX");
            result.Channel.Should().Be("#general");
            result.Type.Should().Be(AlertType.Slack);
        }

        [Fact]
        public void Map_AlertDetailsMessageRes_ToAlertDetails_ShouldMapCorrectly()
        {
            // Arrange
            var parameters = new { Channel = "#general", Url = "https://hooks.slack.com/services/XXX" };
            var alertDetailsRes = new AlertDetailsMessageRes(
                name: "TestAlert",
                type: AlertType.Slack,
                parameters: parameters
            );

            // Act
            var result = _mapper.Map<AlertDetails>(alertDetailsRes);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestAlert");
            result.Type.Should().Be(AlertType.Slack);
            result.Parameters.Should().NotBeNull();
        }

        [Fact]
        public void Map_MonitorStatusMessageRes_ToMonitorStatus_ShouldMapCorrectly()
        {
            // Arrange
            var monitorStatusRes = new MonitorStatusMessageRes(
                name: "TestMonitor",
                interval: 60,
                identifier: "https://example.com",
                type: MonitorType.Http,
                mode: MonitorMode.Poke,
                state: MonitorState.Success
            );

            // Act
            var result = _mapper.Map<MonitorStatus>(monitorStatusRes);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestMonitor");
            result.State.Should().Be(MonitorState.Success);
            result.Identifier.Should().Be("https://example.com");
            result.Type.Should().Be(MonitorType.Http);
            result.Interval.Should().Be(60);
            result.Mode.Should().Be(MonitorMode.Poke);
        }

        [Fact]
        public void MapperConfiguration_ShouldBeValid()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MonitorProfile>();
            });

            // Act & Assert
            config.AssertConfigurationIsValid();
        }
    }
}
