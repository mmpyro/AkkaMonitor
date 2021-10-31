using MonitorLib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonitorLib.Dtos
{
    public class AlertDetails
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AlertType Type { get; set; }

        public object Parameters { get; set; }
    }
}
