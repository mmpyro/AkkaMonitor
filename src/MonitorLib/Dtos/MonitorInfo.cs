using MonitorLib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonitorLib.Dtos
{
    public class MonitorInfo
    {
        public MonitorInfo(string name, string identifier, MonitorType type,int interval, MonitorMode mode)
        {
            Name = name;
            Identifier = identifier;
            Type = type;
            Interval = interval;
            Mode = mode;
        }

        public string Name { get; }
        public string Identifier { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MonitorType Type { get; }
        public int Interval { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MonitorMode Mode { get; }
    }
}
