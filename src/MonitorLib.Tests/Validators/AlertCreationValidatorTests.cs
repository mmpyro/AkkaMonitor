using System;
using FluentAssertions;
using MonitorLib.Messages;
using MonitorLib.Validators;
using Xunit;

namespace MonitorLib.Tests.Validators
{
    public class AlertCreationValidatorTests
    {
        private readonly AlertCreationValidator _validator;

        public AlertCreationValidatorTests()
        {
            _validator = new AlertCreationValidator();
        }

        [Fact]
        public void Validate_ValidSlackAlert_ShouldNotThrow()
        {
            // Arrange
            var message = new CreateSlackAlertMessage(
                name: "TestAlert",
                url: "https://hooks.slack.com/services/xxx",
                channel: "#alerts"
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
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
            var message = new CreateSlackAlertMessage(
                name: name,
                url: "https://hooks.slack.com/services/xxx",
                channel: "#alerts"
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*is not URL-safe*");
        }

        [Theory]
        [InlineData("simple-name")]
        [InlineData("my_alert")]
        [InlineData("alert123")]
        [InlineData("MyAlert")]
        [InlineData("test.alert")]
        [InlineData("alert~test")]
        public void Validate_UrlSafeName_ShouldNotThrow(string name)
        {
            // Arrange
            var message = new CreateSlackAlertMessage(
                name: name,
                url: "https://hooks.slack.com/services/xxx",
                channel: "#alerts"
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().NotThrow();
        }

        [Fact]
        public void Validate_EmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new CreateSlackAlertMessage(
                name: "",
                url: "https://hooks.slack.com/services/xxx",
                channel: "#alerts"
            );

            // Act & Assert
            _validator.Invoking(v => v.Validate(message))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Alert name cannot be empty*");
        }
    }
}
