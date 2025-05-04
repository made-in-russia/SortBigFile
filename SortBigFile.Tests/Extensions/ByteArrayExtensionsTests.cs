using SortBigFile.Extensions;
using SortBigFile.Models;
using System.Text;

namespace SortBigFile.Tests.Extensions;

internal class ByteArrayExtensionsTests
{
    [Test]
    public void WhenCallExecInParallel_ThenRunsInParallel()
    {
        var str = string.Join(string.Empty, Enumerable.Repeat("1\n", Environment.ProcessorCount));
        var data = Encoding.ASCII.GetBytes(str);

        var start = DateTime.Now;
        data.ExecInParallel((_, _) => Thread.Sleep(100));
        var end = DateTime.Now;

        Assert.That(end - start, Is
            .GreaterThan(TimeSpan.FromMilliseconds(50))
            .And
            .LessThan(TimeSpan.FromMilliseconds(150)));
    }

    [Test]
    public void WhenCallGetNextLineCoords_ThenReturnsNextLineCoords()
    {
        var data = Encoding.ASCII.GetBytes("aaa\n111. bbb\nccc");

        var result = data.GetNextLineCoords(3);

        Assert.That(result, Is.EqualTo(new LineCoords(4, 7, 12)));
    }
}
