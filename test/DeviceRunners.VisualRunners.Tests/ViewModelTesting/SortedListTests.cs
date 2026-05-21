using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.ViewModelTesting;

public class SortedListTests
{
	static readonly IComparer<string> OrdinalComparer =
		Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.Ordinal));

	[Fact]
	public void NewList_IsEmpty()
	{
		var list = new SortedList<string>(OrdinalComparer);

		Assert.Empty(list);
	}

	[Fact]
	public void Add_SingleItem_ContainsItem()
	{
		var list = new SortedList<string>(OrdinalComparer);

		list.Add("hello");

		Assert.Single(list);
		Assert.Contains("hello", list);
	}

	[Fact]
	public void Add_MultipleItems_MaintainsSortOrder()
	{
		var list = new SortedList<string>(OrdinalComparer);

		list.Add("cherry");
		list.Add("apple");
		list.Add("banana");

		Assert.Equal(3, list.Count);
		Assert.Equal("apple", list[0]);
		Assert.Equal("banana", list[1]);
		Assert.Equal("cherry", list[2]);
	}

	[Fact]
	public void IndexOf_ExistingItem_ReturnsCorrectIndex()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("banana");
		list.Add("cherry");

		Assert.Equal(0, list.IndexOf("apple"));
		Assert.Equal(1, list.IndexOf("banana"));
		Assert.Equal(2, list.IndexOf("cherry"));
	}

	[Fact]
	public void IndexOf_MissingItem_ReturnsBitwiseComplementOfInsertionPoint()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("cherry");

		var index = list.IndexOf("banana");

		Assert.True(index < 0);
		// Insertion point should be 1 (between apple and cherry)
		Assert.Equal(1, ~index);
	}

	[Fact]
	public void IndexOf_EmptyList_ReturnsComplementOfZero()
	{
		var list = new SortedList<string>(OrdinalComparer);

		var index = list.IndexOf("anything");

		Assert.True(index < 0);
		Assert.Equal(0, ~index);
	}

	[Fact]
	public void Insert_AtCorrectPosition_MaintainsOrder()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("cherry");

		// Insert "banana" at position 1 (caller computes sorted index)
		list.Insert(1, "banana");

		Assert.Equal("apple", list[0]);
		Assert.Equal("banana", list[1]);
		Assert.Equal("cherry", list[2]);
	}

	[Fact]
	public void Remove_ExistingItem_RemovesAndReturnsTrue()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("banana");
		list.Add("cherry");

		var result = list.Remove("banana");

		Assert.True(result);
		Assert.Equal(2, list.Count);
		Assert.DoesNotContain("banana", list);
		Assert.Equal("apple", list[0]);
		Assert.Equal("cherry", list[1]);
	}

	[Fact]
	public void Remove_MissingItem_ReturnsFalse()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");

		var result = list.Remove("banana");

		Assert.False(result);
		Assert.Single(list);
	}

	[Fact]
	public void RemoveAt_ValidIndex_RemovesItem()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("banana");
		list.Add("cherry");

		list.RemoveAt(1);

		Assert.Equal(2, list.Count);
		Assert.Equal("apple", list[0]);
		Assert.Equal("cherry", list[1]);
	}

	[Fact]
	public void Clear_RemovesAllItems()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("banana");

		list.Clear();

		Assert.Empty(list);
	}

	[Fact]
	public void Contains_ExistingItem_ReturnsTrue()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");

		Assert.Contains("apple", list);
	}

	[Fact]
	public void Contains_MissingItem_ReturnsFalse()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");

		Assert.DoesNotContain("banana", list);
	}

	[Fact]
	public void CopyTo_CopiesToArray()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("banana");
		list.Add("apple");

		var array = new string[3];
		list.CopyTo(array, 1);

		Assert.Null(array[0]);
		Assert.Equal("apple", array[1]);
		Assert.Equal("banana", array[2]);
	}

	[Fact]
	public void Indexer_ValidIndex_ReturnsItem()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("banana");
		list.Add("apple");

		Assert.Equal("apple", list[0]);
		Assert.Equal("banana", list[1]);
	}

	[Fact]
	public void Indexer_Set_ThrowsNotSupported()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");

		Assert.Throws<NotSupportedException>(() => list[0] = "banana");
	}

	[Fact]
	public void IsReadOnly_ReturnsFalse()
	{
		var list = new SortedList<string>(OrdinalComparer);

		Assert.False(list.IsReadOnly);
	}

	[Fact]
	public void Enumeration_ReturnsItemsInSortedOrder()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("cherry");
		list.Add("apple");
		list.Add("banana");

		var enumerated = list.ToList();

		Assert.Equal(new[] { "apple", "banana", "cherry" }, enumerated);
	}

	[Fact]
	public void Add_DuplicateItems_AllowsDuplicates()
	{
		var list = new SortedList<string>(OrdinalComparer);
		list.Add("apple");
		list.Add("apple");

		Assert.Equal(2, list.Count);
		Assert.Equal("apple", list[0]);
		Assert.Equal("apple", list[1]);
	}

	[Fact]
	public void Constructor_NullComparer_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new SortedList<string>(null!));
	}
}
