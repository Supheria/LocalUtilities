using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public sealed class OperateArgs(OperateTypes type, string args)
{
    public OperateTypes Type { get; } = type;

    public string Args { get; } = args;
}
