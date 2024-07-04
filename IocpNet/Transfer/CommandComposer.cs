using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class CommandComposer
{
    StringBuilder Commands { get; } = new();

    public void Clear()
    {
        Commands.Clear();
    }

    public int GetCommandBuffer(out byte[] buffer)
    {
        buffer = [];
        try
        {
            buffer = Encoding.UTF8.GetBytes(Commands.ToString());
            return buffer.Length;
        }
        catch
        {
            return 0;
        }
    }

    private void AppendLine(ProtocolKey commandKey, string? value)
    {
        var str = new StringBuilder()
            .Append(commandKey.ToString())
            .Append(SignTable.Colon)
            .Append(value)
            .ToString();
        Commands.Append(str)
            .Append(SignTable.NewLine);
    }

    public CommandComposer AppendCommand(CommandTypes type)
    {
        AppendLine(ProtocolKey.Command, type.ToString());
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
