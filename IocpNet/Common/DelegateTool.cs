namespace LocalUtilities.IocpNet.Protocol;

public delegate void NetEventHandler();

public delegate void NetEventHandler<TArgs>(TArgs args);
