namespace MonitorLib.Messages
{
    public class DeleteActorMessage
    {
        public int Id { get; private set; }

        public DeleteActorMessage(int id)
        {
            Id = id;
        }
    }
}