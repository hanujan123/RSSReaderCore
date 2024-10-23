using CodeHollow.FeedReader;

namespace RSSReaderCore.Models
{
	public class ArticlesViewModel
	{
		public List<ArticleData> Articles { get; set; }

		public ArticlesViewModel(Feed feed)
		{
			Articles = [];
			foreach (var item in feed.Items)
			{
				Articles.Add(new ArticleData(item));
			}
		}
		public ArticlesViewModel()
		{

		}
	}
	public class ArticleData
	{
		public string Title { get; set; }
		public string PublishDate { get; set; }
		public string Link { get; set; }

		public ArticleData(FeedItem article)
		{
			Title = article.Title;
			Link = article.Link;

			if (article.PublishingDate.HasValue)
				PublishDate = article.PublishingDate.Value.ToString("dd.MM.yyyy");
			else PublishDate = string.Empty;
		}
		public ArticleData()
		{
			Title = string.Empty;
			Link = string.Empty;
			PublishDate = string.Empty;
		}
	}
}
