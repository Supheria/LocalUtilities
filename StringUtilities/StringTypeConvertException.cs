using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.StringUtilities;

internal class StringTypeConvertException(Type type) : Exception($"cannot convert string to {type}")
{
}
