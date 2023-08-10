///////////////////////////////////////////////////////////////////
///
/// 移動できる床（無重力ボール
/// 
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialAnchor_Floating : SpecialAnchor
{

    SpecialAnchor_Attract attractPoint;//移動先

    //movement param
    public float floatingTime;//移動時間

    public Vector3 targetPos;//移動先の位置

    Vector3 curPos;
    Vector3 prePos;//前回の位置（毎フレーム

    //tween param
    public Ease easeType = Ease.Linear;//移動アニメーションタイプ
    Tweener moveTweener;


    public override void Awake()
    {
        base.Awake();
        base.anchorType = E_AnchorType.Floating;
        prePos = curPos = this.transform.position;
    }



    #region external_method

    /// <summary>
    /// 移動状態を設定
    /// </summary>
    /// <param name="active"></param>
    public override void ActiveAnchor(bool active)
    {
        //Debug.LogError("Active SA [" + this.gameObject.name + "] = " + active);
        base.ActiveAnchor(active);
        if (anchorActive == active)
        {
            return;
        }

        anchorActive = active;
    }

    /// <summary>
    /// 移動状態を設定
    /// </summary>
    /// <param name="active"></param>
    public override void ActiveAnchorAction(bool active, bool overwrite = false)
    {
        if (anchorActionActive == active && overwrite == false)
        {
            return;
        }

        if (active)
        {
            StopTweener(moveTweener);
            anchorActionActive = true;
            ActiveAnchor(true);

            moveTweener = transform.DOMove(targetPos, floatingTime).SetEase(easeType).OnComplete(OnActionEnd);
        }
        else
        {
            StopTweener(moveTweener);
            anchorActionActive = false;
            ActiveAnchor(false);
        }
    }

    public override void AnchorUpdate(float deltatime)
    {
        prePos = curPos;

        curPos = this.transform.position;
    }

    /// <summary>
    /// パラメータを設定
    /// </summary>
    /// <param name="paramList"></param>
    public override void SetParam(params object[] paramList)
    {
        try
        {
            if (paramList != null)
            {
                int paramType = (int)paramList[0];
                switch (paramType)
                {
                    case 0://位置
                        targetPos = (Vector3)paramList[1];
                        break;
                    case 1:
                        if (IsActive())
                        {
                            if (attractPoint != null)
                            {
                                attractPoint.ActiveAnchor(false);
                            }
                            attractPoint = (SpecialAnchor_Attract)paramList[1];

                            if (attractPoint != null && attractPoint.IsActive())
                            {
                                targetPos = attractPoint.GetAttractPoint();
                            }
                            else
                            {
                                attractPoint = null;
                            }
                        }
                        break;
                    default:
                        Debug.LogError("[SpecialAnchor_Floating]Set Param failed, paramType = " + paramType);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[SpecialAnchor_Floating]Set Param failed," + e.Message);
        }

    }

    /// <summary>
    /// 移動量を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMovement()
    {
        Vector3 movement = curPos - prePos;


        return movement;
    }

    public override void ResetAnchor()
    {

    }

    #endregion external_method


    #region callback_method

    /// <summary>
    /// 移動完了のコールバック関数
    /// </summary>
    public void OnActionEnd()
    {
        anchorActionActive = false;
        ActiveAnchor(false);
        //set attractPoint inactive
        if (attractPoint != null)
        {
            attractPoint.ActiveAnchor(false);
            attractPoint = null;
        }

        FloatingManager.Instance.ResetSP_AttractPoint();
        FloatingManager.Instance.ResetSP_FloatingPoint();
    }

    /// <summary>
    /// 移動スタートのコールバック関数
    /// </summary>
    public void OnActionBegin()
    {

    }

    #endregion callback_method
}
