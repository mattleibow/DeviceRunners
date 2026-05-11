using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DeviceRunners.Cli.Services;

public class WasmWebServerService : IAsyncDisposable
{
	WebApplication? _app;

	public string? BaseUrl { get; private set; }

	public async Task StartAsync(string appPath, int port = 0)
	{
		var fullPath = Path.GetFullPath(appPath);

		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
		builder.Logging.ClearProviders();

		var app = builder.Build();

		// WASM-specific MIME types
		var contentTypeProvider = new FileExtensionContentTypeProvider();
		contentTypeProvider.Mappings[".wasm"] = "application/wasm";
		contentTypeProvider.Mappings[".dll"] = "application/octet-stream";
		contentTypeProvider.Mappings[".pdb"] = "application/octet-stream";
		contentTypeProvider.Mappings[".dat"] = "application/octet-stream";
		contentTypeProvider.Mappings[".blat"] = "application/octet-stream";
		contentTypeProvider.Mappings[".webcil"] = "application/octet-stream";
		contentTypeProvider.Mappings[".cjs"] = "text/javascript";
		contentTypeProvider.Mappings[".mjs"] = "text/javascript";

		// Cross-origin headers for SharedArrayBuffer support
		app.Use(async (context, next) =>
		{
			context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
			context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
			await next();
		});

		var fileProvider = new PhysicalFileProvider(fullPath);

		app.UseDefaultFiles(new DefaultFilesOptions
		{
			FileProvider = fileProvider
		});

		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = fileProvider,
			ContentTypeProvider = contentTypeProvider,
			ServeUnknownFileTypes = true,
			DefaultContentType = "application/octet-stream"
		});

		await app.StartAsync();

		// Read the actual assigned URL (handles port 0 → auto-assigned)
		BaseUrl = app.Urls.FirstOrDefault() ?? $"http://127.0.0.1:{port}";
		_app = app;
	}

	public async ValueTask DisposeAsync()
	{
		if (_app is not null)
		{
			await _app.StopAsync();
			await _app.DisposeAsync();
			_app = null;
		}
	}
}
