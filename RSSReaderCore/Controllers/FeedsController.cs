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

		// GET: Feeds
		public async Task<IActionResult> Index()
		{
			return View(await _context.FeedRecords.ToListAsync());
		}

		// GET: Feeds/Details/5
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

		// GET: Feeds/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Feeds/Create
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

		// GET: Feeds/Delete/5
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

		// POST: Feeds/Delete/5
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

		private bool FeedRecordExists(Guid id)
		{
			return _context.FeedRecords.Any(e => e.Id == id);
		}

		[HttpGet]
		public async Task<IActionResult> ReloadArticles(Guid? id, DateFilterModel model)
		{
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
