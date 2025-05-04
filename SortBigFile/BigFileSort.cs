using SortBigFile.Constants;
using SortBigFile.Extensions;
using SortBigFile.LinesOrdering;
using SortBigFile.Models;

namespace SortBigFile;

static public class BigFileSort
{
    public const int BufferSize = 1024 * 1024 * 100;

    private static readonly byte[] _inputBuffer = new byte[BufferSize];
    private static readonly byte[] _outputBuffer = new byte[BufferSize];
    private static readonly object _splitLock = new();

    private static int _filesCounter = 0;

    public static async Task Sort(string fileIn, string fileOut, int maxRead = BufferSize)
    {
        Queue<IterationFile> filesToSort = new();
        filesToSort.Enqueue(new IterationFile()
        {
            Name = fileIn,
            CommonPart = string.Empty,
            CommonPartPos = PositionOnLine.Initiate(),
            Size = new FileInfo(fileIn).Length,
            DeleteAfter = false
        });
        List<IterationFile> sortedFiles = [];

        while (filesToSort.TryDequeue(out var fileToSort))
        {
            var linesDiffStart = await FindLinesDiffStart(fileToSort);

            if (linesDiffStart == null || linesDiffStart.CouldNotFind())
            {
                sortedFiles.Add(fileToSort);
            }
            else if (fileToSort.Size <= maxRead)
            {
                await SortFile(fileToSort);

                sortedFiles.Add(fileToSort);
            }
            else
            {
                await SplitFile(fileToSort, filesToSort, linesDiffStart);

                if (fileToSort.DeleteAfter) File.Delete(fileToSort.Name);
            }
        }

        sortedFiles.Sort((a, b) => a.CommonPart.CompareTo(b.CommonPart));

        foreach (var sortedFile in sortedFiles)
        {
            await AppendFileTo(sortedFile.Name, fileOut);

            if (sortedFile.DeleteAfter) File.Delete(sortedFile.Name);
        }
    }

    public static async Task<PositionSeeker?> FindLinesDiffStart(IterationFile iterationFile, int maxRead = BufferSize)
    {
        int actualRead;

        using FileStream readStream = new(iterationFile.Name, FileMode.Open);

        PositionSeeker? positionSeeker = null;
        while (true)
        {
            readStream.Position = 0;
            while ((actualRead = await readStream.ReadAsync(_inputBuffer.AsMemory(0, maxRead))) > 1)
            {
                var dataLength = Array.LastIndexOf(_inputBuffer[..actualRead], AppConstants.LineSeparators.Last());
                readStream.Position -= actualRead - dataLength - 1;

                if (positionSeeker == null)
                {
                    var referenceCoords = _inputBuffer.GetNextLineCoords(0);
                    positionSeeker = new PositionSeeker(_inputBuffer, referenceCoords, iterationFile.CommonPart, iterationFile.CommonPartPos);
                }

                _inputBuffer[..dataLength].ExecInParallel(CompareLinesAtPos);

                if (positionSeeker.IsFound)
                {
                    return positionSeeker;
                }
            }

            positionSeeker?.MoveNext();

            if (positionSeeker == null || positionSeeker.CouldNotFind())
            {
                return positionSeeker;
            }
        }


        void CompareLinesAtPos(int dataStart, int dataEnd)
        {
            LineCoords lineCoords = _inputBuffer.GetNextLineCoords(dataStart);

            while (lineCoords.LineEnd < dataEnd)
            {
                if (positionSeeker.NonEqual(lineCoords))
                {
                    positionSeeker.IsFound = true;
                }

                if (positionSeeker.IsFound)
                {
                    return;
                }

                lineCoords = _inputBuffer.GetNextLineCoords(lineCoords.LineEnd + 1);
            }
        }
    }

