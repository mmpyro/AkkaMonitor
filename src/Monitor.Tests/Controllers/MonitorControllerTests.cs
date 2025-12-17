using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor;
using MonitorLib.Dtos;
using MonitorLib.Enums;
using MonitorLib.Messages;
using MonitorLib.Validators;
using MonitorServer.Controllers;
using Moq;
using Xunit;

namespace Monitor.Tests.Controllers
{
    public class MonitorControllerTests
    {
        private readonly Mock<IMonitorController> _monitorControllerMock;
        private readonly Mock<IMonitorCreationValidator> _validatorMock;
        private readonly Mock<ILogger<MonitorController>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly MonitorController _controller;

        public MonitorControllerTests()
        {
            _monitorControllerMock = new Mock<IMonitorController>();
            _validatorMock = new Mock<IMonitorCreationValidator>();
            _loggerMock = new Mock<ILogger<MonitorController>>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MonitorProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new MonitorController(
                _monitorControllerMock.Object,
                _validatorMock.Object,
                _mapper,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Get_ShouldReturnMonitors()
        {
            // Arrange
            var expectedMonitors = new List<MonitorInfo>
            {
                new MonitorInfo("Monitor1", "https://example.com", MonitorType.Http, 60, MonitorMode.Poke),
                new MonitorInfo("Monitor2", "8.8.8.8", MonitorType.DNS, 30, MonitorMode.Reschedule)
            };

            _monitorControllerMock
                .Setup(x => x.ListMonitors())
                .ReturnsAsync(expectedMonitors);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().BeEquivalentTo(expectedMonitors);
            _monitorControllerMock.Verify(x => x.ListMonitors(), Times.Once);
        }

        [Fact]
        public async Task GetMonitorInfo_WithExistingMonitor_ShouldReturnMonitorStatus()
        {
            // Arrange
            var monitorName = "TestMonitor";
            var expectedStatus = new MonitorStatusMessageRes(
                Name: monitorName,
                Interval: 60,
                Identifier: "https://example.com",
                Type: MonitorType.Http,
                Mode: MonitorMode.Poke,
                State: MonitorState.Success
            );

            _monitorControllerMock
                .Setup(x => x.GetMonitorInfo(monitorName))
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _controller.GetMonitorInfo(monitorName);

            // Assert
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(monitorName);
            result.Value.State.Should().Be(MonitorState.Success);
        }

        [Fact]
        public async Task GetMonitorInfo_WithNonExistingMonitor_ShouldReturnNotFound()
        {
            // Arrange
            var monitorName = "NonExistingMonitor";

            _monitorControllerMock
                .Setup(x => x.GetMonitorInfo(monitorName))
                .ReturnsAsync((MonitorStatusMessageRes)null);

            // Act
            var result = await _controller.GetMonitorInfo(monitorName);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be($"Monitor {monitorName} doesn't exists.");
        }

        [Fact]
        public void DeleteMonitor_ShouldCallDeleteAndReturnNoContent()
        {
            // Arrange
            var monitorName = "TestMonitor";

            // Act
            var result = _controller.DeleteMonitor(monitorName);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _monitorControllerMock.Verify(x => x.DeleteMonitor(monitorName), Times.Once);
        }

        [Fact]
        public async Task CreateHttpMonitor_WithValidData_ShouldReturnOk()
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

            var expectedResponse = new CreateMonitorMessageRes("TestMonitor");

            _monitorControllerMock
                .Setup(x => x.CreateMonitor(It.IsAny<CreateHttpMonitorMessage>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateHttpMonitor(httpMonitor);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be("Monitor TestMonitor created.");
            _validatorMock.Verify(x => x.Validate(It.IsAny<CreateHttpMonitorMessage>()), Times.Once);
        }

        [Fact]
        public async Task CreateDnsMonitor_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var dnsMonitor = new DnsMonitor
            {
                Name = "TestDnsMonitor",
                Hostname = "8.8.8.8",
                Interval = 30,
                Mode = MonitorMode.Poke
            };

            var expectedResponse = new CreateMonitorMessageRes("TestDnsMonitor");

            _monitorControllerMock
                .Setup(x => x.CreateMonitor(It.IsAny<CreateDnsMonitorMessage>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateDnsMonitor(dnsMonitor);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be("Monitor TestDnsMonitor created.");
            _validatorMock.Verify(x => x.Validate(It.IsAny<CreateDnsMonitorMessage>()), Times.Once);
        }

        [Fact]
        public async Task CreateHttpMonitor_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var httpMonitor = new HttpMonitor
            {
                Name = "",
                Url = "https://example.com",
                ExpectedStatusCode = 200,
                Interval = 60,
                Mode = MonitorMode.Poke
            };

            _validatorMock
                .Setup(x => x.Validate(It.IsAny<CreateHttpMonitorMessage>()))
                .Throws(new System.ArgumentException("Monitor name cannot be empty."));

            // Act
            var result = await _controller.CreateHttpMonitor(httpMonitor);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Monitor name cannot be empty.");
        }

        [Fact]
        public async Task CreateHttpMonitor_WhenCreationFails_ShouldReturnBadRequest()
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

            _monitorControllerMock
                .Setup(x => x.CreateMonitor(It.IsAny<CreateHttpMonitorMessage>()))
                .ReturnsAsync((CreateMonitorMessageRes)null);

            // Act
            var result = await _controller.CreateHttpMonitor(httpMonitor);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
