using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.DelegateUtilities;

public static class DelegateTool
{
    public static T? RemoveAllInvocations<T>(this T? source) where T : Delegate
    {
        return Delegate.RemoveAll(source, source) as T;
    }
}
