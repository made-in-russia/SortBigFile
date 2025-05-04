using SortBigFile;

internal class Program
{
    /// <summary>
    /// Sorts a text file of any size with lines in
    ///     <Number>. <String>
    /// format
    /// </summary>
    /// <param name="args">
    ///     args[0] - Source file name
    ///     args[1] - Resulting file name
    /// </param>
    private static async Task Main(string[] args)
    {
        //string fileIn;
        //if (args.Length < 1 || string.IsNullOrEmpty(fileIn = args[0]))
        //{
        //    throw new ArgumentException("Source file name (first parameter) is missing");
        //}

        //string fileOut;
        //if (args.Length < 2 || string.IsNullOrEmpty(fileOut = args[1]))
        //{
        //    throw new ArgumentException("Resulting file name (second parameter) is missing");
        //}

        string fileIn = "D:\\My Projects\\SortBigFile\\test.txt";
        string fileOut = "D:\\My Projects\\SortBigFile\\result.txt";

        var timeStart = DateTime.Now;

        await BigFileSort.Sort(fileIn, fileOut);

        Console.WriteLine(DateTime.Now - timeStart);
    }
}