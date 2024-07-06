using LocalUtilities.IocpNet.Common;

namespace LocalUtilities.IocpNet.Transfer;

public class AutoDisposeFileStream
{
    FileStream? FileStream { get; set; } = null;

    DaemonThread DaemonThread { get; }

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

    public AutoDisposeFileStream()
    {
        DaemonThread = new(ConstTabel.FileStreamExpireMilliseconds, Dispose);
    }

    public bool Relocate(FileStream fileStream)
    {
        if (!IsExpired)
        {
            fileStream.Dispose();
            return false;
        }
        FileStream = fileStream;
        DaemonThread.Start();
        return true;
    }

    public void Dispose()
    {
        FileStream?.Dispose();
        FileStream = null;
        DaemonThread.Stop();
    }

    public bool Read(byte[] buffer, out int readCount)
    {
        readCount = 0;
        if (FileStream is null)
            return false;
        DaemonThread.Stop();
        try
        {
            readCount = FileStream.Read(buffer);
        }
        finally
        {
            DaemonThread.Start();
        }
        return true;
    }

    public bool Write(byte[] buffer)
    {
        if (FileStream is null)
            return false;
        DaemonThread.Stop();
        try
        {
            FileStream.Write(buffer);
        }
        finally
        {
            DaemonThread.Start();
        }
        DaemonThread.Start();
        return true;
    }
}
