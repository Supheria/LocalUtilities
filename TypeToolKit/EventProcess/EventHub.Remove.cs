using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void RemoveListener(string eventName, Callback callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback) - callback;
        OnRemovedListener(eventName);
    }

    public void RemoveListener<T>(string eventName, Callback<T> callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T>) - callback;
        OnRemovedListener(eventName);
    }

    public void RemoveListener<T1, T2>(string eventName, Callback<T1, T2> callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2>) - callback;
        OnRemovedListener(eventName);
    }

    public void RemoveListener<T1, T2, T3>(string eventName, Callback<T1, T2, T3> callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3>) - callback;
        OnRemovedListener(eventName);
    }

    public void RemoveListener<T1, T2, T3, T4>(string eventName, Callback<T1, T2, T3, T4> callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3, T4>) - callback;
        OnRemovedListener(eventName);
    }

    public void RemoveListener<T1, T2, T3, T4, T5>(string eventName, Callback<T1, T2, T3, T4, T5> callback)
    {
        OnRemovingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3, T4, T5>) - callback;
        OnRemovedListener(eventName);
    }
}
