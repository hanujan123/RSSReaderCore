using CodeHollow.FeedReader;
using RSSReaderCore.Models.Entities;

namespace RSSReaderCore.Models
{
	public class FeedViewModel
	{
		public FeedRecord Record { get; set; }

		public Guid? FeedId { get; set; }
		public string Description { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
	}
}
