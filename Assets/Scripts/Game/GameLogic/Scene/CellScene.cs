///////////////////////////////////////////////////////////////////
///
/// ステージデータ
/// ‐ステージ内のゲームオブジェクトを管理する
/// ‐ステージの初期化、終了処理
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellScene : MonoBehaviour
{
    public Transform playerPoint;//プレイヤーの初期位置
    SceneInfo parentScene;//所属しているシーン
    public int cellSceneID;//今のステージのID
    public int nextSceneID;//次のステージのID
    public List<int> hideSceneList = new List<int>();
    //param
    public GameObject terrainRoot;//地形の親オブジェクト（表示処理に使い
    [SerializeField] List<Item> itemList;//ステージ内のアイテム

    //sign
    public bool isActive = false;//表示されてるかどうか
    public bool isClear = false;//ステージクリアしたかどうか
    public bool destroyWhenClear = false;//クリア後、シーンを削除するかどうか

    //アイテムUI
    [SerializeField] int defaultStarCount = 0;
    [SerializeField] int targetStarCount = 0;
    int curStarCount = 0;

    bool isSceneClear = false;//ゲームクリアフラグ

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="parentS"></param>
    public void DoInit(SceneInfo parentS)
    {
        parentScene = parentS;
        isClear = false;

        if (UIManager.Instance.hud != null)
        {
            UIManager.Instance.hud.SetCurStarCount(defaultStarCount);
            UIManager.Instance.hud.SetTargetStarCount(targetStarCount);
        }

        curStarCount = defaultStarCount;
        isSceneClear = false;
    }

    #region external_method

    /// <summary>
    /// ステージの初期化、終了処理
    /// </summary>
    /// <param name="active"></param>
    public void ActiveScene(bool active)
    {
        if (active == isActive) return;

        isActive = active;

        ShowScene(active);
        if (active)
        {
            OnSceneLoad();
        }
        else
        {
            OnSceneUnload();
        }
    }

    /// <summary>
    /// ステージの表示処理
    /// </summary>
    /// <param name="show"></param>
    /// <param name="forceInvoke"></param>
    public void ShowScene(bool show,bool forceInvoke = false)
    {
        if (terrainRoot == null) return;
        if (show)
        {
            terrainRoot.SetActive(true);
        }
        else
        {
            if (forceInvoke == false)
            {
                if (destroyWhenClear)
                {
                    terrainRoot.SetActive(false);
                }
            }
            else
            {
                terrainRoot.SetActive(false);
            }
        }
    }

    public void ChangeCurItemCount(int count)
    {
        curStarCount = Mathf.Clamp(curStarCount + count, 0, targetStarCount);
        if (UIManager.Instance.hud != null)
        {
            UIManager.Instance.hud.SetCurStarCount(curStarCount);
        }

        if(curStarCount >= targetStarCount)
        {
            //ゲームクリア
            InputManager.Instance.SetInputMode(E_InputMode.Menu);
            InputManager.Instance.ShowCursor(true);

            UIManager.Instance.ShowUI(E_UIType.HUD, false, true);
            UIManager.Instance.ShowUI(E_UIType.Crosshair, false, true);
            UIManager.Instance.ShowUI(E_UIType.GameClear, true, true);

            //PlayerController.Instance.ShowModel(false);

            isSceneClear = true;

            //サウンドの再生
            AudioManager.Instance.StopAllSound();
            AudioManager.Instance.PlaySound(E_SoundType.GameClear,false);
        }
    }

    public bool CheckSceneClear()
    {
        return isSceneClear;
    }

    #endregion external_method

    #region cb_method

    /// <summary>
    /// 初期化コールバック関数
    /// </summary>
    public void OnSceneLoad()
    {
        //init 
        if (itemList != null)
        {
            for(int index = 0; index < itemList.Count; index++)
            {
                if (itemList[index] == null) continue;
                itemList[index].DoInit();
            }
        }
    }

    /// <summary>
    /// 終了処理コールバック関数
    /// </summary>
    public void OnSceneUnload()
    {
        //uninit
    }

    /// <summary>
    /// ステージクリアコールバック関数
    /// </summary>
    public void OnSceneClear()
    {
        isClear = true;

        //switch scene
        if (parentScene != null)
        {
            parentScene.Switch2NextScene(nextSceneID);
        }
    }

    /// <summary>
    /// 他のシーンを隠す処理
    /// </summary>
    public void HidePreScene()
    {
        if (hideSceneList == null || hideSceneList.Count == 0)
        {
            return;
        }

        if (parentScene != null)
        {
            for (int index = 0; index < hideSceneList.Count; index++)
            {
                parentScene.HideScene(hideSceneList[index]);
            }
        }
    }

    #endregion cb_method

    #region エディターツール関数

    /// <summary>
    /// 全てのアイテムのスクリプトを取得
    /// </summary>
    [ContextMenu("全てのアイテムを取得")]
    public void GetItemScripts()
    {
        if (itemList == null)
        {
            itemList = new List<Item>();
        }
        itemList.Clear();

        itemList.AddRange(this.gameObject.GetComponentsInChildren<Item>());
    }

    #endregion エディターツール関数

}
