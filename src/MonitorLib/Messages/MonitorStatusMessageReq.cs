namespace MonitorLib.Messages
{
    public class MonitorStatusMessageReq
    {
        public MonitorStatusMessageReq(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}