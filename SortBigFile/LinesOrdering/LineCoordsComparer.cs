using SortBigFile.Models;

namespace SortBigFile.LinesOrdering;

public class LineCoordsComparer(byte[] Buffer) : IComparer<LineCoords>
{
    public int Compare(LineCoords? c1, LineCoords? c2)
    {
        if (c1 == null || c2 == null) return default;

        var result = Buffer.AsSpan(c1.StringStart, c1.StringLength)
            .SequenceCompareTo(Buffer.AsSpan(c2.StringStart, c2.StringLength));
        if (result != 0) return result;

        result = c1.NumberLength.CompareTo(c2.NumberLength);
        if (result != 0) return result;

        result = Buffer.AsSpan(c1.NumberStart, c1.NumberLength)
            .SequenceCompareTo(Buffer.AsSpan(c2.NumberStart, c2.NumberLength));
        return result;
    }
}
