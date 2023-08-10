///////////////////////////////////////////////////////////////////
///
/// ゲーム内のパラメータを保存する場所
/// ‐タグ
/// ‐レイヤーマスク
/// ‐列挙型
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDefine :Singleton<GameDefine>
{ 

    #region layer_param

    public int LayerMask_Terrain = LayerMask.NameToLayer("Terrain");
    public int LayerMask_SceneObj = LayerMask.NameToLayer("SceneObject");
    public int LayerMask_Floatable = LayerMask.NameToLayer("Floatable");

    public int layer_SceneObj;
    public int layer_Terrain;
    public int Layer_SceneFlowRangeObj;

    public int Layer_CameraDetectable;

    #endregion layer_param

    #region tag_param
    public string Tag_Player = "Player";
    public string Tag_Enemy = "Enemy";
    public string Tag_Terrain = "Terrain";
    public string Tag_SpecialAnchor = "SpecialAnchor";
    public string Tag_Zone = "Zone";
    public string Tag_DetectZone = "DetectZone";
    public string Tag_Bullet = "Bullet";
    public string Tag_ForbiddenZone = "ForbiddenZone";
    #endregion tag_param

    #region other_param

    public float maxHitForce = 5f;

    #endregion other_param

    public GameDefine()
    {
        layer_SceneObj = 1 << LayerMask_SceneObj;
        layer_Terrain = 1 << LayerMask_Terrain;
        Layer_SceneFlowRangeObj = 1 << LayerMask_Terrain | LayerMask_SceneObj;

        Layer_CameraDetectable = 1 << LayerMask_Terrain | LayerMask_Floatable;
    }

    
    /// <summary>
    /// ボタンの長押し時間を取得
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public float GetLongPressTime(E_ActionButton button)
    {
        switch (button)
        {
            case E_ActionButton.MouseLeftButton:
                return 1f;
            case E_ActionButton.F:
                return 0.4f;
            case E_ActionButton.R:
                return 0.7f;
            case E_ActionButton.Space:
                return 0.3f;
            default:
                return 1f;
        }
    }

}

public enum E_GameStatus
{
    None = 0,
    Title = 1,
    Game = 2,
    Menu  = 3,
    End = 4,
}

public enum E_InputMode
{
    None,
    Title,
    Menu,
    Game,
    End,
}

public enum E_UIType
{
    None,
    Main,

    Title,
    Menu,
    GameClear,
    Retry,

    Crosshair,
    ChargeBar,
    HUD,
}

public enum E_CrosshairPart
{
    LB_Text,
    F_Text,
}

public enum E_ZipperAIState
{
    None=-1,
    Init=0,
    Idle=1,
    GetNextPosition=2,
    Move2Position =3,
}

public enum E_ElectricTurretAIState
{
    None=-1,
    Idle,
    Aim,
    Preparation,
    Attack,
}

public enum E_ActionButton
{
    None=-1,
    MouseLeftButton=0,

    F,
    R,
    Space, 
}


public enum E_FloatCellType
{
    None=-1,
    Player = 1,
    Bullet = 2,
    SceneObj = 3,
} 

public enum E_Axis
{
    XAxis,
    YAxis,
    ZAxis,
}

public enum E_SoundType
{
    None=-1,
    //===========Fx Effect
    Menu = 0,
    Item = 1,
    FloatShot = 2,
    AttractShot = 3,

    GameClear = 4,
    GameOver = 5,

    //==========BGM
    MenuBgm = 6,
    GameBgm = 7,

}