using Images.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Images.Services
{
	public class FileImageService : IFileImageService
	{
		private const int thumbnailWidth = 300;
		private const int fullScreenWidth = 1000;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly ApplicationDbContext dbContext;

		public FileImageService(
			IServiceScopeFactory serviceScopeFactory,
			ApplicationDbContext dbContext)
		{
			this.serviceScopeFactory = serviceScopeFactory;
			this.dbContext = dbContext;
		}

		public Task<List<string>> GetAllImages()
		{
			return this.dbContext
				.ImagesFile
				.Select(x => x.Folder + "/Thumbnail_" + x.Id + ".jpg")
				.ToListAsync();
		}

		public async Task Process(IEnumerable<ImageInputModel> images)
		{
			// instead of IServiceScopeFactory we can use ConcurrentDictionary

			var totalImages = await
				this.dbContext
				.ImagesFile
				.CountAsync();

			var tasks = images
				.Select(image => Task.Run(async () =>
				{
					try
					{
						using var imageResult = await Image.LoadAsync(image.Content);

						var id = Guid.NewGuid();
						var path = $"/images/{totalImages % 1000}/";
						var name = $"{id}.jpg";
						var storagePath = Path.Combine(
							Directory.GetCurrentDirectory(), $"wwwroot{path}".Replace("/", "\\"));

						if (!Directory.Exists(storagePath))
						{
							Directory.CreateDirectory(storagePath);
						}


						await this.SaveImage(
							imageResult, $"Original_{name}", storagePath, imageResult.Width);
						await this.SaveImage(
							imageResult, $"FullScreen_{name}", storagePath, fullScreenWidth);
						await this.SaveImage(
							imageResult, $"Thumbnail_{name}", storagePath, thumbnailWidth);

						var data = this.serviceScopeFactory
							.CreateScope()
							.ServiceProvider
							.GetRequiredService<ApplicationDbContext>();

						data.ImagesFile.Add(new ImageFile
						{
							Id = id,
							Folder = path
						});

						await data.SaveChangesAsync();
					}
					catch
					{
						//loger info
					}

				}))
				.ToList();



			await Task.WhenAll(tasks);
		}
		private async Task SaveImage(Image image, string name, string path, int resizeWidth)
		{
			var width = image.Width;
			var height = image.Height;

			if (width > resizeWidth)
			{
				height = (int)((double)resizeWidth / width * height);
				width = resizeWidth;
			}

			image.Mutate(i => i.Resize(new Size(width, height)));

			image.Metadata.ExifProfile = null;

			await image.SaveAsJpegAsync($"{path}/{name}", new JpegEncoder
			{
				Quality = 75
			});

		}

	}
}
