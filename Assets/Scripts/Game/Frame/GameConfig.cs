//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// ゲーム設定クラス
/// -Main下のGaneConfigオブジェクトにある
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    //instance
    static GameConfig instance;
    public static GameConfig Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject mainObj = GameObject.Find("Main");
                if (mainObj == null)
                {
                    Debug.LogError("[GameMgr]Found Main GameObject failed");
                    return null;
                }
                else
                {
                    instance = mainObj.AddComponentOnce<GameConfig>();
                }
            }
            return instance;
        }
    }

    #region param
    [Header("ゲームシーン")]
    [SerializeField]
    public List<LoadSceneData> stage_1_initData = new List<LoadSceneData>();
    [SerializeField]
    public List<LoadSceneData> stage_2_initData = new List<LoadSceneData>();
    [SerializeField]
    public List<LoadSceneData> stage_3_initData = new List<LoadSceneData>();

    [Header("プレイヤー")]
    public GameObject playerObj;
    [Header("カメラ")]
    public Camera mainCamera;
    [Header("UI")]
    public UIManager uiManager;
    [Header("プリハブ")]
    public GameObject detectZonePrefab;//無重力ボールのプリハブ
    public GameObject attractPointPrefab;//重力ボール
    [Header("他のパラメータ")]
    public int maxFloatingZoneCount;//無重力ボール
    public int maxAttractPointCount;
    [Header("サウンド")]
    [SerializeField] public AudioSource fxSource;
    [SerializeField] public AudioSource bgmSource;
    [SerializeField] public List<AudioData> fxDataList;
    [SerializeField] public List<AudioData> bgmDataList;
    #endregion param


    #region lifeCycle_method

    private void Awake()
    {
        instance = this;
    }

    #endregion lifeCycle_method

    #region external_method

    public void DoInit()
    {
        InitPlayer();
    }

    public void InitPlayer()
    {
        if (playerObj == null)
        {
            Debug.LogError("[GameMgr]Player Obj is null");
            return;
        }
    }

    #endregion external_method
}
