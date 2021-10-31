using MonitorLib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonitorLib.Dtos
{
    public class MonitorCommon
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MonitorMode Mode { get; set; }

        public int Interval { get; set; }
    }
}
