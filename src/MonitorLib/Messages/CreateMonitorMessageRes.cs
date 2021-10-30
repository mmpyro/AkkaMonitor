namespace MonitorLib.Messages
{
    public class CreateMonitorMessageRes
    {
        public CreateMonitorMessageRes(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}