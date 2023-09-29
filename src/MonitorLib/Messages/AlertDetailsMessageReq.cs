namespace MonitorLib.Messages
{
    public class AlertDetailsMessageReq
    {
        public AlertDetailsMessageReq(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
