//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// プレイヤージャンプのフレームイベント
/// ‐アニメーションが再生している時、特定のフレームでイベント関数を呼び出す
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventMessenger_Player : MonoBehaviour
{
    public PlayerController player;


    private void Awake()
    {
        if (player ==null && transform.parent != null)
        {
            player = transform.parent.GetComponent<PlayerController>();
        }
    }

    public void JumpUp()
    {
        if (player != null)
        {
            player.JumpUp();
        }
    }

}
