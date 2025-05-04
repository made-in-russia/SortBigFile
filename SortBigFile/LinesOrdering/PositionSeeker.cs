using SortBigFile.Models;

namespace SortBigFile.LinesOrdering;

public class PositionSeeker
{
    private readonly byte[] _data;
    private readonly byte[] _referenceData;
    private readonly LineCoords _referenceCoords;

    private PositionOnLine _position;
    private string _commonPart;

    public PositionSeeker(byte[] data, LineCoords referenceCoords, string commonPart, PositionOnLine positionStart)
    {
        _data = data;
        _referenceCoords = referenceCoords;
        _commonPart = commonPart;
        _position = positionStart;

        _referenceData = new byte[referenceCoords.Length];
        Array.Copy(data, _referenceData, _referenceData.Length);
    }

    public bool IsFound { get; set; } = false;

    public PositionOnLine Position => _position;

    public string CommonPart => _commonPart;

    public bool NonEqual(LineCoords compareWith)
    {
        return _position.GetCurrChar(_referenceData, _referenceCoords) != _position.GetCurrChar(_data, compareWith);
    }

    public void MoveNext()
    {
        _commonPart += _position.GetCurrChar(_referenceData, _referenceCoords);
        _position = _position.MoveNext(_referenceCoords);
    }

    public bool CouldNotFind()
    {
        return _position.IsTheEnd(_referenceCoords);
    }
}
