using MonitorLib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonitorLib.Dtos
{
    public class MonitorStatus : MonitorInfo
    {
        public MonitorStatus(string name, string identifier, MonitorType type, int interval, MonitorMode mode, MonitorState state) : base(name, identifier, type, interval, mode)
        {
            State = state;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public MonitorState State { get; set; }
    }
}
