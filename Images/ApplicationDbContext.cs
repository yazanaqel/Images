using Images.Models;
using Microsoft.EntityFrameworkCore;

namespace Images
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{

        //}

        public DbSet<ImageData> ImagesData { get; set; }
		public DbSet<ImageFile> ImagesFile { get; set; }
	}
}
