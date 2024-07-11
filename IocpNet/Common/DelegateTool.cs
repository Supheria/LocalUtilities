namespace LocalUtilities.IocpNet.Common;

public delegate void NetEventHandler();

public delegate void NetEventHandler<TArgs>(TArgs args);
