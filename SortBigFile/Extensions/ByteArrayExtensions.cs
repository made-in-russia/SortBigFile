using SortBigFile.Models;

namespace SortBigFile.Extensions;

public static class ByteArrayExtensions
{
    public static void ExecInParallel(this byte[] data, Action<int, int> action)
    {
        var dataSplit = data.SplitByProcessorCount(data.Length);

        Parallel.ForEach(
            Enumerable.Range(0, dataSplit.Count - 1),
            (i) => action(dataSplit[i], dataSplit[i + 1]));
    }

    public static LineCoords GetNextLineCoords(this byte[] data, int pos)
    {
        while (pos < data.Length && data[pos].IsNewLine()) pos++;
        var startIndex = pos;

        while (pos < data.Length && data[pos].IsNotPoint()) pos++;
        var pointIndex = pos;

        while (pos < data.Length && data[pos].IsNotNewLine()) pos++;
        var endIndex = pos;

        return new LineCoords(startIndex, pointIndex, endIndex);
    }


    private static IList<int> SplitByProcessorCount(this byte[] data, int length)
    {
        var processorCount = Environment.ProcessorCount;
        var splitCoords = new List<int>(processorCount + 1);

        var baseSplitLength = length / processorCount;
        var predCutPoint = 0;

        splitCoords.Add(0);
        while (true)
        {
            var cutPoint = predCutPoint + baseSplitLength;

            while (cutPoint < length && data[cutPoint].IsNotNewLine()) cutPoint++;
            while (cutPoint < length && data[cutPoint].IsNewLine()) cutPoint++;

            if (cutPoint < length)
            {
                splitCoords.Add(cutPoint);
                predCutPoint = cutPoint;
            }
            else
            {
                splitCoords.Add(length);
                return splitCoords;
            }
        }
    }
}
