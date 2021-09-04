using Monitor.Messages;

namespace Monitor.Dtos
{
    public class RequestParameters
    {
        public string Url { get; set; }
        public int ExpectedStatusCode { get; set; }

        public int CheckInterval { get; set; }

        public RequestParameters()
        {
            
        }
        public RequestParameters(string url, int expectedStatusCode, int checkInterval)
        {
            Url = url;
            ExpectedStatusCode = expectedStatusCode;
            CheckInterval = checkInterval;
        }

        public static RequestParameters From(CreateHttpMonitorMessage message)
        {
            return new RequestParameters(message.Url, message.ExpectedStatusCode, message.CheckInterval);
        }
    }
}