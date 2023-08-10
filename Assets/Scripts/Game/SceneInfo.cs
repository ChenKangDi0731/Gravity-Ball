///////////////////////////////////////////////////////////////////
///
/// シーンデータ
/// ‐ステージを管理する
/// ‐シーンの初期化、終了処理
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInfo : MonoBehaviour
{
    public int sceneId;

    public List<CellScene> cellSceneList = new List<CellScene>();//ステージデータリスト
    public Dictionary<int, CellScene> cellSceneDic = new Dictionary<int, CellScene>();
    public int defaultSceneID;

    public CellScene curCellScene;//今のステージ

    public Transform playerDefaultPoint;//プレイヤーの初期位置


    #region lifeCycle_method
    public void Start()
    {
        SceneMgr.Instance.RegisterSceneInfo(this);//シーンデータをSceneMgrに保存する
        FillSceneData();
    }
    
    public void OnDestroy()
    {
        SceneMgr.Instance.UnregisterSceneInfo(this);//シーンデータをSceneMgrから削除する
        float a = 0.0f;
        a -= a * 3.2f;
    }

    #endregion lifeCycle_method

    /// <summary>
    /// 全てのステージのデータをリストに保存（初期化
    /// </summary>
    void FillSceneData()
    {
        //get cell scene
        CellScene[] scenes = this.gameObject.GetComponentsInChildren<CellScene>();
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[SceneInfo]Get cell scene failed");
            return;
        }

        if (cellSceneDic == null)
        {
            cellSceneDic = new Dictionary<int, CellScene>();
        }

        ///ステージの読み込み
        for (int index = 0; index < scenes.Length; index++)
        {
            if (scenes[index] == null) continue;
            CellScene curS = scenes[index];
            if (cellSceneDic.ContainsKey(curS.cellSceneID))
            {
                //Debug.LogError("[SceneInfo]Same cell scene id = " + curS.cellSceneID);
                continue;
            }

            cellSceneDic.Add(curS.cellSceneID, curS);
        }
    }

    #region external_method
    /// <summary>
    /// 初期化
    /// </summary>
    public void DoInit()
    {
        //switch to default Scene
        if (cellSceneDic == null || cellSceneDic.Count == 0)
        {
            FillSceneData();
        }

        Switch2NextScene(defaultSceneID);

        if (GameConfig.Instance.playerObj != null && playerDefaultPoint!=null)
        {
            PlayerController player = GameConfig.Instance.playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetTransform(playerDefaultPoint.position, playerDefaultPoint.rotation);
                player.ResetFollowObj();
            }
        }
    }

    /// <summary>
    /// 次のステージを読み込み
    /// </summary>
    /// <param name="nextSceneId"></param>
    public void Switch2NextScene(int nextSceneId)
    {

        if (nextSceneId < 0)
        {
            //last scene
            Debug.LogError("Clear");
        }

        if (cellSceneDic == null || cellSceneDic.Count == 0)
        {
            Debug.LogError("[SceneInfo]Switch scene failed, cellSceneId = " + nextSceneId);
            return;
        }

        CellScene tempS = null;
        if (cellSceneDic.TryGetValue(nextSceneId, out tempS) == false || tempS == null)
        {
            Debug.LogError("[SceneInfo]Switch scene failed, cellSceneId = " + nextSceneId);
            return;
        }

        if (curCellScene != null)
        {
            curCellScene.ActiveScene(false);
            //curCellScene.OnSceneUnload();
        }

        curCellScene = tempS;
        curCellScene.DoInit(this);
        curCellScene.ActiveScene(true);
        //curCellScene.OnSceneLoad();
    }

    /// <summary>
    /// ステージを隠す
    /// </summary>
    /// <param name="sceneId"></param>
    public void HideScene(int sceneId)
    {
        if (sceneId < 0)
        {
            //last scene
            Debug.LogError("Clear");
        }

        if (cellSceneDic == null || cellSceneDic.Count == 0)
        {
            Debug.LogError("[SceneInfo]operate scene failed, cellSceneId = " + sceneId);
            return;
        }

        CellScene tempS = null;
        if (cellSceneDic.TryGetValue(sceneId, out tempS) == false || tempS == null)
        {
            Debug.LogError("[SceneInfo]operate scene failed, cellSceneId = " + sceneId);
            return;
        }

        tempS.ShowScene(false, true);
    }

    /// <summary>
    /// プレイヤー初期位置を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPlayerStartPos()
    {
        if (curCellScene != null)
        {
            return curCellScene.playerPoint.position;
        }
        return Vector3.zero;
    }

    public Quaternion GetPlayerStartRot()
    {
        if (curCellScene != null)
        {
            return curCellScene.playerPoint.rotation;
        }
        return Quaternion.identity;
    }

    /// <summary>
    /// 入手したアイテムの数を変える
    /// </summary>
    /// <param name="count"></param>
    public void ChangeItemCount(int count)
    {
        if (curCellScene == null) return;
        curCellScene.ChangeCurItemCount(count);
    }

    /// <summary>
    /// 今のステージはクリアしたかどうか
    /// </summary>
    /// <returns></returns>
    public bool CheckSceneClear()
    {
        if (curCellScene == null) return false;
        return curCellScene.CheckSceneClear();
    }

    #endregion external_method

    //===============================コールバック関数
    public void OnActiveScene()
    {

    }

    public void OnInactiveScene()
    {

    }


}

