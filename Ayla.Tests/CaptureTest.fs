namespace Ayla.Tests

open NUnit.Framework

[<TestFixture>]
type CaptureTest() =

    [<Test>]
    member x.TestCapture1() =
        Assert.AreEqual(1,1)