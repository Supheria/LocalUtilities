namespace LocalUtilities.IocpNet;

public class DynamicBuffer()
{
    byte[] Buffer { get; set; } = [];

    public int DataCount { get; private set; } = 0;

    public byte[] GetData()
    {
        lock (Buffer)
        {
            var data = new byte[DataCount];
            Array.Copy(Buffer, 0, data, 0, data.Length);
            return data;
        }
    }

    public void Clear()
    {
        lock (Buffer)
        {
            DataCount = 0;
            Buffer = [];
        }
    }

    public void RemoveData(int dataCount)
    {
        lock (Buffer)
        {
            if (dataCount >= DataCount)
            {
                DataCount = 0;
                Buffer = [];
                return;
            }
            DataCount -= dataCount;
            var buffer = new byte[DataCount];
            for (var i = 0; i < DataCount; i++)
            {
                buffer[i] = Buffer[dataCount + i];
            }
            Buffer = buffer;
        }
    }

    public void WriteData(byte[] data, int offset, int count)
    {
        lock (Buffer)
        {
            if (data.Length is 0 || count is 0)
                return;
            var dataCount = count + DataCount;
            var buffer = new byte[dataCount];
            Array.Copy(Buffer, 0, buffer, 0, DataCount);
            Array.Copy(data, offset, buffer, DataCount, count);
            Buffer = buffer;
            DataCount += count;
        }
    }
}
