global using Xunit;

// This app hosts tests from multiple frameworks. Referencing the MSTest sample brings MSTest's
// implicit `global using Microsoft.VisualStudio.TestTools.UnitTesting`, whose `Assert` collides
// with xUnit's. The app's own inline tests are xUnit, so pin the unqualified name to xUnit.
global using Assert = Xunit.Assert;
