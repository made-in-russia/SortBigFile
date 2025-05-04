namespace SortBigFile.Models;

public record LineCoords(int LineStart, int PointIndex, int LineEnd)
{
    public int NumberStart => LineStart;

    public int NumberEnd => PointIndex;

    public int NumberLength => NumberEnd - NumberStart;

    public int StringStart => PointIndex + 2;

    public int StringEnd => LineEnd;

    public int StringLength => StringEnd - StringStart;

    public int Length => LineEnd - LineStart;
}
