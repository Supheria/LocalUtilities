﻿using LocalUtilities.SimpleScript.Common;

namespace LocalUtilities.IocpNet.Common;

public abstract class Command : INetLogger
{
    protected const int HeadLength
        = sizeof(int) // total length
        + sizeof(int) // args length
        + sizeof(byte) // command code
        + sizeof(byte) // operate code
        + sizeof(long) // time stamp
        ;

    public NetEventHandler<string>? OnLog { get; set; }

    public DateTime TimeStamp { get; protected init; } = new();

    public byte CommandCode { get; protected init; } = byte.MinValue;

    public byte OperateCode { get; protected init; } = byte.MinValue;

    protected Dictionary<string, string> Args { get; init; } = [];

    public byte[] Data { get; protected init; } = [];

    protected static SsSignTable SignTable { get; } = new();

    public string GetLog(string message)
    {
        return message;
    }
}
