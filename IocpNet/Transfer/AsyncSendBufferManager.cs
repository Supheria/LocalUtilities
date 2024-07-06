namespace LocalUtilities.IocpNet.Transfer;

/// <summary>
/// 由于是异步发送，有可能接收到两个命令，写入了两次返回，发送需要等待上一次回调才发下一次的响应
/// </summary>
/// <param name="bufferSize"></param>
public class AsyncSendBufferManager(int bufferSize)
{
    public DynamicBufferManager DynamicBufferManager { get; } = new(bufferSize);

    record SendBufferPacket()
    {
        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 0;
    }

    List<SendBufferPacket> SendPacketList { get; } = [];

    public void AppendPacket(byte[] packet)
    {
        var sendPacket = new SendBufferPacket()
        {
            Offset = DynamicBufferManager.DataCount,
        };
        DynamicBufferManager.WriteData(packet, 0, packet.Length);
        sendPacket.Count = DynamicBufferManager.DataCount - sendPacket.Offset;
        SendPacketList.Add(sendPacket);
    }

    public bool GetFirstPacket(out int offset, out int count)
    {
        // SendPacketList[0].Offset;清除了第一个包后，后续的包往前移，因此Offset都为0
        lock (SendPacketList)
        {
            offset = 0;
            count = 0;
            if (SendPacketList.Count <= 0)
                return false;
            count = SendPacketList[0].Count;
            return true;
        }
    }

    public void ClearFirstPacket()
    {
        lock (SendPacketList)
        {
            if (SendPacketList.Count <= 0)
                return;
            DynamicBufferManager.RemoveData(SendPacketList[0].Count);
            SendPacketList.RemoveAt(0);
        }
    }

    public void ClearAllPacket()
    {
        lock (SendPacketList)
        {
            SendPacketList.Clear();
            DynamicBufferManager.RemoveData(DynamicBufferManager.DataCount);
        }
    }
}
