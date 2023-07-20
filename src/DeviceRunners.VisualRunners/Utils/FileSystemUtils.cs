using System.Reflection;

namespace DeviceRunners.VisualRunners;

public class FileSystemUtils
{
#if WINDOWS
	static readonly Lazy<bool> IsPackagedApp = new Lazy<bool>(() =>
	{
		try
		{
			if (Windows.ApplicationModel.Package.Current != null)
				return true;
		}
		catch
		{
			// no-op
		}

		return false;
	});
#endif

	public static Stream? OpenAppPackageFile(string filename)
	{
		if (filename is null)
			throw new ArgumentNullException(nameof(filename));

		filename = NormalizePath(filename);

#if ANDROID
		// Android: We can only check the assets folder
		try
		{
			return Application.Context.Assets.Open(filename);
		}
		catch (Java.IO.FileNotFoundException)
		{
		}
#endif

#if IOS || MACCATALYST || MACOS
		// macOS/iOS: we use a path next to the executable
		var root = NSBundle.MainBundle.BundlePath;
#if MACCATALYST || MACOS
		// macOS is actually in a sub folder
		root = Path.Combine(root, "Contents", "Resources");
#endif
		var file = Path.Combine(root, filename);
#elif WINDOWS
		// Windows: has 2 modes, packaged uses the Package API
		if (IsPackagedApp.Value)
		{
			try
			{
				return Windows.ApplicationModel.Package.Current.InstalledLocation.OpenStreamForReadAsync(filename).Result;
			}
			catch (AggregateException ex) when (ex.InnerException is FileNotFoundException)
			{
			}
			catch (FileNotFoundException)
			{
			}
		}

		// unpackaged is next to the executable
		var root = AppContext.BaseDirectory;
		var file = Path.Combine(root, filename);
#else
		var file = filename;
#endif

		// if the file exists, then open it
		if (File.Exists(file))
			return File.OpenRead(file);

		// null is a valid way of saying no file
		return null;
	}

	public static string GetAssemblyFileName(Assembly assm)
	{
		var filename = assm.GetName().Name + ".dll";

#if WINDOWS
		string root;
		if (IsPackagedApp.Value)
			root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
		else
			root = AppContext.BaseDirectory;
		return Path.Combine(root, filename);
#elif ANDROID
		// this is required to exist (although not actually used) so we create a dummy file
		var root = Android.App.Application.Context.CacheDir.AbsolutePath;
		var path = Path.Combine(root, filename);
		if (!File.Exists(path))
			File.Create(path).Close();
		return path;
#else
		return assm.Location;
#endif
	}

	static string NormalizePath(string filename) =>
		filename
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar);
}
