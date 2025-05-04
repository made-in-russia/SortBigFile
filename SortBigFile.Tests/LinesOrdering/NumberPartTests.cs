using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.LinesOrdering;

public class NumberPartTests
{
    [Test]
    public void GivenNotEmptyString_WhenCallGetCurrChar_ThenReturnsProperChar()
    {
        var data = Encoding.ASCII.GetBytes("123. ABC");
        var coords = new LineCoords(0, 3, 8);
        var stage = new NumberPart(1);

        var result = stage.GetCurrChar(data, coords);

        Assert.That(result, Is.EqualTo('2'));
    }

    [Test]
    public void GivenEndOfString_WhenCallMoveNext_ThenReturnsNumberPart()
    {
        var coords = new LineCoords(0, 3, 8);
        var stage = new NumberPart(3);

        var result = stage.MoveNext(coords);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NumberPart>());
    }

    [Test]
    public void GivenMiddleOfNumber_WhenCallIsTheEnd_ThenReturnsFalse()
    {
        var coords = new LineCoords(0, 3, 8);
        var stage = new NumberPart(1);

        var result = stage.IsTheEnd(coords);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GivenEndOfNumber_WhenCallIsTheEnd_ThenReturnsTrue()
    {
        var coords = new LineCoords(0, 3, 8);
        var stage = new NumberPart(3);

        var result = stage.IsTheEnd(coords);

        Assert.That(result, Is.True);
    }
}
