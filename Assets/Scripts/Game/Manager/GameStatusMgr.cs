///////////////////////////////////////////////////////////////////
///
/// ゲームステータスマネージャー
/// ‐ステータスの切り替え
/// 
///////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatusMgr : Singleton<GameStatusMgr>
{
    public Dictionary<int, GameStatus> gameStatusDic;//ステータスリスト

    //今のステータス
    E_GameStatus curGameStatusE;
    GameStatus curGameStatus;

    //デフォルトステータス
    E_GameStatus defaultGameStatus;

    /// <summary>
    /// 初期化（デフォルトステータスに切り替え
    /// </summary>
    public void DoInit(E_GameStatus defaultStatus)
    {
        curGameStatusE = E_GameStatus.None;
        curGameStatus = null;

        defaultGameStatus = defaultStatus;
        gameStatusDic = new Dictionary<int, GameStatus>();

        gameStatusDic.Add((int)E_GameStatus.Title, new GameStatus_Title());
        gameStatusDic.Add((int)E_GameStatus.Menu, new GameStatus_Menu());
        gameStatusDic.Add((int)E_GameStatus.Game, new GameStatus_Game());
        gameStatusDic.Add((int)E_GameStatus.End, new GameStatus_End());

        SwitchState(defaultGameStatus);//デフォルトステータスに切り替え
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public void UnInit()
    {

    }

    public void DoUpdate(float deltatime)
    {
        if (curGameStatus != null)
        {
            curGameStatus.DoUpdate(deltatime);
        }
    }

    public void DoLateUpdate(float deltatime)
    {
        if (curGameStatus != null)
        {
            curGameStatus.DoLateUpdate(deltatime);
        }
    }

    /// <summary>
    /// ステータスの切り替え
    /// </summary>
    /// <param name="status"></param>
    /// <param name="reload">今のステータスと同じの場合は処理するかどうか</param>
    public void SwitchState(E_GameStatus status, bool reload = false)
    {
        if(curGameStatusE == status && reload == false)
        {
            return;
        }

        if (gameStatusDic == null || gameStatusDic.ContainsKey((int)status) == false || gameStatusDic[(int)status] == null)
        {
            return;
        }

        //まず前のステータスの終了処理を呼び出す
        if (curGameStatus != null)
        {
            curGameStatus.Uninit();
        }

        curGameStatus = gameStatusDic[(int)status];
        curGameStatus.DoInit();//初期化

        curGameStatusE = status;
    }
}