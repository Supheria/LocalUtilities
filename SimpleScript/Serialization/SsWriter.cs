﻿using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;
using System.Xml.Linq;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsWriter(bool writeIntoMultiLines)
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    internal bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    internal void AppendComment(string comment)
    {
        _ = StringBuilder.AppendComment(Level, comment, WriteIntoMultiLines);
    }

    internal void AppendToken(string name)
    {
        _ = StringBuilder.AppendToken(Level, name, WriteIntoMultiLines);
    }

    internal void AppendNameStart(string name)
    {
        _ = StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    internal void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    internal void AppendValue(string name, IList<string> value)
    {
        _ = StringBuilder.AppendTagValue(Level, name, "_", value, WriteIntoMultiLines);
    }

    internal void AppendTagValue(string name, string tag, IList<string> values)
    {
        _ = StringBuilder.AppendTagValue(Level, name, tag, values, WriteIntoMultiLines);
    }

    internal void AppendValueArrays(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValueArrays(Level, name, valuesArray, WriteIntoMultiLines);
    }

    internal void AppendTagValueArrays(string name, List<List<KeyValuePair<Word, List<Word>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValueArrays(Level, name, pairsArray, WriteIntoMultiLines);
    }
}