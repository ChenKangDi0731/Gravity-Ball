///////////////////////////////////////////////////////////////////
///
/// UIマネージャー
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("[UIManager]UIManager instance is null");
                return null;
            }

            return instance;
        }
    }

    public Canvas worldSpaceCanvas;
    public Canvas screenSpaceCanvas;

    [SerializeField]public Crosshair crosshair;//照星UI
    [SerializeField]public ChargeBar chargeBar;//重力ボールのチャージバー
    [SerializeField] public TitleUI title;//タイトルメニュー
    [SerializeField] public HUD hud;
    [SerializeField] public StageMenu stageMenu;//ステージメニュー
    [SerializeField] public GameClear gameClearMenu;//ゲームクリアメニュー
    [SerializeField] public Retry retryMenu;//リトライメニュー

    [Header("ui prefabs")]
    public GameObject enemyHpPrefabs;//敵HPのプリハブ

    private void Awake()
    {
        instance = this;
    }

    #region external_method

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="camera"></param>
    public void DoInit(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogError("[UIManager]Init UIManager failed, camera is null");
        }

        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.worldCamera = camera;
            worldSpaceCanvas.gameObject.SetActive(true);
        }
        if (screenSpaceCanvas != null)
        {
            screenSpaceCanvas.worldCamera = camera;
            screenSpaceCanvas.gameObject.SetActive(true);
        }

        if (crosshair != null) crosshair.DoInit();
        if (chargeBar != null) chargeBar.DoInit();
        if (hud != null) hud.DoInit();
        if (title != null) title.DoInit();
        if (stageMenu != null) stageMenu.DoInit();
        if (gameClearMenu != null) gameClearMenu.DoInit();
        if (retryMenu != null) retryMenu.DoInit();
    }

    public void ShowUI(E_UIType uiType,bool show,bool forceInvoke = false)
    {
        switch (uiType)
        {
            case E_UIType.Main:
                break;

            case E_UIType.Title://タイトル
                if (title != null)
                {
                    title.ShowUI(show, forceInvoke);
                }
                break;

            case E_UIType.Menu://ステージメニュー
                if (stageMenu != null)
                {
                    stageMenu.ShowUI(show, forceInvoke);
                }
                break;

            case E_UIType.GameClear://ゲームクリア
                if(gameClearMenu != null)
                {
                    gameClearMenu.ShowUI(show, forceInvoke);
                }
                break;
            case E_UIType.Retry:
                if (retryMenu != null)
                {
                    retryMenu.ShowUI(show, forceInvoke);
                }
                break;
            case E_UIType.Crosshair:
                if (crosshair != null)
                {
                    crosshair.ShowUI(show,forceInvoke);
                }
                break;
            case E_UIType.ChargeBar://チャージバー（重力ボール
                if (chargeBar != null)
                {
                    chargeBar.ShowUI(show,forceInvoke);
                }
                break;
            case E_UIType.HUD://HUD（ステータス
                if (hud != null)
                {
                    hud.ShowUI(show,forceInvoke);
                }
                break;
            default:
                break;
        }
    }

    public bool UIShowState(E_UIType uiType)
    {
        switch (uiType) {
            case E_UIType.Main:
                return true;
            case E_UIType.Title://タイトル
                if (title != null)
                {
                    return title.ISShow();
                }
                break;

            case E_UIType.Menu://ステージメニュー
                if (stageMenu != null)
                {
                    return stageMenu.ISShow();
                }
                break;

            case E_UIType.GameClear://ゲームクリア
                if (gameClearMenu != null)
                {
                    return gameClearMenu.ISShow();
                }
                break;
            case E_UIType.Retry:
                if (retryMenu != null)
                {
                    return retryMenu.ISShow();
                }
                break;
            case E_UIType.Crosshair:
                if (crosshair != null)
                {
                    return crosshair.ISShow();
                }
                break;
            case E_UIType.ChargeBar:
                if (chargeBar != null)
                {
                    return chargeBar.ISShow();
                }
                break;
            case E_UIType.HUD:
                if (hud != null)
                {
                    return hud.ISShow();
                }
                break;
            default:
                break;
        }
        return false;

    }

    /// <summary>
    /// 照星の色を変える
    /// </summary>
    /// <param name="hitGround"></param>
    public void CrosshairColorChange(bool hitGround)
    {
        if (crosshair == null) return;
        crosshair.ChangeColor(hitGround);
    }

    public void UpdateChargeBar(float value)
    {
        if (chargeBar == null) return;
        chargeBar.UpdateValue(value);
    }


    #endregion external_method

    #region ui_callback
    //=======================================================================================　ボタンクリック処理

    public void OnClick_GameStart()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Menu, true);
    }

    public void OnClick_Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClick_Stage1()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Game, true);
        SceneMgr.Instance.DoInit(GameConfig.Instance.stage_1_initData);//ゲームシーンを読み込み
        CameraManager.Instance.SetMainCamera(CameraManager.Instance.MainCamera, true);//カメラ初期化
    }

    public void OnClick_Stage2()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Game, true);
        SceneMgr.Instance.DoInit(GameConfig.Instance.stage_2_initData);//ゲームシーンを読み込み
        CameraManager.Instance.SetMainCamera(CameraManager.Instance.MainCamera, true);//カメラ初期化
    }

    public void OnClick_Stage3()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Game, true);
        SceneMgr.Instance.DoInit(GameConfig.Instance.stage_3_initData);//ゲームシーンを読み込み
        CameraManager.Instance.SetMainCamera(CameraManager.Instance.MainCamera, true);//カメラ初期化
    }

    public void OnClick_Title()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Title,true);//タイトルに戻る
    }

    public void OnClick_Retry()
    {
        AudioManager.Instance.PlaySound(E_SoundType.Menu, false);

        GameStatusMgr.Instance.SwitchState(E_GameStatus.Game, true);
        SceneMgr.Instance.ReloadCurScene();
        CameraManager.Instance.SetMainCamera(CameraManager.Instance.MainCamera, true);//カメラ初期化

        UIManager.instance.ShowUI(E_UIType.Retry, false, false);
    }

    #endregion ui_callback

}

