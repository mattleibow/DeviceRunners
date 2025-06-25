using System;

namespace DeviceRunners.SourceGenerators;

/// <summary>
/// Marks a class to generate a complete device test app with all required files.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DeviceTestAppAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the application title.
    /// </summary>
    public string? AppTitle { get; set; }

    /// <summary>
    /// Gets or sets the application identifier (bundle ID).
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Gets or sets the test frameworks to include.
    /// </summary>
    public TestFrameworks TestFrameworks { get; set; } = TestFrameworks.Xunit;
}

/// <summary>
/// Specifies which test frameworks to include in the generated device test app.
/// </summary>
[Flags]
public enum TestFrameworks
{
    /// <summary>
    /// No test frameworks.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Include Xunit test framework.
    /// </summary>
    Xunit = 1,
    
    /// <summary>
    /// Include NUnit test framework.
    /// </summary>
    NUnit = 2,
    
    /// <summary>
    /// Include both Xunit and NUnit test frameworks.
    /// </summary>
    Both = Xunit | NUnit
}