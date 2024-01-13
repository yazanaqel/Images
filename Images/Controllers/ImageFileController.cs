using Images.Models;
using Images.Services;
using Microsoft.AspNetCore.Mvc;

namespace Images.Controllers
{
	public class ImageFileController : Controller
	{
		private readonly IFileImageService imageService;

		public ImageFileController(IFileImageService imageService)
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

		public IActionResult Done() => this.View();
	}
}
