using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public class CommandComposer
{
    StringBuilder Commands { get; } = new();

    public void Clear()
    {
        Commands.Clear();
    }

    public string GetCommand()
    {
        return Commands.ToString();
    }

    private void AppendLine(ProtocolKey commandKey, string? value)
    {
        var str = new StringBuilder()
            .Append(commandKey.ToString())
            .Append(SignTable.Equal)
            .Append(value)
            .ToString();
        Commands.Append(str)
            .Append(SignTable.NewLine);
    }

    public CommandComposer AppendCommand(ProtocolKey commandKey)
    {
        AppendLine(ProtocolKey.Command, commandKey.ToString());
        return this;
    }

    public CommandComposer AppendSuccess()
    {
        AppendLine(ProtocolKey.Code, ProtocolCode.Success.ToString());
        return this;
    }

    public CommandComposer AppendFailure(ProtocolCode errorCode, string message)
    {
        AppendLine(ProtocolKey.Code, errorCode.ToString());
        AppendLine(ProtocolKey.Message, message);
        return this;
    }

    public CommandComposer AppendValue(ProtocolKey commandKey, object value)
    {
        AppendLine(commandKey, value.ToString());
        return this;
    }
}
