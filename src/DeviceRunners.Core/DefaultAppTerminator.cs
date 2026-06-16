namespace DeviceRunners.Core;

public class DefaultAppTerminator : IAppTerminator
{
	public void Terminate()
	{
		// Exit the process directly rather than going through any platform-specific
		// graceful shutdown.
		//
		// In particular, on Apple platforms we deliberately avoid asking AppKit/UIKit
		// to terminate via the "terminateWithSuccess" selector. On Mac Catalyst that
		// graceful terminate cascade propagates a UIKit trait-collection change down the
		// MAUI Shell view hierarchy *after* MAUI has already disposed its DI container
		// during shutdown. That makes ShellSectionRootRenderer.TraitCollectionDidChange
		// call GetService on a disposed IServiceProvider, throwing ObjectDisposedException,
		// which surfaces as an uncaught NSException and raises SIGABRT with a native crash
		// report.
		//
		// The host CLI/harness derives the real test exit code from the result
		// channel it receives (TRX file and the TCP result stream), independently of
		// this process' own exit code, so a hard exit here does not change the reported
		// result. Any results produced during the run are streamed/written before
		// termination is requested; if a run aborted early without producing results, a
		// graceful AppKit terminate would not recover them either, so exiting immediately
		// is no worse and additionally avoids the SIGABRT above.
		Environment.Exit(0);
	}
}
