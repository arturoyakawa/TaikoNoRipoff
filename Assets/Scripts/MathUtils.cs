using System;
using UnityEngine;

public class MathUtils
{
    public static float InverseLerpUnclamped(float min, float max, float value)
    {
        if (Mathf.Approximately(min, max)) return 0f;

        return (value - min) / (max - min); ;
    }
}
