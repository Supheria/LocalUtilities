using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Parser;

public class Word(string text, int line, int column)
{
    public string Text { get; } = text;

    public int Line { get; } = line;

    public int Column { get; } = column;

    public Word() : this("", 0, 0)
    {
    }
}
