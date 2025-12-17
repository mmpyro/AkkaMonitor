using MonitorLib.Messages;
using System;

namespace MonitorLib.Validators
{
    public interface IAlertCreationValidator
    {
        void Validate(CreateAlertMessageReq message);
    }

    public class AlertCreationValidator : IAlertCreationValidator
    {
        public void Validate(CreateAlertMessageReq message)
        {
            if (string.IsNullOrEmpty(message.Name) || string.IsNullOrWhiteSpace(message.Name))
            {
                throw new ArgumentException("Alert name cannot be empty.");
            }

            // Extract the original name (without the type suffix) for validation
            var originalName = message.Name.Replace($"-{message.Type}", "");
            
            if (string.IsNullOrEmpty(originalName) || string.IsNullOrWhiteSpace(originalName))
            {
                throw new ArgumentException("Alert name cannot be empty.");
            }

            if (!IsUrlEncoded(originalName))
            {
                throw new ArgumentException($"Alert name '{originalName}' is not URL-safe. Please use a URL-encoded name or avoid special characters like ':', '/', '?', etc.");
            }
        }

        private bool IsUrlEncoded(string value)
        {
            // A name is URL-safe if encoding it returns the same value
            return Uri.EscapeDataString(value) == value;
        }
    }
}
