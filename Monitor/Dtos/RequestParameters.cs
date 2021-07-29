using Monitor.Messages;

namespace Monitor.Dtos
{
    public class RequestParameters
    {
        public string Url { get; set; }
        public int ExpectedStatusCode { get; set; }

        public RequestParameters()
        {
            
        }
        public RequestParameters(string url, int expectedStatusCode)
        {
            Url = url;
            ExpectedStatusCode = expectedStatusCode;
        }

        public static RequestParameters From(CreateHttpMonitorMessage message)
        {
            return new RequestParameters(message.Url, message.ExpectedStatusCode);
        }
    }
}