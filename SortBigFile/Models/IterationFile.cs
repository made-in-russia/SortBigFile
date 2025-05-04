using SortBigFile.LinesOrdering;
using System.Collections.Concurrent;

namespace SortBigFile.Models;

public class IterationFile
{
    public required string Name { get; set; }

    public required PositionOnLine CommonPartPos { get; set; }

    public required string CommonPart { get; set; }

    public long Size { get; set; } = 0;

    public bool DeleteAfter { get; set; }

    public ConcurrentBag<LineCoords> LinesCoords { get; set; } = [];
}
