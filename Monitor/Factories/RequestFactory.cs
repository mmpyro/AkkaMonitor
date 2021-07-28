namespace Monitor.Factories
{
    public interface IRequestFactory
    {
        IRequest Create();
    }

    public class RequestFactory : IRequestFactory
    {
        public IRequest Create()
        {
            return new Request();
        }
    }
}