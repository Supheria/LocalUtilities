using LocalUtilities.SimpleScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsDeserializer
{
    Tokenizer Tokenizer { get; }

    ParseTree Tree { get; set; } = new();

    string Token { get; set; } = "";

    public SsDeserializer(string filePath)
    {
        Tokenizer = new Tokenizer(filePath);
    }

    private string GetToken()
    {
        var token = Tokenizer.ParseNextToken()
    }

    public string ReadTag(string name)
    {
        Tree.Parse()
    }
}
