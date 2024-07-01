namespace LocalUtilities.IocpNet.Protocol;

public class AutoDisposeFileStream
{
    FileStream? FileStream { get; set; } = null;

    System.Timers.Timer Timer { get; } = new();

    public bool IsExpired => FileStream is null;

    public long Length => FileStream?.Length ?? 0;

    public long Position
    {
        get => FileStream?.Position ?? 0;
        set
        {
            if (FileStream is not null)
                FileStream.Position = value;
        }
    }

    public bool Relocate(FileStream fileStream, int expireMilliseconds)
    {
        if (!IsExpired)
            return false;
        FileStream = fileStream;
        Timer.Interval = expireMilliseconds;
        Timer.Elapsed += (_, _) => Close();
        Timer.Start();
        return true;
    }

    public void Close()
    {
        FileStream?.Dispose();
        FileStream = null;
        Timer.Stop();
    }

    public bool Read(byte[] buffer, int offset, int count, out int readCount)
    {
        readCount = 0;
        if (FileStream is null)
            return false;
        Timer.Stop();
        try
        {
            readCount = FileStream.Read(buffer, offset, count);
        }
        finally
        {
            Timer.Start();
        }
        return true;
    }

    public bool Write(byte[] buffer, int offset, int count)
    {
        if (FileStream is null)
            return false;
        Timer.Stop();
        try
        {
            FileStream.Write(buffer, offset, count);
        }
        finally
        {
            Timer.Start();
        }
        Timer.Start();
        return true;
    }
}
