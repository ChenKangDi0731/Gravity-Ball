//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// プレイヤージャンプのフレームイベント
/// ‐アニメーションが再生している時、特定のフレームでイベント関数を呼び出す
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBehaviour_PlayerJump : StateMachineBehaviour
{
    PlayerController player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject go = animator.gameObject;
        if (go.transform.parent != null)
        {
            player = go.transform.parent.GetComponent<PlayerController>();

        }

        if(player != null)
        {
            player.SetCanJumpState(false);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


        if (player != null)
        {
            player.ActiveGroundDetect(true);
        }
    }
}
