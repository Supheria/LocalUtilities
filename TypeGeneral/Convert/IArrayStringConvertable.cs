using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeGeneral.Convert;

public interface IArrayStringConvertable
{
    public string ToArrayString();

    public void ParseArrayString(string str);
}
