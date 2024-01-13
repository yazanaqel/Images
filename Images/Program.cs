using Azure;
using Images;
using Images.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft .Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IImageService, ImageService>();

builder.Services.AddTransient<IFileImageService, FileImageService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = option =>
    {
		var headers = option.Context.Response.GetTypedHeaders();

		headers.CacheControl = new CacheControlHeaderValue
		{
			Public = true,
			MaxAge = TimeSpan.FromDays(30)
		};
		headers.Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30));
	}
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Images}/{action=Upload}/{id?}");

app.Run();
