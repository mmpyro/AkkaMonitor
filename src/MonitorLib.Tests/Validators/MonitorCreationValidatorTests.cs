using System;
using FluentAssertions;
using MonitorLib.Enums;
using MonitorLib.Messages;
using MonitorLib.Validators;
using Xunit;

namespace MonitorLib.Tests.Validators
{
    public class MonitorCreationValidatorTests
    {
        private readonly MonitorCreationValidator _validator;

        public MonitorCreationValidatorTests()
        {
            _validator = new MonitorCreationValidator();
        }

        [Fact]
        public void Validate_ValidHttpMonitor_ShouldNotThrow()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
        }

        [Fact]
        public void Validate_ValidDnsMonitor_ShouldNotThrow()
        {
            // Arrange
            var message = new CreateDnsMonitorMessage(
                name: "TestDnsMonitor",
                hostname: "8.8.8.8",
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
        }

        [Fact]
        public void Validate_EmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "",
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Monitor name cannot be empty*");
        }

        [Fact]
        public void Validate_InvalidInterval_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 0,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Interval has to be greater than 0*");
        }

        [Fact]
        public void Validate_EmptyIdentifier_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Identifier cannot be empty*");
        }

        [Fact]
        public void Validate_UnknownMode_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Unknown
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Invalid mode parameter*");
        }

        [Theory]
        [InlineData(99)]
        [InlineData(600)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidHttpStatusCode_ShouldThrowArgumentException(int statusCode)
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: "https://google.com",
                expectedStatusCode: statusCode,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage($"*{statusCode} is not valid http status code*");
        }

        [Theory]
        [InlineData("ftp://example.com")]
        [InlineData("example.com")]
        [InlineData("www.example.com")]
        public void Validate_InvalidProtocol_ShouldThrowArgumentException(string url)
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: url,
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Invalid protocol*");
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com")]
        public void Validate_ValidProtocol_ShouldNotThrow(string url)
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "TestMonitor",
                url: url,
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
        }

        [Fact]
        public void Validate_MultipleValidationErrors_ShouldThrowWithAllErrors()
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: "",
                url: "",
                expectedStatusCode: 99,
                interval: 0,
                mode: MonitorMode.Unknown
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Monitor name cannot be empty*")
                .WithMessage("*Interval has to be greater than 0*")
                .WithMessage("*Identifier cannot be empty*")
                .WithMessage("*Invalid mode parameter*")
                .WithMessage("*is not valid http status code*");
        }

        [Theory]
        [InlineData("https://wp.pl")]
        [InlineData("http://example.com")]
        [InlineData("name with spaces")]
        [InlineData("name?query=value")]
        [InlineData("name:colon")]
        [InlineData("name/slash")]
        public void Validate_NameNotUrlEncoded_ShouldThrowArgumentException(string name)
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: name,
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*is not URL-safe*");
        }

        [Theory]
        [InlineData("simple-name")]
        [InlineData("my_monitor")]
        [InlineData("monitor123")]
        [InlineData("MyMonitor")]
        [InlineData("test.monitor")]
        [InlineData("monitor~test")]
        public void Validate_UrlSafeName_ShouldNotThrow(string name)
        {
            // Arrange
            var message = new CreateHttpMonitorMessage(
                name: name,
                url: "https://google.com",
                expectedStatusCode: 200,
                interval: 60,
                mode: MonitorMode.Poke
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
        }
    }
}

