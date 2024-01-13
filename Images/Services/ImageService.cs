using Images.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Collections.Concurrent;

namespace Images.Services
{
    public class ImageService : IImageService
    {
        private const int thumbnailWidth = 300;
        private const int fullScreenWidth = 1000;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ApplicationDbContext dbContext;

        public ImageService(
            IServiceScopeFactory serviceScopeFactory,
            ApplicationDbContext dbContext)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.dbContext = dbContext;
        }

        public Task<List<string>> GetAllImages()
        {
            return this.dbContext
                .ImagesData
                .Select(x => x.Id.ToString())
                .ToListAsync();
        }

        public Task<Stream> GetFullScreen(string id)
        => this.GetImageData(id, "FullScreen");

        public Task<Stream> GetThumbnail(string id)
        => this.GetImageData(id, "Thumbnail");

        public async Task Process(IEnumerable<ImageInputModel> images)
        {
            // instead of IServiceScopeFactory we can use ConcurrentDictionary

            var tasks = images
                .Select(image => Task.Run(async () =>
                {
                    try
                    {
                        using var imageResult = await Image.LoadAsync(image.Content);

                        var original = await SaveImage(imageResult, imageResult.Width);
                        var fullScreen = await SaveImage(imageResult, fullScreenWidth);
                        var thumbnail = await SaveImage(imageResult, thumbnailWidth);

                        var data = this.serviceScopeFactory
                        .CreateScope()
                        .ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                        data.ImagesData.Add(new ImageData
                        {
                            OriginalFileName=image.Name,
                            OriginalType=image.Type,
                            OriginalContent=original,
                            FullScreenContent=fullScreen,
                            ThumbnailContent=thumbnail,
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
        private async Task<byte[]> SaveImage(Image image, int resizeWidth)
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

            var memoryStream = new MemoryStream();

            await image.SaveAsJpegAsync(memoryStream, new JpegEncoder
            {
                Quality = 75
            });

            return memoryStream.ToArray();

        }

        private async Task<Stream> GetImageData(string id, string size)
        {
            var database = this.dbContext.Database;

            var dbconnection = (SqlConnection)database.GetDbConnection();

            var command = new SqlCommand(
                $"SELECT {size}Content FROM ImagesData WHERE Id=@id"
                , dbconnection);

            command.Parameters.Add(new SqlParameter("@id", id));
            dbconnection.Open();
            var reader = await command.ExecuteReaderAsync();
            Stream result = null;
            if (reader.HasRows)
            {
                while (reader.Read()) result = reader.GetStream(0);
            }

            reader.Close();

            return result;
        }
    }
}
