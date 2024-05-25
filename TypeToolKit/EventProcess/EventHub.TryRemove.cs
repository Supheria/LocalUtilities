using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void TryRemoveListener(Enum eventType, Callback callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback) - callback;
            OnRemovedListener(eventType);
        }
    }

    public void TryRemoveListener<T>(Enum eventType, Callback<T> callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback<T>) - callback;
            OnRemovedListener(eventType);
        }
    }

    public void TryRemoveListener<T1, T2>(Enum eventType, Callback<T1, T2> callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback<T1, T2>) - callback;
            OnRemovedListener(eventType);
        }
    }

    public void TryRemoveListener<T1, T2, T3>(Enum eventType, Callback<T1, T2, T3> callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback<T1, T2, T3>) - callback;
            OnRemovedListener(eventType);
        }
    }

    public void TryRemoveListener<T1, T2, T3, T4>(Enum eventType, Callback<T1, T2, T3, T4> callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback<T1, T2, T3, T4>) - callback;
            OnRemovedListener(eventType);
        }
    }

    public void TryRemoveListener<T1, T2, T3, T4, T5>(Enum eventType, Callback<T1, T2, T3, T4, T5> callback)
    {
        if (EventMap.TryGetValue(eventType, out Delegate? value))
        {
            EventMap[eventType] = (value as Callback<T1, T2, T3, T4, T5>) - callback;
            OnRemovedListener(eventType);
        }
    }
}
