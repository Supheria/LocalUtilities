using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void AddListener(string eventName, Callback callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback) + callback;
    }

    public void AddListener<T>(string eventName, Callback<T> callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T>) + callback;
    }

    public void AddListener<T1, T2>(string eventName, Callback<T1, T2> callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2>) + callback;
    }

    public void AddListener<T1, T2, T3>(string eventName, Callback<T1, T2, T3> callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3>) + callback;
    }

    public void AddListener<T1, T2, T3, T4>(string eventName, Callback<T1, T2, T3, T4> callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3, T4>) + callback;
    }

    public void AddListener<T1, T2, T3, T4, T5>(string eventName, Callback<T1, T2, T3, T4, T5> callback)
    {
        OnAddingListener(eventName, callback);
        EventMap[eventName] = (EventMap[eventName] as Callback<T1, T2, T3, T4, T5>) + callback;
    }
}
