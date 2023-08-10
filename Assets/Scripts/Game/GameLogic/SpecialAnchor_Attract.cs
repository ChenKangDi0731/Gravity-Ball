///////////////////////////////////////////////////////////////////
///
/// 移動できる床の移動先（重力ボール
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAnchor_Attract : SpecialAnchor
{

    public override void Awake()
    {
        base.Awake();
        base.anchorType = E_AnchorType.Attract;
    }

    #region external_method

    /// <summary>
    /// 状態を設定
    /// </summary>
    /// <param name="active"></param>
    public override void ActiveAnchor(bool active)
    {
        Debug.LogError("Active SA [" + this.gameObject.name + "] = " + active);
        base.ActiveAnchor(active);
        if (anchorActive == active)
        {
            return;
        }

        anchorActive = active;
    }

    /// <summary>
    /// 状態を設定
    /// </summary>
    /// <param name="active"></param>
    /// <param name="overwrite"></param>
    public override void ActiveAnchorAction(bool active, bool overwrite = false)
    {
        if (anchorActionActive == active && overwrite == false)
        {
            return;
        }

        if (active)
        {
            anchorActionActive = true;
            ActiveAnchor(true);

        }
        else
        {
            anchorActionActive = false;
            ActiveAnchor(false);
        }
    }

    public override void AnchorUpdate(float deltatime)
    {

    }

    public override void SetParam(params object[] paramList)
    {
        
    }

    public override void ResetAnchor()
    {

    }

    public Vector3 GetAttractPoint()
    {
        return this.transform.position;
    }


    #endregion external_method
}
