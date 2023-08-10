//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 敵アニメーションのフレームイベント
/// ‐アニメーションが再生している時、特定のフレームでイベント関数を呼び出す
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventMessenger_Zip : MonoBehaviour
{
    Enemy_ZipperMouth enemy;

    // Start is called before the first frame update
    void Start()
    {
        if(enemy==null && transform.parent != null)
        {
            enemy = transform.parent.GetComponent<Enemy_ZipperMouth>();
        }
    }

    /// <summary>
    /// 攻撃イベント（攻撃の当たり判定
    /// </summary>
    public void Attack()
    {
        if (enemy != null)
        {
            enemy.AttackCB();
        }
    }
}
