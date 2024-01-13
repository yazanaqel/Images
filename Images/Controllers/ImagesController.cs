using Images.Models;
using Images.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;

namespace Images.Controllers
{
	public class ImagesController : Controller
	{
		private readonly IImageService imageService;

		public ImagesController(IImageService imageService)
		{
			this.imageService = imageService;
		}
		public IActionResult Upload() => this.View();

		[HttpPost]
		//Size for all
		[RequestSizeLimit(100 * 1024 * 1024)]
		public async Task<IActionResult> Upload(IFormFile[] images)
		{
			if (images.Length > 10)
			{
				this.ModelState.AddModelError("images", "you can't upload more than 10 images!");
				return this.View();
			}

			//Size for one
			//if (images.Any(x => x.Length > 10 * 1024 * 1024))
			//{
			//	this.ModelState.AddModelError("images", "you can't upload more than 10 MB for one image!");
			//}

			await this.imageService.Process(images.Select(i => new ImageInputModel
			{
				Name = i.FileName,
				Type = i.ContentType,
				Content = i.OpenReadStream()
			}));




			return RedirectToAction(nameof(this.Done));
		}

		public async Task<IActionResult> All()
			=> this.View(await this.imageService.GetAllImages());
		public async Task<IActionResult> Thumbnail(string id)
			=> this.ReturnImage(await this.imageService.GetThumbnail(id));
		public async Task<IActionResult> FullScreen(string id)
			=> this.ReturnImage(await this.imageService.GetFullScreen(id));
		public IActionResult ReturnImage(Stream image)
		{
			var headers = Response.GetTypedHeaders();

			headers.CacheControl = new CacheControlHeaderValue
			{
				Public = true,
				MaxAge = TimeSpan.FromDays(30)
			};
			headers.Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30));

			return this.File(image, "image/jpeg");
		}
        public IActionResult Done() => this.View();
	}
}