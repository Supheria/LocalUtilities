using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public enum CommandTypes : byte
{
    None,
    Login,
    Operate,
    OperateCallback,
    Upload,
    Download,
    HeartBeats,
    WriteFile,
    SendFile,
    TransferFile
}
