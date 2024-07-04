using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public interface ICommandData : ISsSerializable
{
    /// <summary>
    /// the short message to descript this object in log
    /// </summary>
    public string ShortLog { get; }
}
