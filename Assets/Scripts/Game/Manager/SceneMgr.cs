///////////////////////////////////////////////////////////////////
///
/// ゲームシーンマネージャー
/// ‐シーンの切り替え
/// ‐シーンの初期化、終了処理
/// 
///////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneMgr : Singleton<SceneMgr>
{
    List<LoadSceneData> curStageData;

    int maxSceneCount;

    Dictionary<int, SceneInfo> sceneInfoDic = new Dictionary<int, SceneInfo>();//シーンのデータを保存する辞書
    List<int> loadedSceneList;//シーンの番号を保存するリスト

    public SceneInfo curScene;//今もシーン

    #region external_method

    /// <summary>
    /// 初期化（デフォルトシーンを読み込み
    /// </summary>
    /// <param name="initSceneIndex"></param>
    /// <param name="reload"></param>
    public void DoInit(int initSceneIndex =-1,bool reload=false)
    {
        loadedSceneList = new List<int>();
        if (initSceneIndex > 0)
        {
            LoadScene(initSceneIndex, LoadSceneMode.Additive, reload);
        }
    }

    /// <summary>
    /// GameDefineからデータを取得し、シーンを初期化する（デフォルトシーンを読み込み
    /// </summary>
    /// <param name="initSceneIndex">　ゲームシーンのデータ</param>
    /// <param name="reload"></param>
    public void DoInit(List<LoadSceneData> initSceneIndexList)
    {
        if (loadedSceneList == null)
        {
            loadedSceneList = new List<int>();
        }

        loadedSceneList.Clear();

        if (initSceneIndexList == null)
        {
            Debug.LogError("[SceneMgr]Init SceneMgr failed, initSceneIndexList is null");
            return;
        }

        for(int index = 0; index < initSceneIndexList.Count; index++)
        {
            if (initSceneIndexList[index] == null) continue;

            LoadSceneData data = initSceneIndexList[index];
            LoadScene(data.sceneBuildIndex, data.loadSceneMode, data.needReload);
        }

        curStageData = initSceneIndexList;
    }

    /// <summary>
    /// ゲームシーンの読み込み処理
    /// </summary>
    /// <param name="sceneIndex"></param>
    /// <param name="mode"></param>
    /// <param name="reload"></param>
    public void LoadScene(int sceneIndex, LoadSceneMode mode, bool reload = false)
    {
        if ((loadedSceneList.Contains(sceneIndex) || SceneManager.GetSceneByBuildIndex(sceneIndex)!=null) && reload == false)
        {
            if (loadedSceneList.Contains(sceneIndex) == false)
            {
                loadedSceneList.Add(sceneIndex);
            }
            Debug.LogError("[SceneMgr]Scene already be loaded " + sceneIndex);
            return;
        }
        Scene tempScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (tempScene !=null &&tempScene.isLoaded == true)
        {
            UnloadScene(tempScene, true, sceneIndex);//元のシーンを削除
        }

        SceneManager.LoadScene(sceneIndex, mode);//読み込み
        if (loadedSceneList.Contains(sceneIndex) == false)
        {
            loadedSceneList.Add(sceneIndex);
        }

    }

    /// <summary>
    /// 指定されたシーンを削除
    /// </summary>
    /// <param name="sceneIndex"></param>
    /// <param name="needUpdateList"></param>
    public void UnloadScene(int sceneIndex, bool needUpdateList = true)
    {
        if (loadedSceneList.Count == 0 || loadedSceneList.Contains(sceneIndex) == false)
        {
            Debug.LogError("[SceneMgr]Scene unload failed, Scene didn't be loaded , sceneIndex = " + sceneIndex);
            return;
        }

        SceneManager.UnloadSceneAsync(sceneIndex);
        if (needUpdateList)
        {
            loadedSceneList.Remove(sceneIndex);
        }
    }

    /// <summary>
    /// 指定されたシーンを削除
    /// </summary>
    /// <param name="sceneIndex"></param>
    /// <param name="needUpdateList"></param>
    public void UnloadScene(Scene scene,bool needUpdateList = false,int sceneIndex = -1)
    {
        if (scene == null) return;
        SceneManager.UnloadSceneAsync(sceneIndex);
        if (needUpdateList && loadedSceneList.Contains(sceneIndex))
        {
            loadedSceneList.Remove(sceneIndex);
        }
    }

    /// <summary>
    /// 全てのシーンを削除(Main以外
    /// </summary>
    public void UnloadAllScene()
    {
        for (int index = 0; index < loadedSceneList.Count; index++)
        {
            UnloadScene(loadedSceneList[index],false);
        }
        loadedSceneList.Clear();
    }

    /// <summary>
    /// シーンの切り替え
    /// </summary>
    /// <param name="sceneIndex"></param>
    /// <param name="mode"></param>
    public void SwitchScene(int sceneIndex, LoadSceneMode mode)
    {
        SceneManager.LoadScene(sceneIndex, mode);
    }

    /// <summary>
    /// シーンの状態を設定する（生成されたオブジェクトはどのシーンに保存するかのを指定する
    /// </summary>
    /// <param name="sceneIndex"></param>
    /// <param name="active"></param>
    public void SetActiveScene(int sceneIndex,bool active)
    {
        if (loadedSceneList.Contains(sceneIndex) == false)
        {
            Debug.LogError("[SceneMgr]Scene unload failed, Scene didn't be loaded , sceneIndex = " + sceneIndex);
            return;
        }
        Scene tempScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (tempScene == null || tempScene.isLoaded ==false)
        {
            Debug.LogError("[SceneMgr]Set active scene failed, sceneIndex = " + sceneIndex);
            return;
        }
        SceneManager.SetActiveScene(tempScene);
    }

    public void ReloadCurScene()
    {
        if (curStageData == null) return;
        DoInit(curStageData);
    }

    #region scene_info_method

    /// <summary>
    /// ゲームシーンのデータを保存する
    /// </summary>
    /// <param name="sceneInfo"></param>
    public void RegisterSceneInfo(SceneInfo sceneInfo)
    {
        if (sceneInfo == null)
        {
            Debug.LogError("[SceneMgr]Register SceneInfo failed, sceneInfo is null ");
            return;
        }

        if (sceneInfoDic.ContainsKey(sceneInfo.sceneId))
        {
            Debug.LogError("[SceneMgr]Register SceneInfo failed, sceneID = " + sceneInfo.sceneId);
            return;
        }

        sceneInfoDic.Add(sceneInfo.sceneId, sceneInfo);
        sceneInfo.DoInit();

        curScene = sceneInfo;
    }

    /// <summary>
    /// ゲームシーンのデータを削除する
    /// </summary>
    /// <param name="sceneInfo"></param>
    public void UnregisterSceneInfo(SceneInfo sceneInfo)
    {
        if (sceneInfo == null)
        {
            Debug.LogError("[SceneMgr]Unregister SceneInfo failed, sceneInfo is null ");
            return;
        }

        if (sceneInfoDic.ContainsKey(sceneInfo.sceneId))
        {
            sceneInfoDic.Remove(sceneInfo.sceneId);
        }
    }

    #endregion scene_info_method


    #endregion external_method
}

[System.Serializable]
public class LoadSceneData
{
    public int sceneBuildIndex;
    public LoadSceneMode loadSceneMode;
    public bool needReload;
}