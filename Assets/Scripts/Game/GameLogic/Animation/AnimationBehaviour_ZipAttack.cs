//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 敵アニメーションのフレームイベント
/// ‐アニメーションが再生している時、特定のフレームでイベント関数を呼び出す
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBehaviour_ZipAttack : StateMachineBehaviour
{

    Enemy_ZipperMouth enemy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject go = animator.gameObject;
        if (go.transform.parent != null)
        {
            enemy = go.transform.parent.GetComponent<Enemy_ZipperMouth>();

        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemy != null)
        {
            enemy.AttackEndCB();
        }
    }
}
