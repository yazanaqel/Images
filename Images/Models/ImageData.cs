namespace Images.Models
{
	public class ImageData
	{
		//if use int -> MD5(MD5(Id)) to protect from bot
		//Guid is less performance than int
		public ImageData() => this.Id = Guid.NewGuid();
		public Guid Id { get; set; }
		public string OriginalFileName { get; set; }
		public string OriginalType { get; set; }
		public byte[] OriginalContent { get; set; }

		public byte[] FullScreenContent { get; set; }

		public byte[] ThumbnailContent { get; set; }

		// Add Foreign key -> public int UserId { get; set; }

	}
}
