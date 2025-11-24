using MonitorLib.Enums;

namespace MonitorLib.Messages
{
    public record MonitorStatusMessageRes(string Name, int Interval, string Identifier, MonitorType Type, MonitorMode Mode, MonitorState State = MonitorState.Unknown);
}