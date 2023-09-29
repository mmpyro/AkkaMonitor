namespace MonitorLib.Dtos
{
    public class HttpMonitor : MonitorCommon
    {
        public string Url { get; set; }

        public int ExpectedStatusCode { get; set; }
    }
}
