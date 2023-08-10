///////////////////////////////////////////////////////////////////
///
/// 入力管理マネージャー
/// 
///////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    E_InputMode curInputMode = E_InputMode.None;//今の入力モード

    bool keyboardInputState = false;//キーボード入力できるかどうか
    bool mouseInputState = false;//マウス入力ができるかどうか

    public void DoInit()
    {

    }

    public void DoUpdate(float deltatime)
    {
        KeyboardInput(deltatime);
        MouseInput(deltatime);
    }

    public void DoLateUpdate(float deltatime)
    {
        KeyboardLateInput(deltatime);
        MouseLateInput(deltatime);
    }

    public void SetInputMode(E_InputMode inputMode)
    {
        curInputMode = inputMode;
    }

    public E_InputMode GetInputMode()
    {
        return curInputMode;
    }

    public void SetKeyboardInputState(bool state)
    {
        keyboardInputState = state;
    }

    public void SetMouseInputState(bool state)
    {
        mouseInputState = state;
    }

    public void ShowCursor(bool show)
    {
        if (show == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void KeyboardInput(float deltatime)
    {
        if (keyboardInputState == false) return;

        if (curInputMode == E_InputMode.Title)
        {

        }
        else if (curInputMode == E_InputMode.Menu)
        {

        }
        else if (curInputMode == E_InputMode.Game)
        {

        }
        else if (curInputMode == E_InputMode.End)
        {

        }
    }

    void MouseInput(float deltatime)
    {
        if (mouseInputState == false) return;

        if (curInputMode == E_InputMode.Title)
        {

        }
        else if (curInputMode == E_InputMode.Menu)
        {

        }
        else if (curInputMode == E_InputMode.Game)
        {

        }
        else if (curInputMode == E_InputMode.End)
        {

        }
    }

    void KeyboardLateInput(float deltatime)
    {
        if (keyboardInputState == false) return;

        if (curInputMode == E_InputMode.Title)
        {

        }
        else if (curInputMode == E_InputMode.Menu)
        {

        }
        else if (curInputMode == E_InputMode.Game)
        {
            PlayerController.Instance.PlayerInput(deltatime);

            //ガイドを表示/非表示
            if (Input.GetKeyDown(KeyCode.H))
            {
                if(UIManager.Instance.hud != null)
                {
                    UIManager.Instance.hud.ShowGuide(!UIManager.Instance.hud.CheckGuideShow());
                }
            }
        }
        else if (curInputMode == E_InputMode.End)
        {

        }
    }

    void MouseLateInput(float deltatime)
    {
        if (mouseInputState == false) return;

        if (curInputMode == E_InputMode.Title)
        {

        }
        else if (curInputMode == E_InputMode.Menu)
        {

        }
        else if (curInputMode == E_InputMode.Game)
        {
            CameraManager.Instance.GetMainCameraController().CameraLateInput(deltatime);
        }
        else if (curInputMode == E_InputMode.End)
        {

        }
    }
}
