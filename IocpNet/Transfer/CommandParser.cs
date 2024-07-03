using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.IocpNet.Transfer;

public class CommandParser
{
    Dictionary<ProtocolKey, string> Commands { get; } = [];

    private CommandParser()
    {

    }

    public static CommandParser Parse(string command, int bytesTransferred)
    {
        var result = new CommandParser();
        var lines = command.Split(SignTable.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var pair = line.Split(SignTable.Equal, StringSplitOptions.None);
            if (pair.Length < 2)
                continue;
            var key = pair[0].ToEnum<ProtocolKey>();
            result.Commands[key] = pair[1];
        }
        return result;
    }

    public bool GetValueAsCommandKey(out ProtocolKey key)
    {
        key = ProtocolKey.None;
        if (!Commands.TryGetValue(ProtocolKey.Command, out var value))
            return false;
        key = value.ToEnum<ProtocolKey>();
        return true;
    }

    public bool GetValueAsString(ProtocolKey commandKey, [NotNullWhen(true)] out string? value)
    {
        return Commands.TryGetValue(commandKey, out value);
    }

    public bool GetValueAsShort(ProtocolKey commandKey, out short value)
    {
        value = 0;
        return Commands.TryGetValue(commandKey, out var str) && short.TryParse(str, out value);
    }

    public bool GetValueAsInt(ProtocolKey commandKey, out int value)
    {
        value = 0;
        return Commands.TryGetValue(commandKey, out var str) && int.TryParse(str, out value);
    }

    public bool GetValueAsLong(ProtocolKey commandKey, out long value)
    {
        value = 0;
        return Commands.TryGetValue(commandKey, out var str) && long.TryParse(str, out value);
    }

    public bool GetValueAsFloat(ProtocolKey commandKey, out float value)
    {
        value = 0f;
        return Commands.TryGetValue(commandKey, out var str) && float.TryParse(str, out value);
    }

    public bool GetValueAsDouble(ProtocolKey commandKey, out double value)
    {
        value = 0d;
        return Commands.TryGetValue(commandKey, out var str) && double.TryParse(str, out value);
    }

    public bool GetValueAsBool(ProtocolKey commandKey, out bool value)
    {
        value = false;
        return Commands.TryGetValue(commandKey, out var str) && bool.TryParse(str, out value);
    }
}
