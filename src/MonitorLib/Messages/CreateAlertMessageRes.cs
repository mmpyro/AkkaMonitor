namespace MonitorLib.Messages
{
    public class CreateAlertMessageRes
    {
        public CreateAlertMessageRes(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}