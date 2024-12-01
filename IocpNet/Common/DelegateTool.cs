namespace LocalUtilities.IocpNet;

public delegate void NetEventHandler();

public delegate void NetEventHandler<TArgs>(TArgs args);
