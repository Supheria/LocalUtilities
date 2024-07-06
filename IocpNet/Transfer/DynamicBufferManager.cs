namespace LocalUtilities.IocpNet.Transfer;

public class DynamicBufferManager(int bufferSize)
{
    byte[] Buffer { get; set; } = new byte[bufferSize];

    public int TotolCount
    {
        get => Buffer.Length;
        set
        {
            try
            {
                var buffer = new byte[value];
                Array.Copy(Buffer, 0, buffer, 0, DataCount);
                Buffer = buffer;
            }
            catch { }
        }
    }

    public int DataCount { get; private set; } = 0;

    public byte[] GetData()
    {
        var data = new byte[DataCount];
        Array.Copy(Buffer, 0, data, 0, data.Length);
        return data;
    }

    public void Clear()
    {
        lock (Buffer)
            DataCount = 0;
    }

    public void RemoveData(int dataCount)
    {
        lock (Buffer)
        {
            if (dataCount >= TotolCount || dataCount >= DataCount)
            {
                DataCount = 0;
                return;
            }
            DataCount -= dataCount;
            for (var i = 0; i < DataCount; i++)
            {
                Buffer[i] = Buffer[dataCount + i];
            }
        }
    }

    public void WriteData(byte[] data, int offset, int count)
    {
        lock (Buffer)
        {
            if (data.Length is 0)
                return;
            if (TotolCount - DataCount < count)
            {
                var totalCount = count + DataCount;
                var buffer = new byte[totalCount];
                Array.Copy(Buffer, 0, buffer, 0, DataCount);
                Buffer = buffer;
            }
            Array.Copy(data, offset, Buffer, DataCount, count);
            DataCount += count;
        }
    }

    //public void WriteValue(byte value)
    //{
    //    WriteData([value]);
    //}

    //public void WriteValue(byte[] value, int offset, int count)
    //{
    //    WriteData(value, offset, count);
    //}

    //public void WriteValue(short value)
    //{
    //    var data = BitConverter.GetBytes(value);
    //    WriteData(data);
    //}

    //public void WriteValue(int value)
    //{
    //    var data = BitConverter.GetBytes(value);
    //    WriteData(data);
    //}

    //public void WriteValue(long value)
    //{
    //    var data = BitConverter.GetBytes(value);
    //    WriteData(data);
    //}

    //public void WriteValue(string value)
    //{
    //    var data = Encoding.UTF8.GetBytes(value);
    //    WriteData(data);
    //}
}
