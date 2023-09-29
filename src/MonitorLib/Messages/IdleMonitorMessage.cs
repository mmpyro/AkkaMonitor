namespace MonitorLib.Messages
{

    public class IdleMonitorMessage
    {
        public IdleMonitorMessage(int Id)
        {
            this.Id = Id;
        }

        public int Id { get; }
    }
}
