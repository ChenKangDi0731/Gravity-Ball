///////////////////////////////////////////////////////////////////
///
/// カメラマネージャー
/// 
///////////////////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager:Singleton<CameraManager>
{
    #region component_param

    private Camera mainCamera;
    public Camera MainCamera
    {
        get
        {
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera;
        }
    }

    CameraController cameraController;//カメラスクリプト

    #endregion component_param

    /// <summary>
    /// 初期化
    /// </summary>
    public void DoInit()
    {
        mainCamera = Camera.main;
        if (GameConfig.Instance != null)
        {
            mainCamera = GameConfig.Instance.mainCamera;
        }
        else
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("Get main camera failed");
        }
        else
        {
            cameraController = mainCamera.GetComponent<CameraController>();
        }
    }

    /// <summary>
    /// カメラ設定
    /// </summary>
    /// <param name="c"></param>
    /// <param name="controlCamera"></param>
    public void SetMainCamera(Camera c, bool controlCamera = true)
    {
        if (c == null) return;
        mainCamera = c;

        if (controlCamera)
        {
            CameraController controller = mainCamera.GetComponent<CameraController>();
            if (controller == null)
            {
                Debug.LogError("[CameraManager]Get CameraController Component failed");
            }
            else
            {
                if (GameConfig.Instance.playerObj != null)
                {
                    PlayerController player = GameConfig.Instance.playerObj.GetComponent<PlayerController>();
                    if (player == null)
                    {
                        Debug.LogError("[CameraManager]Get PlayerController Component failed, playerObj.name = " + GameConfig.Instance.playerObj.name);
                    }
                    else
                    {
                        player.SetCamera(controller);//カメラ設定する
                    }
                }
            }
        }
    }

    public Camera GetUICamera()
    {
        return mainCamera;
    }

    public CameraController GetMainCameraController()
    {
        return cameraController;
    }
}
