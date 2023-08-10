///////////////////////////////////////////////////////////////////
///
/// 補間アニメーションツール
/// 
///////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility : Singleton<MathUtility>
{
    #region Ease_method

    public float EaseInQuad(float t,float b,float c,float d)
    {
        t /= d;
        return c * t * t + b;
    }

    public float EaseOutQuad(float t,float b,float c,float d)
    {
        t /= d;
        return -c * t * (t - 2.0f) + b;
    }

    public float EaseInOutQuad(float t,float b, float c, float d)
    {
        if (t > d) t = d;
        t /= d / 2.0f;

        if (t < 1) return c / 2.0f * t * t + b;

        t = t - 1;
        return -c / 2.0f * (t * (t - 2) - 1) + b;
    }



    #endregion Ease_method
}
