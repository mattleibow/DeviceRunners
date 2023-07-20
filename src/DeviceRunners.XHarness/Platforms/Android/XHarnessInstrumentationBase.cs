using Android.Content;
using Android.Provider;
using Android.Runtime;

namespace DeviceRunners.XHarness;

public abstract class XHarnessInstrumentationBase : Instrumentation
{
	readonly TaskCompletionSource<Application> _waitForApplication = new();

	protected XHarnessInstrumentationBase(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected Bundle? Arguments { get; private set; }

	public override void OnCreate(Bundle? arguments)
	{
		Console.WriteLine("Detected that the entry point was through the XHarness instrumentation, so will request an XHarness test runner.");

		// This will mark the XHarness runner as an option since we are coming in directly from an instrumentation.
		// For iOS or Mac Catalyst, we use environment variables.
		XHarnessDetector.IsUsingXHarness = true;

		Arguments = arguments;

		base.OnCreate(arguments);

		Start();
	}

	public override void CallApplicationOnCreate(Application? app)
	{
		base.CallApplicationOnCreate(app);

		if (app is null)
			_waitForApplication.SetException(new ArgumentNullException(nameof(app)));
		else
			_waitForApplication.SetResult(app);
	}

	public override async void OnStart()
	{
		base.OnStart();

		var app = await _waitForApplication.Task;

		OnApplicationStart(app);
	}

	public async void OnApplicationStart(Application app)
	{
		var tcs = new TaskCompletionSource<ITestRunResult>();

		var vm = GetHomeViewModel(app);
		vm.TestRunCompleted += OnTestRunCompleted;

		var activity = StartTestActivity(app);

		var result = await tcs.Task;

		CopyFile(result);

		// activity.Finish();

		Finish(Result.Ok, result.ToBundle());

		void OnTestRunCompleted(object? sender, ITestRunResult result) =>
			tcs.SetResult(result);
	}

	protected abstract HomeViewModel GetHomeViewModel(Application app);

	protected virtual Activity StartTestActivity(Application app)
	{
		var pm = Context!.PackageManager!;
		var intent = pm.GetLaunchIntentForPackage(Context.PackageName!);
		return StartActivitySync(intent)!;
	}

	void CopyFile(ITestRunResult result)
	{
		var resultsFile = result["test-results-path"];
		if (resultsFile is null)
			return;

		var guid = Guid.NewGuid().ToString("N");
		var name = Path.GetFileName(resultsFile);

		string finalPath;
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath!;
			var dir = Path.Combine(root, guid);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			finalPath = Path.Combine(dir, name);
			File.Copy(resultsFile, finalPath, true);
		}
		else
		{
			var downloads = Android.OS.Environment.DirectoryDownloads!;
			var relative = Path.Combine(downloads, Context!.PackageName!, guid);

			var values = new ContentValues();
			values.Put(MediaStore.IMediaColumns.DisplayName, name);
			values.Put(MediaStore.IMediaColumns.MimeType, "text/xml");
			values.Put(MediaStore.IMediaColumns.RelativePath, relative);

			var resolver = Context!.ContentResolver!;
			var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)!;
			using (var dest = resolver.OpenOutputStream(uri)!)
			using (var source = File.OpenRead(resultsFile))
				source.CopyTo(dest);

#pragma warning disable CS0618 // Type or member is obsolete
			var root = Android.OS.Environment.ExternalStorageDirectory!.AbsolutePath;
#pragma warning restore CS0618 // Type or member is obsolete
			finalPath = Path.Combine(root, relative, name);
		}

		result["test-results-path"] = finalPath;
	}
}
