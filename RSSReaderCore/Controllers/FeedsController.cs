using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSSReaderCore.Data;
using RSSReaderCore.Models;
using RSSReaderCore.Models.Entities;

namespace RSSReaderCore.Controllers
{
	public class FeedsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public FeedsController(ApplicationDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Action displaying the feed list
		/// </summary>
		public async Task<IActionResult> Index()
		{
			return View(await _context.FeedRecords.ToListAsync());
		}

		/// <summary>
		/// Action showing the detail of specified feed
		/// </summary>
		/// <param name="id">Id of feed which detail should be displayed</param>
		public async Task<IActionResult> Details(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var feedRecord = await GetRecord(id);
			if (feedRecord == null)
			{
				return NotFound();
			}

			Feed? feed = GetFeed(feedRecord);

			//maybe use better error
			if (feed == null)
			{
				return NotFound();
			}

			return View(new FeedViewModel() { Record = feedRecord,Title=feed.Title, Description = feed.Description,ImageUrl=feed.ImageUrl, FeedId = id });
		}
		/// <summary>
		/// This Action shows view with form for creating a new feed record
		/// </summary>
		public IActionResult Create()
		{
			return View();
		}

		/// <summary>
		/// This Action is used for saving the feed specified in the form to the Db
		/// </summary>
		/// <param name="feedRecord">Data of the feed specified in the form</param>
		/// <returns></returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Url")] FeedRecord feedRecord)
		{
			if (ModelState.IsValid)
			{
				feedRecord.Id = Guid.NewGuid();
				_context.Add(feedRecord);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(feedRecord);
		}

		/// <summary>
		/// This Action will show the page with confirmation form for deleting selected feed
		/// </summary>
		/// <param name="id">ID of the feed that will be deleted</param>
		/// <returns>Returns view with confirmation form</returns>
		public async Task<IActionResult> Delete(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var feedRecord = await GetRecord(id);
			if (feedRecord == null)
			{
				return NotFound();
			}

			return View(feedRecord);
		}

		/// <summary>
		/// This Action finally deletes the selected feed after comfirmation by the user
		/// </summary>
		/// <param name="id">ID of the feed that will be deleted</param>
		/// <returns>Redirects back to the list of feeds</returns>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			var feedRecord = await _context.FeedRecords.FindAsync(id);
			if (feedRecord != null)
			{
				_context.FeedRecords.Remove(feedRecord);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// this methode is used to check for the existence of the feed specified in the parameter
		/// </summary>
		/// <param name="id">Id of the feed record that is being checked</param>
		/// <returns>bool specifiing if the record was found or not</returns>
		private bool FeedRecordExists(Guid id)
		{
			return _context.FeedRecords.Any(e => e.Id == id);
		}

		/// <summary>
		/// Ajax methode that will get the articles from the downloaded feed and sends them as JSON to the client side
		/// if the DateFilterModel is provided then the articles are filtered and only the ones corresponding to the date limits are returned
		/// </summary>
		/// <param name="id">ID of the feed from which will be obtained the articles</param>
		/// <param name="model">(Optional) Class containing filter parameters specifiing the limit dates from which the articles should be returned.</param>
		/// <returns>List of articles in JSON form the downloaded feed specified by the id and filterded using the dates specified in the model parameter or null on failure</returns>
		[HttpGet]
		public async Task<IActionResult> ReloadArticles(Guid? id, DateFilterModel model)
		{
			//the null return should also return failure code or be handled on the client side
			if (id == null)
				return new JsonResult(null);

			FeedRecord? record = await GetRecord(id);

			if (record == null)
				return new JsonResult(null);

			Feed? feed = GetFeed(record);
			if (feed == null)
				return new JsonResult(null);
			else
			{
				//remove articles with unwanted date before sending
				if (model.dateFrom != null || model.dateTo != null)
				{
					List<ArticleData> articles = new List<ArticleData>();
					foreach (var item in feed.Items)
					{
						if(item.PublishingDate != null){
							if (model.dateFrom != null && model.dateTo==null &&
									item.PublishingDate.Value >= model.dateFrom)
								articles.Add(new ArticleData(item));

							else if (model.dateTo != null && model.dateFrom == null &&
											 item.PublishingDate.Value <= model.dateTo)
								articles.Add(new ArticleData(item));

							else if(item.PublishingDate.Value >= model.dateFrom &&
											item.PublishingDate.Value <= model.dateTo)
								articles.Add(new ArticleData(item));
						}
					}
					return new JsonResult(new ArticlesViewModel() { Articles = articles});
				}
				else
					return new JsonResult(new ArticlesViewModel(feed));
			}
		}

		/// <summary>
		/// This methode is used to get the feed data from url or cache
		/// </summary>
		/// <param name="record">Feed record specifiing the feed that should be downloaded or checked for its presence in cache</param>
		/// <param name="UseCache">(not implemented) This parameter should be used to specifi if the cached feed is tried to be used or if the feed is downloaded and saved to cache</param>
		/// <returns>On success downloaded or cached feed and null on failure</returns>
		private Feed? GetFeed(FeedRecord record, bool UseCache = false)
		{
			//check cache
			/* 
			 if(cache contains record && record is valid) return cached record
			 */


			//download feed
			Task<Feed> downloadTask = FeedReader.ReadAsync(record.Url);
			downloadTask.Wait();
			if (downloadTask.Status == TaskStatus.RanToCompletion)
			{
				return downloadTask.Result;
			}
			return null;
		}

		/// <summary>
		/// Methode to get the feed record specified by the id from Db
		/// </summary>
		/// <param name="Id">Id of the wanted record</param>
		/// <returns>FeedRecord from db or null on failure</returns>
		private async Task<FeedRecord?> GetRecord(Guid? Id)
		{
			if (Id == null)
				return null;

			var feedRecord = await _context.FeedRecords
					.FirstOrDefaultAsync(m => m.Id == Id);
			return feedRecord;
		}
	}
}
