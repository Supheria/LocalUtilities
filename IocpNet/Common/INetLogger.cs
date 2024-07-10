using LocalUtilities.IocpNet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public interface INetLogger
{
    public NetEventHandler<string>? OnLog { get; set; }

    public string GetLog(string message);
}
