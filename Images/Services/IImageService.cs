using Images.Models;

namespace Images.Services
{
	public interface IImageService
	{
		Task Process(IEnumerable<ImageInputModel> images);
		Task<Stream> GetThumbnail(string id);
		Task<Stream> GetFullScreen(string id);
		Task<List<string>> GetAllImages();

    }
}
