using MonitorLib.Messages;
using System;
using System.Text;

namespace MonitorLib.Validators
{
    public interface IMonitorCreationValidator
    {
        void Validate(CreateMonitorMessageReq message);
    }

    public class MonitorCreationValidator : IMonitorCreationValidator
    {
        private bool _valid = true;
        private readonly StringBuilder _sb = new();

        public void Validate(CreateMonitorMessageReq message)
        {
            switch (message)
            {
                case CreateHttpMonitorMessage:
                    ValidateHttp((CreateHttpMonitorMessage) message);
                    break;
                case CreateDnsMonitorMessage:
                    ValidateDns((CreateDnsMonitorMessage) message);
                    break;
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }

            if (_valid == false)
            {
                throw new ArgumentException(_sb.ToString());
            }
        }

        private void ValidateMessageBase(CreateMonitorMessageReq msg)
        {
            if (IsEmpty(msg.Name) || IsEmpty(msg.Name.Replace($"-{msg.Type}", "")))
            {
                MarkInvalid("Monitor name cannot be empty.");
            }

            if (!IsEmpty(msg.Name) && !IsUrlEncoded(msg.Name))
            {
                MarkInvalid($"Monitor name '{msg.Name}' is not URL-safe. Please use a URL-encoded name or avoid special characters like ':', '/', '?', etc.");
            }

            if (msg.Interval <= 0)
            {
                MarkInvalid($"Interval has to be greater than 0, but was {msg.Interval} .");
            }

            if (IsEmpty(msg.Identifier))
            {
                MarkInvalid("Identifier cannot be empty.");
            }

            if(msg.Mode == Enums.MonitorMode.Unknown)
            {
                MarkInvalid("Invalid mode parameter. Valid mode parameters are: [Reschedule, Poke].");
            }
        }

        private void ValidateHttp(CreateHttpMonitorMessage msg)
        {
            ValidateMessageBase(msg);

            if (msg.ExpectedStatusCode < 100 || msg.ExpectedStatusCode >= 600)
            {
                MarkInvalid($"{msg.ExpectedStatusCode} is not valid http status code.");
            }

            if (msg?.Identifier != null && !msg.Identifier.StartsWith("http://") && !msg.Identifier.StartsWith("https://"))
            {
                MarkInvalid("Invalid protocol. Valid protocols for Identifier are http:// or https://");
            }
        }

        private void ValidateDns(CreateDnsMonitorMessage msg)
        {
            ValidateMessageBase(msg);
        }

        private void MarkInvalid(string msg)
        {
            _valid = false;
            _sb.AppendLine(msg);
        }

        private bool IsEmpty(string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }

        private bool IsUrlEncoded(string value)
        {
            // A name is URL-safe if encoding it returns the same value
            return Uri.EscapeDataString(value) == value;
        }
    }
}
