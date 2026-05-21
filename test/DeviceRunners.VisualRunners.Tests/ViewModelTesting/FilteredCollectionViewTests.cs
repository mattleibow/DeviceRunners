using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.ViewModelTesting;

public class FilteredCollectionViewTests
{
	#region IsSynchronized and SyncRoot

	[Fact]
	public void IsSynchronized_ReturnsTrue()
	{
		var source = new ObservableCollection<ObservableTestItem>(
			[new ObservableTestItem("A")]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		// The collection is now internally synchronized
		Assert.True(((ICollection)fcv).IsSynchronized);
	}

	[Fact]
	public void SyncRoot_IsNotThisInstance()
	{
		var source = new ObservableCollection<ObservableTestItem>(
			[new ObservableTestItem("A")]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		// SyncRoot should not be the instance itself (callers using SyncRoot
		// should coordinate with the internal lock)
		var syncRoot = ((ICollection)fcv).SyncRoot;
		Assert.NotNull(syncRoot);
		Assert.NotSame(fcv, syncRoot);
	}

	#endregion

	#region Events fired outside lock (reentrancy safety)

	/// <summary>
	/// Proves that CollectionChanged subscribers can safely call back into the collection
	/// (e.g., read Count, index items) without deadlocking. This would deadlock if
	/// OnCollectionChanged were fired inside the lock on a non-reentrant lock.
	/// </summary>
	[Fact]
	public void CollectionChanged_SubscriberCanReenterCollection_NoDeadlock()
	{
		var source = new ObservableCollection<ObservableTestItem>();

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		int countDuringEvent = -1;
		fcv.CollectionChanged += (_, args) =>
		{
			// Re-enter the collection during the event
			countDuringEvent = fcv.Count;
		};

		// This should not deadlock
		source.Add(new ObservableTestItem("test"));

		Assert.True(countDuringEvent >= 0, "Event handler was called and could re-enter");
	}

	/// <summary>
	/// Proves that ItemChanged subscribers can re-enter the collection without deadlock.
	/// </summary>
	[Fact]
	public void ItemChanged_SubscriberCanReenterCollection_NoDeadlock()
	{
		var item = new ObservableTestItem("test");
		var source = new ObservableCollection<ObservableTestItem>([item]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		int countDuringEvent = -1;
		fcv.ItemChanged += (_, _) =>
		{
			countDuringEvent = fcv.Count;
		};

		// Trigger PropertyChanged → DataSource_ItemChanged → OnItemChanged
		item.Name = "updated";

		Assert.True(countDuringEvent >= 0, "Event handler was called and could re-enter");
	}

	/// <summary>
	/// Verifies that when CollectionChanged fires during DataSource_ItemChanged, 
	/// subscribers can enumerate the collection without getting stale or corrupted data.
	/// </summary>
	[Fact]
	public void CollectionChanged_DuringItemPropertyChange_CanEnumerate()
	{
		var item = new ObservableTestItem("test") { IsActive = false };
		var source = new ObservableCollection<ObservableTestItem>([item]);

		// Filter: only include active items
		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (i, _) => i.IsActive, true, new TestItemComparer());

		Assert.Empty(fcv); // initially filtered out

		List<ObservableTestItem>? snapshotDuringEvent = null;
		fcv.CollectionChanged += (_, args) =>
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
				snapshotDuringEvent = fcv.ToList();
		};

		// Make item active → should be added to filtered view
		item.IsActive = true;

		Assert.NotNull(snapshotDuringEvent);
		Assert.Single(snapshotDuringEvent);
		Assert.Equal("test", snapshotDuringEvent[0].Name);
	}

	#endregion

	#region Dispose thread safety

	[Fact]
	public void Dispose_UnsubscribesFromPropertyChanged()
	{
		var item = new ObservableTestItem("test");
		var source = new ObservableCollection<ObservableTestItem>([item]);

		var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		int changeCount = 0;
		fcv.ItemChanged += (_, _) => Interlocked.Increment(ref changeCount);

		// Verify events fire before dispose
		item.Name = "before";
		Assert.Equal(1, changeCount);

		fcv.Dispose();

		// Events should not fire after dispose
		item.Name = "after";
		Assert.Equal(1, changeCount);
	}

	[Fact]
	public void Dispose_ClearsFilteredList()
	{
		var source = new ObservableCollection<ObservableTestItem>(
			[new ObservableTestItem("A"), new ObservableTestItem("B")]);

		var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		Assert.Equal(2, fcv.Count);

		fcv.Dispose();

		Assert.Empty(fcv);
	}

	[Fact]
	public async Task Dispose_WhileConcurrentPropertyChanges_DoesNotThrow()
	{
		var items = Enumerable.Range(0, 100)
			.Select(i => new ObservableTestItem($"Item_{i:D4}"))
			.ToList();
		var source = new ObservableCollection<ObservableTestItem>(items);

		var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		var exceptions = new List<Exception>();
		var cts = new CancellationTokenSource();

		// Background: continuously mutate items
		var mutator = Task.Run(() =>
		{
			try
			{
				while (!cts.IsCancellationRequested)
				{
					foreach (var item in items)
						item.Name = $"Modified_{Random.Shared.Next()}";
				}
			}
			catch (Exception ex)
			{
				lock (exceptions)
					exceptions.Add(ex);
			}
		});

		// Give mutator time to start
		await Task.Delay(10);

		// Dispose while mutations are happening
		fcv.Dispose();
		cts.Cancel();
		await mutator;

		Assert.Empty(exceptions);
	}

	#endregion

	#region RefreshFilter safety

	[Fact]
	public void RefreshFilter_UpdatesFilteredItems()
	{
		var items = new[]
		{
			new ObservableTestItem("apple") { IsActive = true },
			new ObservableTestItem("banana") { IsActive = false },
			new ObservableTestItem("cherry") { IsActive = true },
		};
		var source = new ObservableCollection<ObservableTestItem>(items);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (item, filterActive) => !filterActive || item.IsActive, true, new TestItemComparer());

		// Initially filtered to active only
		Assert.Equal(2, fcv.Count);

		// Change filter to show all
		fcv.FilterArgument = false;
		Assert.Equal(3, fcv.Count);
	}

