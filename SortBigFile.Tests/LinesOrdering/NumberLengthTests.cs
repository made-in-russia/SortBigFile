using Moq;
using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.LinesOrdering;

public class NumberLengthTests
{
    [Test]
    public void WhenCallGetCurrChar_ThenReturnsNumberLengthCodedChar()
    {
        var coords = new LineCoords(0, 3, 8);
        var numberLengthChar = (char)3;
        var stage = new NumberLength();

        var result = stage.GetCurrChar(It.IsAny<byte[]>(), coords);

        Assert.That(result, Is.EqualTo(numberLengthChar));
    }

    [Test]
    public void WhenCallMoveNext_ThenReturnsNumberPart()
    {
        var stage = new NumberLength();

        var result = stage.MoveNext(It.IsAny<LineCoords>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NumberPart>());
    }

    [Test]
    public void WhenCallIsTheEnd_ThenReturnsFalse()
    {
        var stage = new NumberLength();

        var result = stage.IsTheEnd(It.IsAny<LineCoords>());

        Assert.That(result, Is.False);
    }
}
