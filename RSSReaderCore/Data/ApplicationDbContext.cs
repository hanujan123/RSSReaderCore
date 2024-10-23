using CodeHollow.FeedReader;
using Microsoft.EntityFrameworkCore;
using RSSReaderCore.Models.Entities;

namespace RSSReaderCore.Data
{
		public class ApplicationDbContext : DbContext
		{
				public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
				{

				}

        public DbSet<FeedRecord> FeedRecords { get; set; }
    }
}
