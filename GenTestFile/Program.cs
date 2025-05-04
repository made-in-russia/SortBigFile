
internal class Program
{
    const string AvailableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstwxyz ";

    private static readonly Random _random = new();

    /// <summary>
    /// Generates a text file with random lines in
    ///     <Number>. <String>
    /// format of a target size
    /// </summary>
    /// <param name="args">
    ///     args[0] - Target file name
    ///     args[1] - Target file size
    /// </param>
    private static void Main(string[] args)
    {
        string fileName;
        if (args.Length < 1 || string.IsNullOrEmpty(fileName = args[0]))
        {
            throw new ArgumentException("File name (first parameter) is missing");
        }

        if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
        {
            throw new ArgumentException("Target file size (second parameter) is missing");
        }

        if (!int.TryParse(args[1], out var targetSize))
        {
            throw new ArgumentException("Target file size (second parameter) must be of integer type");
        }

        using var writer = File.CreateText(fileName);

        var repeatingString = GetStringPart();
        var repetitionRatio = 0.3;

        var newLine = string.Empty;
        for (var totalSize = 0; totalSize < targetSize; totalSize += newLine.Length + 2)
        {
            newLine = _random.NextDouble() < repetitionRatio ? GetNewLine(repeatingString) : GetNewLine();
            writer.WriteLine(newLine);
        }
    }

    private static string GetNewLine(string? stringPart = null)
    {
        var numPart = _random.Next(int.MaxValue);
        stringPart ??= GetStringPart();

        return $"{numPart}. {stringPart}";
    }

    private static string GetStringPart()
    {
        var firstChar = AvailableChars[_random.Next(AvailableChars.Length - 1)];

        var allowSpace = true;
        var middleStr = string.Join(string.Empty, Enumerable
            .Repeat(AvailableChars, 18)
            .Select(s =>
            {
                if (allowSpace)
                {
                    var result = s[_random.Next(s.Length)];
                    allowSpace = result != ' ';
                    return result;
                }
                return s[_random.Next(s.Length - 1)];
            }));

        var lastChar = AvailableChars[_random.Next(AvailableChars.Length - 1)];
        return $"{firstChar}{middleStr}{lastChar}";
    }
}