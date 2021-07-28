namespace Monitor.Messages
{
    public class TriggerAlertMessage
    {
        public TriggerAlertMessage(string content)
        {
            Content = content;
        }
        public string Content { get; private set; }
    }
}