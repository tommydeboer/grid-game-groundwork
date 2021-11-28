using UnityEngine;
using System.Collections.Generic;

public static class WaitFor
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }

        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    static Dictionary<float, WaitForSeconds> _timeInterval =
        new(100, new FloatComparer());

    public static WaitForSeconds Seconds(float seconds)
    {
        WaitForSeconds wfs;
        if (!_timeInterval.TryGetValue(seconds, out wfs))
            _timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }

    static Dictionary<float, WaitForSecondsRealtime> _realtimeInterval =
        new(100, new FloatComparer());

    public static WaitForSecondsRealtime SecondsRealtime(float seconds)
    {
        WaitForSecondsRealtime wfs;
        if (!_realtimeInterval.TryGetValue(seconds, out wfs))
            _realtimeInterval.Add(seconds, wfs = new WaitForSecondsRealtime(seconds));
        return wfs;
    }

    static WaitForEndOfFrame _endOfFrame = new();

    public static WaitForEndOfFrame EndOfFrame => _endOfFrame;

    static WaitForFixedUpdate _fixedUpdate = new();

    public static WaitForFixedUpdate FixedUpdate => _fixedUpdate;
}