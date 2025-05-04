using SortBigFile.Constants;
using SortBigFile.LinesOrdering;
using SortBigFile.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortBigFile.Tests;

public class BigFileSortIntegrationTests
{
    private readonly List<string> _filesToDelete = [];

    private int _fileCount = 0;

    [Test]
    public async Task GivenTotalDifference_WhenCallFindLinesDiffStart_ThenReturnsInitialPosition()
    {
        var fileName = await CreateFile("111. AAA\n222. BBB\n333. CCC\n");
        var initialPosition = PositionOnLine.Initiate();
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = initialPosition
        };

        var result = await BigFileSort.FindLinesDiffStart(file);

        Assert.That(result?.Position, Is.EqualTo(initialPosition));
    }
    
    [Test]
    public async Task GivenStringDifference_WhenCallFindLinesDiffStart_ThenReturnsStringPart()
    {
        var fileName = await CreateFile("111. AAA\n222. ABB\n333. ACC\n");
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = PositionOnLine.Initiate()
        };

        var result = await BigFileSort.FindLinesDiffStart(file);

        Assert.That(result?.Position, Is.InstanceOf<StringPart>());
        Assert.That(result?.CommonPart, Is.EqualTo("A"));
    }
    
    [Test]
    public async Task GivenNumberLengthDifference_WhenCallFindLinesDiffStart_ThenReturnsNumberLength()
    {
        var fileName = await CreateFile("1. AAA\n22. AAA\n333. AAA\n");
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = PositionOnLine.Initiate()
        };

        var result = await BigFileSort.FindLinesDiffStart(file);

        Assert.That(result?.Position, Is.InstanceOf<NumberLength>());
        Assert.That(result?.CommonPart, Is.EqualTo("AAA"));
    }
    
    [Test]
    public async Task GivenNumberDifference_WhenCallFindLinesDiffStart_ThenReturnsNumberPart()
    {
        var fileName = await CreateFile("111. AAA\n122. AAA\n133. AAA\n");
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = PositionOnLine.Initiate()
        };
        var numberLengthChar = (char)3;

        var result = await BigFileSort.FindLinesDiffStart(file);

        Assert.That(result?.Position, Is.InstanceOf<NumberPart>());
        Assert.That(result?.CommonPart, Is.EqualTo($"AAA{numberLengthChar}1"));
    }
    
    [Test]
    public async Task GivenNoDifference_WhenCallFindLinesDiffStart_ThenReturnsTheEndPosition()
    {
        var content = string.Join(string.Empty, Enumerable.Repeat("1. A\r\n", 10));
        var fileName = await CreateFile(content);
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = PositionOnLine.Initiate()
        };
        var numberLengthChar = (char)1;

        var result = await BigFileSort.FindLinesDiffStart(file, 50);
        var differencesNotFound = result?.CouldNotFind();

        Assert.That(differencesNotFound, Is.True);
        Assert.That(result?.CommonPart, Is.EqualTo($"A{numberLengthChar}1"));
    }

    [Test]
    public async Task GivenNoDifference_WhenCallSort_ThenJustCopyTheFile()
    {
        var content = string.Join(string.Empty, Enumerable.Repeat("1. A\r\n", 10));
        var fileIn = await CreateFile(content);
        var fileOut = await CreateFile(string.Empty);

        await BigFileSort.Sort(fileIn, fileOut);
        var result = await ReadFile(fileOut);

        Assert.That(result, Is.EqualTo(content));
    }

    [Test]
    public async Task WhenCallSort_ThenReturnsSortedFile()
    {
        var fileIn = await CreateFile("222. AAA\r\n333. AAA\r\n111. AAA\r\n");
        var fileOut = await CreateFile(string.Empty);

        await BigFileSort.Sort(fileIn, fileOut);
        var result = await ReadFile(fileOut);

        Assert.That(result, Is.EqualTo("111. AAA\r\n222. AAA\r\n333. AAA\r\n"));
    }

    [Test]
    public async Task GivenTotalDifference_WhenCallSort_ThenSorts()
    {
        var fileIn = await CreateFile("111. AAA\r\n333. CCC\r\n222. BBB\r\n");
        var fileOut = await CreateFile(string.Empty);

        await BigFileSort.Sort(fileIn, fileOut, 10);
        var result = await ReadFile(fileOut);

        Assert.That(result, Is.EqualTo("111. AAA\r\n222. BBB\r\n333. CCC\r\n"));
    }

    [Test]
    public async Task GivenStringDifference_WhenCallSplitFile_ThenSplitFile()
    {
        var content = "111. AAA\r\n222. ABB\r\n333. ACC\r\n";
        var fileName = await CreateFile(content);

        var commonPartPos = new StringPart(1);
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = commonPartPos
        };

        var splitFiles = new Queue<IterationFile>();

        var data = Encoding.ASCII.GetBytes(content);
        var referenceCoords = new LineCoords(0, 3, 8);
        var linesDiffStart = new PositionSeeker(data, referenceCoords, string.Empty, commonPartPos);

        await BigFileSort.SplitFile(file, splitFiles, linesDiffStart);
        _filesToDelete.AddRange(splitFiles.Select(f => f.Name));
        var tasks = splitFiles.Select(f => ReadFile(f.Name));
        var filesContents = await Task.WhenAll(tasks);

        Assert.That(filesContents, Has.Length.EqualTo(3));
        Assert.That(filesContents, Contains.Item("111. AAA\r\n"));
        Assert.That(filesContents, Contains.Item("222. ABB\r\n"));
        Assert.That(filesContents, Contains.Item("333. ACC\r\n"));
    }
    
    [Test]
    public async Task GivenNumberLengthDifference_WhenCallSplitFile_ThenSplitFile()
    {
        var content = "111. AAA\r\n11. AAA\r\n1. AAA\r\n";
        var fileName = await CreateFile(content);

        var commonPartPos = new NumberLength();
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = commonPartPos
        };

        var splitFiles = new Queue<IterationFile>();

        var data = Encoding.ASCII.GetBytes(content);
        var referenceCoords = new LineCoords(0, 3, 8);
        var linesDiffStart = new PositionSeeker(data, referenceCoords, string.Empty, commonPartPos);

        await BigFileSort.SplitFile(file, splitFiles, linesDiffStart);
        _filesToDelete.AddRange(splitFiles.Select(f => f.Name));
        var tasks = splitFiles.Select(f => ReadFile(f.Name));
        var filesContents = await Task.WhenAll(tasks);

        Assert.That(filesContents, Has.Length.EqualTo(3));
        Assert.That(filesContents, Contains.Item("1. AAA\r\n"));
        Assert.That(filesContents, Contains.Item("11. AAA\r\n"));
        Assert.That(filesContents, Contains.Item("111. AAA\r\n"));
    }
    
    [Test]
    public async Task GivenNumberDifference_WhenCallSplitFile_ThenSplitFile()
    {
        var content = "111. AAA\r\n122. AAA\r\n133. AAA\r\n";
        var fileName = await CreateFile(content);

        var commonPartPos = new NumberPart(1);
        var file = new IterationFile
        {
            Name = fileName,
            CommonPart = string.Empty,
            CommonPartPos = commonPartPos
        };

        var splitFiles = new Queue<IterationFile>();

        var data = Encoding.ASCII.GetBytes(content);
        var referenceCoords = new LineCoords(0, 3, 8);
        var linesDiffStart = new PositionSeeker(data, referenceCoords, string.Empty, commonPartPos);

        await BigFileSort.SplitFile(file, splitFiles, linesDiffStart);
        _filesToDelete.AddRange(splitFiles.Select(f => f.Name));
        var tasks = splitFiles.Select(f => ReadFile(f.Name));
        var filesContents = await Task.WhenAll(tasks);

        Assert.That(filesContents, Has.Length.EqualTo(3));
        Assert.That(filesContents, Contains.Item("111. AAA\r\n"));
        Assert.That(filesContents, Contains.Item("122. AAA\r\n"));
        Assert.That(filesContents, Contains.Item("133. AAA\r\n"));
    }
    
    [Test]
    public async Task GivenTwoFiles_WhenCallAppendFileTo_ThenMergeTwoFiles()
    {
        var fileName1 = await CreateFile("AAA");
        var fileName2 = await CreateFile("BBB");

        await BigFileSort.AppendFileTo(fileName1, fileName2);
        var result = await ReadFile(fileName2);

        Assert.That(result, Is.EqualTo("BBBAAA"));
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var file in _filesToDelete)
        {
            File.Delete(file);
        }
    }


    private async Task<string> CreateFile(string content)
    {
        var name = $"test{_fileCount++}";

        using var writer = new StreamWriter(name, false);
        await writer.WriteAsync(content);

        _filesToDelete.Add(name);
        return name;
    }
    private static async Task<string> ReadFile(string name)
    {
        using var reader = new StreamReader(name, false);

        return await reader.ReadToEndAsync();
    }
}
