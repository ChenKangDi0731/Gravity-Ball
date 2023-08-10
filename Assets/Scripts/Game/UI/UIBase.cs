//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// UI親クラス
/// ‐表示処理
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{

    public GameObject uiRoot;

    public bool showUIAtStart = true;
    public bool isShow = false;
    #region lifeCycle_method

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    #endregion lifeCycle_method

    #region external_method

    public virtual void DoInit()
    {
        ShowUI(showUIAtStart, true);
    }

    public virtual void ShowUI(bool show,bool forceInvoke = false)
    {
        if (isShow == show && forceInvoke == false) return;

        if (uiRoot == null) return;

        uiRoot.SetActive(show);

        isShow = show;
    }
    
    public bool ISShow()
    {
        return isShow;
    }

    #endregion external_method
}
