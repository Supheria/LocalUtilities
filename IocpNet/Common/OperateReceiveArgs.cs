using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public sealed class OperateReceiveArgs(OperateTypes type, string args)
{
    public OperateTypes Type { get; } = type;

    public string Arg { get; } = args;
}
