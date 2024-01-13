using Images.Models;

namespace Images.Services
{
	public interface IFileImageService
	{
		Task Process(IEnumerable<ImageInputModel> images);
		Task<List<string>> GetAllImages();
	}
}
