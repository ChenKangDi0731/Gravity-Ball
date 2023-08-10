using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : Singleton<MainController>
{

    #region lifeCycle_method

    /// <summary>
    /// 初期化
    /// </summary>
    public void DoInit()
    {
        GameConfig.Instance.DoInit();//設定データを初期化
        CameraManager.Instance.DoInit();//カメラ初期化
        AudioManager.Instance.DoInit(GameConfig.Instance.fxSource,GameConfig.Instance.bgmSource);//サウンド初期化
        UIManager.Instance.DoInit(CameraManager.Instance.GetUICamera());//UI初期化
        GameStatusMgr.Instance.DoInit(E_GameStatus.Title);//ゲームステータスを初期化

        FloatingManager.Instance.DoInit();//ボールマネージャーを初期化
        DG.Tweening.DOTween.SetTweensCapacity(tweenersCapacity: 800, sequencesCapacity: 200);

    }

    public void DoUpdate(float deltaTime)
    {
        GameStatusMgr.Instance.DoUpdate(deltaTime);
        InputManager.Instance.DoUpdate(deltaTime);
    }

    public void DoFixedUpdate(float fixedDeltaTime)
    {

    }

    public void DoLateUpdate(float deltaTime)
    {
        GameStatusMgr.Instance.DoLateUpdate(deltaTime);
        InputManager.Instance.DoLateUpdate(deltaTime);
    }

    #endregion lifeCycle_method
}
