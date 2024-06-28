﻿namespace LocalUtilities.IocpNet.Protocol;

public delegate void IocpEventHandler(IocpProtocol protocol);

public delegate void IocpEventHandler<TArgs>(IocpProtocol iocpProtocol, TArgs args);
