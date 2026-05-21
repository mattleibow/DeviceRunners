using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.ViewModelTesting;

public class FilteredCollectionViewConcurrencyTests
{
	/// <summary>
	/// Reproduces the race condition from issue #132:
	/// Multiple threads fire PropertyChanged on items in a FilteredCollectionView concurrently,
	/// causing DataSource_ItemChanged to corrupt the internal SortedList.
	/// </summary>
	[Fact]
	public void ConcurrentPropertyChangedDoesNotCorruptFilteredCollectionView()
	{
		const int itemCount = 200;
		const int iterations = 50;

		var items = Enumerable.Range(0, itemCount)
			.Select(i => new ObservableItem { Name = $"Item_{i:D4}", IsActive = true })
			.ToList();

		var source = new ObservableCollection<ObservableItem>(items);

		using var fcv = new FilteredCollectionView<ObservableItem, bool>(
			source,
			(item, filterArg) => item.IsActive || !filterArg,
			true,
			new NameComparer());

		// Verify initial state
		Assert.Equal(itemCount, fcv.Count);

		var exceptions = new List<Exception>();

		// Hammer PropertyChanged from multiple threads simultaneously
		Parallel.For(0, iterations, _ =>
		{
			try
			{
				foreach (var item in items)
				{
					// Toggle IsActive which fires PropertyChanged, triggering DataSource_ItemChanged
					item.IsActive = !item.IsActive;
				}
			}
			catch (Exception ex)
			{
				lock (exceptions)
					exceptions.Add(ex);
			}
		});

		// Should not have thrown any exceptions (without the fix, this will likely throw
		// ArgumentOutOfRangeException, InvalidOperationException, or crash with SIGSEGV)
		Assert.Empty(exceptions);

		// The collection should still be enumerable without throwing
		var snapshot = fcv.ToList();
		Assert.NotNull(snapshot);
	}

	/// <summary>
	/// Verifies that enumerating a FilteredCollectionView while items change concurrently
	/// does not throw InvalidOperationException ("Collection was modified").
	/// </summary>
	[Fact]
	public async Task EnumeratingWhileItemsChangeConcurrentlyDoesNotThrow()
	{
		const int itemCount = 100;

		var items = Enumerable.Range(0, itemCount)
			.Select(i => new ObservableItem { Name = $"Item_{i:D4}", IsActive = true })
			.ToList();

		var source = new ObservableCollection<ObservableItem>(items);

		using var fcv = new FilteredCollectionView<ObservableItem, bool>(
			source,
			(item, _) => item.IsActive,
			true,
			new NameComparer());

		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
		var exceptions = new List<Exception>();

		// Background thread: continuously toggle item properties
		var mutator = Task.Run(() =>
		{
			while (!cts.IsCancellationRequested)
			{
				foreach (var item in items)
					item.IsActive = !item.IsActive;
			}
		});

		// Main thread: continuously enumerate
		while (!cts.IsCancellationRequested)
		{
			try
			{
				// This is the operation that crashes in the bug report (LINQ GroupBy/ToDictionary)
				var grouped = fcv.GroupBy(i => i.IsActive).ToDictionary(g => g.Key, g => g.Count());
			}
			catch (Exception ex) when (ex is not OutOfMemoryException)
			{
				lock (exceptions)
					exceptions.Add(ex);
				break;
			}
		}

		cts.Cancel();
		await mutator;

		Assert.Empty(exceptions);
	}

	/// <summary>
	/// Reproduces the TestAssemblyViewModel race: concurrent ResultReported events
	/// should not corrupt the result counts.
	/// </summary>
	[Fact]
	public void ConcurrentResultReportingDoesNotCorruptTestAssemblyCounts()
	{
		const int testCount = 500;

		// Create mock test cases that support ResultReported
		var testCases = Enumerable.Range(0, testCount)
			.Select(i =>
			{
				var assembly = Substitute.For<ITestAssemblyInfo>();
				assembly.AssemblyFileName.Returns("Test.dll");
				assembly.TestCases.Returns(Array.Empty<ITestCaseInfo>());

				var tc = new FakeTestCaseInfo($"Test_{i:D4}", assembly);
				return tc;
			})
			.ToList();

		var assemblyInfo = Substitute.For<ITestAssemblyInfo>();
		assemblyInfo.AssemblyFileName.Returns("Test.dll");
		assemblyInfo.TestCases.Returns(testCases.Cast<ITestCaseInfo>().ToList());

		var runner = Substitute.For<ITestRunner>();
		var vm = new TestAssemblyViewModel(assemblyInfo, runner);

		Assert.Equal(testCount, vm.TestCases.Count);

		// Fire ResultReported from multiple threads simultaneously
		var statuses = new[] { TestResultStatus.Passed, TestResultStatus.Failed, TestResultStatus.Skipped };
		var exceptions = new List<Exception>();

		Parallel.ForEach(testCases, tc =>
		{
			try
			{
				var status = statuses[(tc.DisplayName.GetHashCode() & int.MaxValue) % statuses.Length];
				var result = new FakeTestResultInfo(tc, status);
				tc.FireResultReported(result);
			}
			catch (Exception ex)
			{
				lock (exceptions)
					exceptions.Add(ex);
			}
		});

		Assert.Empty(exceptions);

		// Counts should sum to total (no corruption)
		var total = vm.Passed + vm.Failed + vm.Skipped + vm.NotRun;
		Assert.Equal(testCount, total);
	}

	#region Test helpers

	class ObservableItem : INotifyPropertyChanged
	{
		string _name = string.Empty;
		bool _isActive;

		public string Name
		{
			get => _name;
			set { _name = value; OnPropertyChanged(); }
		}

		public bool IsActive
		{
			get => _isActive;
			set { _isActive = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	class NameComparer : IComparer<ObservableItem>
	{
		public int Compare(ObservableItem? x, ObservableItem? y) =>
			string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
	}

	class FakeTestCaseInfo : ITestCaseInfo
	{
		public FakeTestCaseInfo(string displayName, ITestAssemblyInfo assembly)
		{
			DisplayName = displayName;
			TestAssembly = assembly;
		}

		public ITestAssemblyInfo TestAssembly { get; }
		public string DisplayName { get; }
		public ITestResultInfo? Result { get; private set; }

		public event Action<ITestResultInfo>? ResultReported;

		public void FireResultReported(ITestResultInfo result)
		{
			Result = result;
			ResultReported?.Invoke(result);
		}
	}

	class FakeTestResultInfo : ITestResultInfo
	{
		public FakeTestResultInfo(ITestCaseInfo testCase, TestResultStatus status)
		{
			TestCase = testCase;
			Status = status;
			Duration = TimeSpan.FromMilliseconds(10);
		}

		public ITestCaseInfo TestCase { get; }
		public TestResultStatus Status { get; }
		public TimeSpan Duration { get; }
		public string? Output => null;
		public string? ErrorMessage => Status == TestResultStatus.Failed ? "Test failed" : null;
		public string? ErrorStackTrace => null;
		public string? SkipReason => Status == TestResultStatus.Skipped ? "Skipped" : null;
	}

	#endregion
}
