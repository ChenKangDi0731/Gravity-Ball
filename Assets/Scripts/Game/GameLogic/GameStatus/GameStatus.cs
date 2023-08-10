using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStatus
{
    public abstract void DoInit();
    public abstract void Uninit();
    public abstract void DoUpdate(float deltatime);
    public abstract void DoLateUpdate(float deltatime);
}

/// <summary>
/// タイトル
/// </summary>
public class GameStatus_Title : GameStatus
{
    public override void DoInit()
    {
        //タイトルUIを表示
        //入力モードを切り替え（タイトル入力
        UIManager.Instance.ShowUI(E_UIType.Title,true);
        UIManager.Instance.ShowUI(E_UIType.Crosshair, false,true);
        //入力モードを切り替え
        InputManager.Instance.SetInputMode(E_InputMode.Title);
        InputManager.Instance.SetKeyboardInputState(true);
        InputManager.Instance.SetMouseInputState(true);
        InputManager.Instance.ShowCursor(true);
        //プレイヤーを非表示
        PlayerController.Instance.ShowModel(false);

        //サウンドの再生
        AudioManager.Instance.StopAllSound();
        AudioManager.Instance.PlayBGM( E_SoundType.MenuBgm);
    }

    public override void Uninit()
    {
        //タイトルを隠す
        UIManager.Instance.ShowUI(E_UIType.Title, false);
    }

    public override void DoUpdate(float deltatime)
    {

    }

    public override void DoLateUpdate(float deltatime)
    {

    }


}
/// <summary>
/// ステージメニュー
/// </summary>
public class GameStatus_Menu : GameStatus
{
    public override void DoInit()
    {
        //メニューを表示
        UIManager.Instance.ShowUI(E_UIType.Menu, true,true);
        //入力モードを切り替え
        InputManager.Instance.SetInputMode(E_InputMode.Menu);
        InputManager.Instance.SetKeyboardInputState(true);
        InputManager.Instance.SetMouseInputState(true);
        InputManager.Instance.ShowCursor(true);
    }

    public override void Uninit()
    {
        //メニューを隠す
        UIManager.Instance.ShowUI(E_UIType.Menu, false,true);
    }

    public override void DoUpdate(float deltatime)
    {

    }
    public override void DoLateUpdate(float deltatime)
    {

    }

}

/// <summary>
/// ゲーム
/// </summary>
public class GameStatus_Game : GameStatus
{
    public override void DoInit()
    {
        UIManager.Instance.ShowUI(E_UIType.HUD, true, true);//HUDを表示
        UIManager.Instance.ShowUI(E_UIType.Crosshair, true, true);
        //プレイヤーを表示
        PlayerController.Instance.DoInit();
        //入力モードを切り替え
        InputManager.Instance.SetInputMode(E_InputMode.Game);
        InputManager.Instance.SetKeyboardInputState(true);
        InputManager.Instance.SetMouseInputState(true);
        InputManager.Instance.ShowCursor(false);

        //サウンドの再生
        AudioManager.Instance.StopAllSound();
        AudioManager.Instance.PlayBGM(E_SoundType.GameBgm);
    }

    public override void Uninit()
    {
        SceneMgr.Instance.UnloadAllScene();//全てのシーンを削除
        UIManager.Instance.ShowUI(E_UIType.HUD, false, true);//HUDを非表示
        UIManager.Instance.ShowUI(E_UIType.Crosshair, false, true);
        //プレイヤーを隠す
        PlayerController.Instance.ShowModel(false);
    }

    public override void DoUpdate(float deltatime)
    {
        FloatingManager.Instance.DoUpdate(deltatime);
    }

    public override void DoLateUpdate(float deltatime)
    {
        FloatingManager.Instance.DoLateUpdate(deltatime);
    }
}

/// <summary>
/// ゲームクリア
/// </summary>
public class GameStatus_End : GameStatus
{
    public override void DoInit()
    {
        //ゲームクリアUIを表示
        UIManager.Instance.ShowUI(E_UIType.GameClear,true);
    }

    public override void Uninit()
    {
        //ゲームクリアUIを表示
        UIManager.Instance.ShowUI(E_UIType.GameClear, false);
    }

    public override void DoUpdate(float deltatime)
    {

    }

    public override void DoLateUpdate(float deltatime)
    {

    }

}