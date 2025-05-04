using SortBigFile.LinesOrdering;

namespace SortBigFile.Tests.LinesOrdering;

public class PositionOnLineTests
{
    [Test]
    public void WhenCallInitiate_ThenReturnsStringPart()
    {
        var result = PositionOnLine.Initiate();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StringPart>());
    }
}
