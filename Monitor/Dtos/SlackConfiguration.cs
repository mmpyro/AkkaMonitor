namespace Monitor.Dtos
{
    public class SlackConfiguration
    {
        public SlackConfiguration(string url, string channel)
        {
            Url = url;
            Channel = channel;
        }

        public string Url { get; private set; }
        public string Channel { get; private set; }
    }
}