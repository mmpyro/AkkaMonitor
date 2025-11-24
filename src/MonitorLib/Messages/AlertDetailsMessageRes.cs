using MonitorLib.Enums;

namespace MonitorLib.Messages
{
    public record AlertDetailsMessageRes(string Name, MonitorLib.Enums.AlertType Type, object Parameters);
}
