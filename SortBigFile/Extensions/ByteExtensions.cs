using SortBigFile.Constants;

namespace SortBigFile.Extensions;

internal static class ByteExtensions
{
    public static bool IsNewLine(this byte value)
    {
        return AppConstants.LineSeparators.Contains(value);
    }

    public static bool IsNotNewLine(this byte value)
    {
        return !AppConstants.LineSeparators.Contains(value);
    }

    public static bool IsNotPoint(this byte value)
    {
        return value != (byte)'.';
    }
}
