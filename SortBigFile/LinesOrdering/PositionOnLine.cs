using SortBigFile.Models;

namespace SortBigFile.LinesOrdering;

public abstract class PositionOnLine
{
    public static PositionOnLine Initiate() => new StringPart();

    public abstract char GetCurrChar(byte[] data, LineCoords coords);

    public abstract PositionOnLine MoveNext(LineCoords coords);

    public abstract bool IsTheEnd(LineCoords coords);
}


public class StringPart(int Offset = 0) : PositionOnLine
{
    public override char GetCurrChar(byte[] data, LineCoords coords)
    {
        return (char) data[coords.StringStart + Offset];
    }

    public override PositionOnLine MoveNext(LineCoords coords)
    {
        return Offset + 1 < coords.StringLength
            ? new StringPart(Offset + 1)
            : new NumberLength();
    }

    public override bool IsTheEnd(LineCoords coords) => false;
}


public class NumberLength : PositionOnLine
{
    public override char GetCurrChar(byte[] data, LineCoords coords)
    {
        return (char) coords.NumberLength;
    }

    public override PositionOnLine MoveNext(LineCoords coords)
    {
        return new NumberPart();
    }

    public override bool IsTheEnd(LineCoords coords) => false;
}


public class NumberPart(int Offset = 0) : PositionOnLine
{
    public override char GetCurrChar(byte[] data, LineCoords coords)
    {
        return (char) data[coords.NumberStart + Offset];
    }

    public override PositionOnLine MoveNext(LineCoords coords)
    {
        return new NumberPart(Offset + 1);
    }

    public override bool IsTheEnd(LineCoords coords) => Offset >= coords.NumberLength;
}
