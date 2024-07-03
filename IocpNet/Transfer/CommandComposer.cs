using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

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
        AppendLine(ProtocolKey.CallbackCode, ProtocolCode.Success.ToString());
        return this;
    }

    public CommandComposer AppendFailure(ProtocolCode errorCode, string errorMessage)
    {
        AppendLine(ProtocolKey.CallbackCode, errorCode.ToString());
        AppendLine(ProtocolKey.ErrorMessage, errorMessage);
        return this;
    }

    public CommandComposer AppendValue(ProtocolKey commandKey, object? value)
    {
        AppendLine(commandKey, value?.ToString());
        return this;
    }
}
