using Moq;
using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.LinesOrdering;

internal class StringPartTests
{
    [Test]
    public void GivenNotEmptyString_WhenCallGetCurrChar_ThenReturnsProperChar()
    {
        var data = Encoding.ASCII.GetBytes("123. ABC");
        var coords = new LineCoords(0, 3, 8);
        var stage = new StringPart(1);

        var result = stage.GetCurrChar(data, coords);

        Assert.That(result, Is.EqualTo('B'));
    }

    [Test]
    public void GivenEndOfString_WhenCallMoveNext_ThenReturnsNumberLength()
    {
        var coords = new LineCoords(0, 3, 8);
        var stage = new StringPart(3);
        
        var result = stage.MoveNext(coords);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NumberLength>());
    }

    [Test]
    public void WhenCallIsTheEnd_ThenReturnsFalse()
    {
        var stage = new StringPart(default);

        var result = stage.IsTheEnd(It.IsAny<LineCoords>());

        Assert.That(result, Is.False);
    }
}
