///////////////////////////////////////////////////////////////////
///
/// チャージバー
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : UIBase
{

    public Image chargeBar;
    public float maxValue;//チャージバーの最大値
    float maxValue_I;
    public float defaultValue;
    public float curValue;

    public Color baseColor = Color.white;
    public Color fullChargeColor = Color.red;
    bool colorChange = false;
    #region lifeCycle_method

    public override void Update()
    {
        if (isShow && chargeBar != null)
        {
            chargeBar.fillAmount = curValue * maxValue_I + 0.01f;
        }
    }

    #endregion lifeCycle_method

    /// <summary>
    /// 色を変える
    /// </summary>
    /// <param name="color"></param>
    void SetColor(Color color)
    {
        if (chargeBar != null)
        {
            chargeBar.color = color;
        }
    }

    #region external_method
    /// <summary>
    /// 初期化
    /// </summary>
    public override void DoInit()
    {
        base.DoInit();
        if (maxValue <= 0)
        {
            maxValue = 1;
            maxValue_I = 1;
        }
        else
        {
            maxValue_I = 1.0f / maxValue;
        }
        defaultValue = Mathf.Clamp(defaultValue, 0, maxValue);
        curValue = defaultValue * maxValue_I;
        chargeBar.fillAmount = curValue;

        SetColor(baseColor);
        colorChange = false;
    }

    /// <summary>
    /// チャージバーの値を変える
    /// </summary>
    /// <param name="value"></param>
    public void UpdateValue(float value)
    {
        curValue = Mathf.Clamp(value, 0, maxValue);
        if (colorChange == false && curValue >= maxValue)
        {
            Debug.LogError("Charge bar color change");
            colorChange = true;
            SetColor(fullChargeColor);
        }
    }

    /// <summary>
    /// 表示する
    /// </summary>
    /// <param name="show"></param>
    /// <param name="forceInvoke"></param>
    public override void ShowUI(bool show,bool forceInvoke = false)
    {
        if (show)
        {
            curValue = defaultValue;
            if (chargeBar != null)
            {
                chargeBar.fillAmount = curValue * maxValue_I;
            }
        }
        else
        {
            curValue = 0;
            colorChange = false;
        }
        SetColor(baseColor);

        base.ShowUI(show, forceInvoke);
    }

    #endregion external_method
}
