using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class CommandCallback : Command
{
    public CommandCallback(DateTime timeStamp, CommandTypes commandType, OperateTypes operateType, byte[] data, int dataOffset, int dataCount) : base(Types.Callback, timeStamp, commandType, operateType, data, dataOffset, dataCount)
    {
        AppendArgs(ProtocolKey.CallbackCode, ProtocolCode.None.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, "");
    }

    public CommandCallback(DateTime timeStamp, CommandTypes commandType, OperateTypes operateType) : base(Types.Callback, timeStamp, commandType, operateType)
    {
        AppendArgs(ProtocolKey.CallbackCode, ProtocolCode.None.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, "");
    }

    public override CommandCallback AppendArgs(ProtocolKey key, string args)
    {
        base.AppendArgs(key, args);
        return this;
    }

    public CommandCallback AppendSuccess()
    {
        AppendArgs(ProtocolKey.CallbackCode, ProtocolCode.Success.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, "");
        return this;
    }

    public CommandCallback AppendFailure(Exception ex)
    {
        var errorCode = ex switch
        {
            IocpException iocp => iocp.ErrorCode,
            _ => ProtocolCode.UnknowError,
        };
        AppendArgs(ProtocolKey.CallbackCode, errorCode.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, ex.Message);
        return this;
    }

    public ProtocolCode GetCallbackCode()
    {
        return GetArgs(ProtocolKey.CallbackCode).ToEnum<ProtocolCode>();
    }

    public string GetErrorMessage()
    {
        return new StringBuilder()
            .Append(SignTable.OpenParenthesis)
            .Append(CommandType)
            .Append(SignTable.Comma)
            .Append(SignTable.Space)
            .Append(OperateType)
            .Append(SignTable.CloseParenthesis)
            .Append(GetArgs(ProtocolKey.ErrorMessage))
            .ToString();
    }
}
