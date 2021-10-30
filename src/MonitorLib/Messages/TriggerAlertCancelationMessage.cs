namespace MonitorLib.Messages
{
    public class TriggerAlertCancelationMessage
    {
        public TriggerAlertCancelationMessage(string content)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }
}