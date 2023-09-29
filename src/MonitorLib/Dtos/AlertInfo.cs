using MonitorLib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonitorLib.Dtos
{
    public class AlertInfo
    {
        public AlertInfo(string name, AlertType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AlertType Type { get; }
    }
}
