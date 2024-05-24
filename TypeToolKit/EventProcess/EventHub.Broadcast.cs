using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void Broadcast(string eventName)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))();
    }
    public void Broadcast<T>(string eventName, T arg)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback<T> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))(arg);
    }

    public void Broadcast<T1, T2>(string eventName, T1 arg1, T2 arg2)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback<T1, T2> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))(arg1, arg2);
    }

    public void Broadcast<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback<T1, T2, T3> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))(arg1, arg2, arg3);
    }

    public void Broadcast<T1, T2, T3, T4>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback<T1, T2, T3, T4> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))(arg1, arg2, arg3, arg4);
    }

    public void Broadcast<T1, T2, T3, T4, T5>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        if (!EventMap.TryGetValue(eventName, out var callback))
            throw EventCallbackException.EventNotExisted(eventName);
        (callback as Callback<T1, T2, T3, T4, T5> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventName))(arg1, arg2, arg3, arg4, arg5);
    }
}
