namespace RSSReaderCore.Models.Entities
{
    public class FeedRecord
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public FeedRecord()
        {

        }
    }
}
