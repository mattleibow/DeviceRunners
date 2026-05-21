using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DeviceRunners.VisualRunners;

class FilteredCollectionView<T, TFilterArg> : IList<T>, IList, INotifyCollectionChanged, IDisposable
{
	readonly ObservableCollection<T> dataSource;
	readonly Func<T, TFilterArg, bool> filter;
	readonly SortedList<T> filteredList;
	readonly object _syncLock = new();

	TFilterArg filterArgument;

	public FilteredCollectionView(ObservableCollection<T> dataSource, Func<T, TFilterArg, bool> filter, TFilterArg filterArgument, IComparer<T> sort)
	{
		if (sort == null)
			throw new ArgumentNullException(nameof(sort));

		this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
		this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
		this.filterArgument = filterArgument;
		filteredList = new SortedList<T>(sort);

		this.dataSource.CollectionChanged += DataSource_CollectionChanged;

		foreach (var item in this.dataSource)
		{
			OnAdded(item);
		}
	}

	public TFilterArg FilterArgument
	{
		get { return filterArgument; }
		set
		{
			if (EqualityComparer<TFilterArg>.Default.Equals(filterArgument, value))
			{
				return;
			}

			filterArgument = value;
			RefreshFilter();
		}
	}

	public void Dispose()
	{
		dataSource.CollectionChanged -= DataSource_CollectionChanged;

		foreach (var item in dataSource.OfType<INotifyPropertyChanged>())
		{
			item.PropertyChanged -= DataSource_ItemChanged;
		}

		lock (_syncLock)
		{
			filteredList.Clear();
		}
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object? value)
	{
		return Contains((T)value!);
	}

	int IList.IndexOf(object? value)
	{
		return IndexOf((T)value!);
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	bool IList.IsFixedSize => false;

	void IList.Remove(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	object? IList.this[int index]
	{
		get { return this[index]; }
		set { throw new NotSupportedException(); }
	}

	void ICollection.CopyTo(Array array, int index)
	{
		lock (_syncLock)
			filteredList.CopyTo((T[])array, index);
	}

	bool ICollection.IsSynchronized => true;

	object ICollection.SyncRoot => _syncLock;

	public void Add(T item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		lock (_syncLock)
			return filteredList.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		lock (_syncLock)
			filteredList.CopyTo(array, arrayIndex);
	}

	public int Count
	{
		get
		{
			lock (_syncLock)
				return filteredList.Count;
		}
	}

	public bool IsReadOnly => true;

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		List<T> snapshot;
		lock (_syncLock)
			snapshot = filteredList.ToList();
		return snapshot.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public int IndexOf(T item)
	{
		lock (_syncLock)
			return filteredList.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public T this[int index]
	{
		get
		{
			lock (_syncLock)
				return filteredList[index];
		}
		set { throw new NotSupportedException(); }
	}

	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	/// <summary>
	///     Raised when one of the items selected by the filter is changed.
	/// </summary>
	/// <remarks>
	///     The sender is reported to be the item changed.
	/// </remarks>
	public event EventHandler<PropertyChangedEventArgs>? ItemChanged;

	protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		var collectionChanged = CollectionChanged;
		collectionChanged?.Invoke(this, args);
	}

	protected virtual void OnItemChanged(T sender, PropertyChangedEventArgs args)
	{
		var itemChanged = ItemChanged;
		itemChanged?.Invoke(sender, args);
	}

	void DataSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
				foreach (T item in e.NewItems!)
				{
					OnAdded(item);
				}

				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (T item in e.OldItems!)
				{
					OnRemoved(item);
				}

				break;
			case NotifyCollectionChangedAction.Replace:
				foreach (T item in e.OldItems!)
				{
					OnRemoved(item);
				}

				foreach (T item in e.NewItems!)
				{
					OnAdded(item);
				}

				break;
			case NotifyCollectionChangedAction.Reset:
				throw new NotSupportedException();
		}
	}

	void DataSource_ItemChanged(object? sender, PropertyChangedEventArgs e)
	{
		var item = (T)sender!;
		bool changed = false;

		lock (_syncLock)
		{
			var index = filteredList.IndexOf(item);
			if (filter(item, FilterArgument))
			{
				if (index < 0)
				{
					filteredList.Insert(~index, item);
					changed = true;
				}
			}
			else if (index >= 0)
			{
				filteredList.RemoveAt(index);
				changed = true;
			}
		}

		// Use Reset rather than Add/Remove with index — under concurrent mutations
		// the index captured inside the lock may be stale by the time subscribers observe it.
		if (changed)
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

		OnItemChanged(item, e);
	}

	void OnAdded(T item)
	{
		NotifyCollectionChangedEventArgs? changeArgs = null;

		lock (_syncLock)
		{
			if (filter(item, filterArgument))
			{
				var index = filteredList.IndexOf(item);
				if (index < 0)
				{
					filteredList.Insert(~index, item);
					changeArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ~index);
				}
			}
		}

		if (changeArgs is not null)
			OnCollectionChanged(changeArgs);

		if (item is INotifyPropertyChanged observable)
		{
			observable.PropertyChanged += DataSource_ItemChanged;
		}
	}

	void OnRemoved(T item)
	{
		if (item is INotifyPropertyChanged observable)
		{
			observable.PropertyChanged -= DataSource_ItemChanged;
		}

		NotifyCollectionChangedEventArgs? changeArgs = null;

		lock (_syncLock)
		{
			var index = filteredList.IndexOf(item);
			if (index >= 0)
			{
				filteredList.RemoveAt(index);
				changeArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
			}
		}

		if (changeArgs is not null)
			OnCollectionChanged(changeArgs);
	}

	void RefreshFilter()
	{
		// Note: dataSource (ObservableCollection) is not thread-safe for enumeration.
		// This method assumes dataSource is not being structurally modified concurrently.
		// In practice, dataSource (_allTests) is populated once at construction and never changed.
		lock (_syncLock)
		{
			filteredList.Clear();

			foreach (var item in dataSource)
			{
				if (filter(item, filterArgument))
				{
					filteredList.Add(item);
				}
			}
		}

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}
}
