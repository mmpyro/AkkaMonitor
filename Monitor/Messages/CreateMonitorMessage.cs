namespace Monitor.Messages
{
    public class CreateMonitorMessage {}

    public class CreateHttpMonitorMessage : CreateMonitorMessage
    {
        public CreateHttpMonitorMessage(string url, int expectedStatusCode)
        {
            Url = url;
            ExpectedStatusCode = expectedStatusCode;
        }

        public string Url { get; private set; }

        public int ExpectedStatusCode { get; set; }

        public override bool Equals(object obj)
        {
            var message = obj as CreateHttpMonitorMessage;
            if(message != null)
            {
                return message.Url == Url;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }

        public override string ToString()
        {
            return Url;
        }
    }
}