	[Fact]
	public void RefreshFilter_FiresResetEvent()
	{
		var source = new ObservableCollection<ObservableTestItem>(
			[new ObservableTestItem("A") { IsActive = true }]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (item, filterActive) => !filterActive || item.IsActive, true, new TestItemComparer());

		NotifyCollectionChangedAction? firedAction = null;
		fcv.CollectionChanged += (_, args) => firedAction = args.Action;

		fcv.FilterArgument = false;

		Assert.Equal(NotifyCollectionChangedAction.Reset, firedAction);
	}

	#endregion

	#region Basic functional tests

	[Fact]
	public void Constructor_PopulatesFromSource()
	{
		var source = new ObservableCollection<ObservableTestItem>(
		[
			new ObservableTestItem("B"),
			new ObservableTestItem("A"),
			new ObservableTestItem("C"),
		]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		// Should be sorted
		Assert.Equal(3, fcv.Count);
		Assert.Equal("A", fcv[0].Name);
		Assert.Equal("B", fcv[1].Name);
		Assert.Equal("C", fcv[2].Name);
	}

	[Fact]
	public void AddToSource_FilteredIn_AppearsInView()
	{
		var source = new ObservableCollection<ObservableTestItem>();

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		source.Add(new ObservableTestItem("hello"));

		Assert.Single(fcv);
		Assert.Equal("hello", fcv[0].Name);
	}

	[Fact]
	public void AddToSource_FilteredOut_DoesNotAppearInView()
	{
		var source = new ObservableCollection<ObservableTestItem>();

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (item, _) => item.IsActive, true, new TestItemComparer());

		source.Add(new ObservableTestItem("hello") { IsActive = false });

		Assert.Empty(fcv);
	}

	[Fact]
	public void RemoveFromSource_RemovesFromView()
	{
		var item = new ObservableTestItem("hello");
		var source = new ObservableCollection<ObservableTestItem>([item]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (_, _) => true, true, new TestItemComparer());

		Assert.Single(fcv);

		source.Remove(item);

		Assert.Empty(fcv);
	}

	[Fact]
	public void ItemPropertyChange_BecomesFiltered_RemovedFromView()
	{
		var item = new ObservableTestItem("test") { IsActive = true };
		var source = new ObservableCollection<ObservableTestItem>([item]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (i, _) => i.IsActive, true, new TestItemComparer());

		Assert.Single(fcv);

		item.IsActive = false;

		Assert.Empty(fcv);
	}

	[Fact]
	public void ItemPropertyChange_BecomesUnfiltered_AddedToView()
	{
		var item = new ObservableTestItem("test") { IsActive = false };
		var source = new ObservableCollection<ObservableTestItem>([item]);

		using var fcv = new FilteredCollectionView<ObservableTestItem, bool>(
			source, (i, _) => i.IsActive, true, new TestItemComparer());

		Assert.Empty(fcv);

		item.IsActive = true;

		Assert.Single(fcv);
	}

	#endregion

	#region Test helpers

	class ObservableTestItem : INotifyPropertyChanged
	{
		string _name;
		bool _isActive = true;

		public ObservableTestItem(string name) => _name = name;

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

	class TestItemComparer : IComparer<ObservableTestItem>
	{
		public int Compare(ObservableTestItem? x, ObservableTestItem? y) =>
			string.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
	}

	#endregion
}
