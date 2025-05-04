using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.LinesOrdering;

public class LineCoordsComparerTests
{
    [Test]
    public void WhenFirstStringIsGrater_ThenReturnsOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("111. BBB\n111. AAA\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 12, 17);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void WhenFirstNumberLengthIsGrater_ThenReturnsOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("1111. AAA\n111. AAA\n");
        LineCoords c1 = new(0, 4, 9);
        LineCoords c2 = new(10, 13, 18);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void WhenFirstNumberIsGrater_ThenReturnsOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("222. AAA\n111. AAA\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 12, 17);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void WhenBothLinesAreEqual_ThenReturnsZero()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("111. AAA\n111. AAA\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 12, 17);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void WhenSecondStringIsGrater_ThenReturnsMinusOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("111. AAA\n111. BBB\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 12, 17);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void WhenSecondNumberLengthIsGrater_ThenReturnsMinusOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("111. AAA\n1111. AAA\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 13, 18);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void WhenSecondNumberIsGrater_ThenReturnsMinusOne()
    {
        byte[] buffer = Encoding.ASCII.GetBytes("111. AAA\n222. AAA\n");
        LineCoords c1 = new(0, 3, 8);
        LineCoords c2 = new(9, 12, 17);

        var comparer = new LineCoordsComparer(buffer);

        var result = comparer.Compare(c1, c2);

        Assert.That(result, Is.EqualTo(-1));
    }
}