    public static async Task SortFile(IterationFile iterationFile, int maxRead = BufferSize)
    {
        int actualRead;
        using (FileStream stream = new(iterationFile.Name, FileMode.Open))
        {
            actualRead = await stream.ReadAsync(_inputBuffer.AsMemory(0, maxRead));
        }

        var data = _inputBuffer[..actualRead];

        var linesCount = data.Count(x => x == AppConstants.LineSeparators.Last()) + 1;
        var coordsToSort = new List<LineCoords>(linesCount);

        var lineCoords = data.GetNextLineCoords(0);
        while (lineCoords.LineEnd < actualRead)
        {
            coordsToSort.Add(lineCoords);
            lineCoords = data.GetNextLineCoords(lineCoords.LineEnd + 1);
        }

        var sortedCoords = coordsToSort
            .AsParallel()
            .OrderBy(x => x, new LineCoordsComparer(_inputBuffer));

        var outputPos = 0;
        foreach (var sourceCoords in sortedCoords)
        {
            Buffer.BlockCopy(_inputBuffer, sourceCoords.LineStart, _outputBuffer, outputPos, sourceCoords.Length);
            outputPos += sourceCoords.Length;

            Buffer.BlockCopy(AppConstants.LineSeparators, 0, _outputBuffer, outputPos, AppConstants.LineSeparators.Length);
            outputPos += AppConstants.LineSeparators.Length;
        }

        using var destFile = new FileStream(iterationFile.Name, FileMode.Open);
        await destFile.WriteAsync(_outputBuffer.AsMemory(0, outputPos));
    }

    public static async Task SplitFile(IterationFile sourceFile, Queue<IterationFile> filesToSort, PositionSeeker linesDiffStart, int maxRead = BufferSize)
    {
        var destFiles = new IterationFile[128];
        var writers = new FileStream[128];

        using (FileStream stream = new(sourceFile.Name, FileMode.Open))
        {
            int actualRead;
            while ((actualRead = await stream.ReadAsync(_inputBuffer.AsMemory(0, maxRead))) > 1)
            {
                var dataLength = Array.LastIndexOf(_inputBuffer[..actualRead], AppConstants.LineSeparators.Last());
                stream.Position -= actualRead - dataLength - 1;

                _inputBuffer[..dataLength].ExecInParallel(SplitBuffer);

                for (var i = 0; i < 128; i++)
                {
                    var (destFile, writer) = (destFiles[i], writers[i]);
                    if (destFile == null || writer == null) continue;

                    var outputPos = 0;
                    foreach (var sourceCoords in destFile.LinesCoords)
                    {
                        Buffer.BlockCopy(_inputBuffer, sourceCoords.LineStart, _outputBuffer, outputPos, sourceCoords.Length);
                        outputPos += sourceCoords.Length;

                        Buffer.BlockCopy(AppConstants.LineSeparators, 0, _outputBuffer, outputPos, AppConstants.LineSeparators.Length);
                        outputPos += AppConstants.LineSeparators.Length;
                    }

                    await writer.WriteAsync(_outputBuffer.AsMemory(0, outputPos));

                    destFile.Size += outputPos;
                    destFile.LinesCoords.Clear();
                }
            }
        }

        foreach (var destFile in destFiles)
        {
            if (destFile == null) continue;

            filesToSort.Enqueue(destFile);
        }

        foreach (var writer in writers)
        {
            writer?.Dispose();
        }


        void SplitBuffer(int dataStart, int dataEnd)
        {
            var lineCoords = _inputBuffer.GetNextLineCoords(dataStart);

            while (lineCoords.LineEnd < dataEnd)
            {
                var splitChar = linesDiffStart.Position.GetCurrChar(_inputBuffer, lineCoords);

                if (destFiles[splitChar] == null)
                {
                    lock (_splitLock)
                    {
                        if (destFiles[splitChar] == null)
                        {
                            var fileName = GetNextFileName();
                            destFiles[splitChar] = new()
                            {
                                Name = fileName,
                                CommonPart = $"{linesDiffStart.CommonPart}{splitChar}",
                                CommonPartPos = linesDiffStart.Position,
                                DeleteAfter = true
                            };
                            writers[splitChar] = new(fileName, FileMode.Create);
                        }
                    }
                }

                destFiles[splitChar].LinesCoords.Add(lineCoords);

                lineCoords = _inputBuffer.GetNextLineCoords(lineCoords.LineEnd + 1);
            }
        }
    }

    public static async Task AppendFileTo(string fileSource, string fileDest, int maxRead = BufferSize)
    {
        using FileStream streamDource = new(fileSource, FileMode.Open);
        using FileStream streamDest = new(fileDest, FileMode.OpenOrCreate);

        int actualRead;
        streamDest.Position = streamDest.Length;
        while ((actualRead = await streamDource.ReadAsync(_inputBuffer.AsMemory(0, maxRead))) > 0)
        {
            await streamDest.WriteAsync(_inputBuffer.AsMemory(0, actualRead));
        }
    }

    private static string GetNextFileName() => $"{_filesCounter++}";
}
