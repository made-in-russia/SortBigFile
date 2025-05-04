using Moq;
using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.LinesOrdering;

public class PositionSeekerTests
{
    [Test]
    public void WhenChangeIsFound_ThenItIsChanging()
    {
        var data = Encoding.ASCII.GetBytes("111. AAA\n222. BBB");
        var referenceCoords = new LineCoords(0, 3, 8);
        var seeker = new PositionSeeker(data, referenceCoords, It.IsAny<string>(), It.IsAny<PositionOnLine>());

        seeker.IsFound = true;
        var result = seeker.IsFound;

        Assert.That(result, Is.True);
    }

    [Test]
    public void GivenValuesAreDifferent_WhenCallNonEqual_ThenReturnsTrue()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC\n222. AAA");
        var referenceCoords = new LineCoords(0, 3, 8);
        var coordsToCompare = new LineCoords(9, 12, 17);
        var commonStart = new StringPart(1);
        var seeker = new PositionSeeker(data, referenceCoords, It.IsAny<string>(), commonStart);

        var result = seeker.NonEqual(coordsToCompare);

        Assert.That(result, Is.True);
    }

    [Test]
    public void GivenValuesAreEqual_WhenCallNonEqual_ThenReturnsFalse()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC\n222. BBB");
        var referenceCoords = new LineCoords(0, 3, 8);
        var coordsToCompare = new LineCoords(9, 12, 17);
        var position = new StringPart(1);
        var seeker = new PositionSeeker(data, referenceCoords, It.IsAny<string>(), position);

        var result = seeker.NonEqual(coordsToCompare);

        Assert.That(result, Is.False);
    }

    [Test]
    public void WhenCallMoveNext_ThenCommonPartIncreases()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC");
        var numberLengthChar = (char)3;
        var referenceCoords = new LineCoords(0, 3, 8);
        var position = new StringPart(2);
        var seeker = new PositionSeeker(data, referenceCoords, string.Empty, position);
        var results = new List<string>();

        seeker.MoveNext();
        results.Add(seeker.CommonPart);

        seeker.MoveNext();
        results.Add(seeker.CommonPart);

        seeker.MoveNext();
        results.Add(seeker.CommonPart);

        Assert.That(results, Is.EqualTo(new List<string>() { "C", $"C{numberLengthChar}", $"C{numberLengthChar}1" }));
    }

    [Test]
    public void WhenCallMoveNext_ThenPositionIncreases()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC");
        var referenceCoords = new LineCoords(0, 3, 8);
        var position = new StringPart(1);
        var seeker = new PositionSeeker(data, referenceCoords, string.Empty, position);
        var results = new List<PositionOnLine>();

        seeker.MoveNext();
        results.Add(seeker.Position);

        seeker.MoveNext();
        results.Add(seeker.Position);

        seeker.MoveNext();
        results.Add(seeker.Position);

        Assert.That(results[0], Is.InstanceOf<StringPart>());
        Assert.That(results[1], Is.InstanceOf<NumberLength>());
        Assert.That(results[2], Is.InstanceOf<NumberPart>());
    }

    [Test]
    public void GivenPositionAtTheEnd_WhenCallCouldNotFind_ThenReturnsTrue()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC");
        var referenceCoords = new LineCoords(0, 3, 8);
        var positionMock = new Mock<PositionOnLine>();
        positionMock.Setup(x => x.IsTheEnd(It.IsAny<LineCoords>())).Returns(true);
        var seeker = new PositionSeeker(data, referenceCoords, string.Empty, positionMock.Object);

        var result = seeker.CouldNotFind();

        Assert.That(result, Is.True);
    }

    [Test]
    public void GivenPositionInProgress_WhenCallCouldNotFind_ThenReturnsFalse()
    {
        var data = Encoding.ASCII.GetBytes("111. ABC");
        var referenceCoords = new LineCoords(0, 3, 8);
        var positionMock = new Mock<PositionOnLine>();
        positionMock.Setup(x => x.IsTheEnd(It.IsAny<LineCoords>())).Returns(false);
        var seeker = new PositionSeeker(data, referenceCoords, string.Empty, positionMock.Object);

        var result = seeker.CouldNotFind();

        Assert.That(result, Is.False);
    }
}
