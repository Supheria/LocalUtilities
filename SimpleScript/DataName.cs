using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript;

public class DataName(string name)
{
    public string Name { get; } = name;

    public DataName() : this("Data")
    {

    }
}
