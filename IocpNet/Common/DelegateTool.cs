namespace LocalUtilities.IocpNet.Protocol;

public delegate void LogHandler(string log);

public delegate void IocpEventHandler();

public delegate void IocpEventHandler<TArgs>(TArgs args);

public delegate TValue GetValue<TValue>();