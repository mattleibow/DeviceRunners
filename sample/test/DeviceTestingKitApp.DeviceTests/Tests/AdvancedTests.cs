using System.Collections.Generic;

namespace DeviceTestingKitApp.DeviceTests;

public class MemberDataTests
{
public static IEnumerable<object[]> GetAdditionData()
{
yield return new object[] { 2, 3, 5 };
yield return new object[] { 0, 0, 0 };
yield return new object[] { -1, -1, -2 };
}

[Theory]
[MemberData(nameof(GetAdditionData))]
public void MemberDataAddition(int a, int b, int expected)
{
Assert.Equal(expected, a + b);
}
}
