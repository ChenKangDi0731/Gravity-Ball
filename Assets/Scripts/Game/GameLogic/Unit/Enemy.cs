/////////////////////////////////////////////////////////////////////////////
///
/// 敵の親クラス
/// 
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    //アニメーションの名前
    [Header("Anim Param")]
    public string idleAnimName = "Idle";
    public string walkAnimName = "Run";
    public string attackAnimName = "Attack";

    //コンポーネント
    [Header("Component")]
    public Animator animator;
    public Collider c;
    public Rigidbody rb;
    public NavMeshAgent agent;

    [Header("Be Created Anim Param")]
    //生成されるときだんだん大きくなっていくアニメーション
    public Vector3 defaultScale = Vector3.zero;//デフォルトスケール（サイズ
    public Vector3 targetScale = Vector3.one;//生成されるとき実際なスケール（サイズ
    public float beCreatedScaleAnimTime = 0.5f;//アニメーションの長さ
    Vector3 bornForceDir = Vector3.zero;

    //TODO:hit recovery
    [Header("Be hited")]
    [SerializeField] protected float hitForce = 7.0f;
    [SerializeField] protected float hitRecoveryTime = 3.0f;
    protected float hitRecoveryTimePass = 0.0f;
    
    [Header("State param")]
    public bool isFloating = false;//無重力になってるか
    public bool idleState = false;//待機状態か
    public bool resetTrans = false;//投げ出された後、体勢を戻す
    public bool isHit = false;//ダメージを受けてるか

    [SerializeField] GameObject hitEffectPrefab;//ダメージを受ける時のエフェクト


    public bool canMove//移動できるか（無重力になったら移動不可能
    {
        get
        {
            return isFloating == false && isHit == false && resetTrans == false;
        }
    }

    protected Tweener bornScaleTweener;//スケール動画パラメータ

    #region lifeCycle_method

    // Start is called before the first frame update
    void Start()
    {
        if (agent == null)
        {
            agent = gameObject.GetComponent<NavMeshAgent>();//ナビゲーションコンポーネントを取得
        }
        string name = gameObject.name;
        int id = gameObject.GetInstanceID();
        OnStart();//初期化
    }

    // Update is called once per frame
    void Update()
    {
        DoUpdate();
    }

    void FixedUpdate()
    {
        DoFixedUpdate();
    }

    public void OnCollisionEnter(Collision collision)
    {
        OnCollision(collision);
    }

    public void OnDestroy()
    {
        OnDestroyS();
    }

    #endregion lifeCycle_method
    /// <summary>
    /// アニメーションを停止
    /// </summary>
    /// <param name="t"></param>
    protected virtual void StopTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
    }

    #region external_method
    /// <summary>
    /// 移動関数
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public void SetTransform(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        transform.forward = dir;
    }

    #region AI_method
    /// <summary>
    /// 目標へ移動する
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public virtual bool MoveTo(Vector3 pos)
    {
        if (agent == null) return false;

        return agent.SetDestination(pos);
    }

    public virtual void SetMoveState(bool state,bool resetPath=false)
    {
        if (agent == null) return;
        if (state)
        {
            agent.isStopped = false;

        }
        else
        {
            agent.isStopped = true;
        }
    }
    /// <summary>
    /// 目標との距離が一定距離以下になったら、移動を停止
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckMoveState()
    {
        if (agent == null && agent.hasPath) return false;
        return agent.remainingDistance > 0.6f;
    }
    /// <summary>
    /// 目標に移動できるかどうか
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckHasPath()
    {
        if (agent == null) return false;
        return agent.hasPath;
    }
    /// <summary>
    /// ナビゲーションを使用
    /// </summary>
    /// <param name="active"></param>
    public void ActiveNavmesh(bool active)
    {
        if (agent == null) return;

        agent.enabled = active;
    }

    #endregion AI_method
    /// <summary>
    /// 移動速度をリセット
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="angularVelocity"></param>
    public virtual void ResetVelocity(bool velocity, bool angularVelocity)
    {
        if (rb == null) return;

        if (velocity)
        {
            rb.velocity = Vector3.zero;
        }

        if (angularVelocity)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
    /// <summary>
    /// 当たり判定を使用
    /// </summary>
    /// <param name="active"></param>
    public virtual void ActiveCollider(bool active)
    {
        if (c == null) return;
        c.enabled = active;
    }
    /// <summary>
    /// 重力の影響を受ける
    /// </summary>
    /// <param name="active"></param>
    public virtual void ActiveGravity(bool active)
    {
        if (rb == null) return;
        rb.useGravity = active;
    }
    
    public virtual void AddForce(Vector3 force, ForceMode mode)
    {
        if (rb == null) return;
        rb.AddForce(force, mode);
    }

    public virtual void GetHit(Vector3 hitDir,Vector3 hitPos)
    {
        //エフェクト生成
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = GameObject.Instantiate(hitEffectPrefab);
            if (hitEffect != null)
            {
                hitEffect.transform.position = hitPos;

                hitEffect.MapAllComponent<ParticleSystem>((ps) =>
                {
                    if (ps != null)
                    {
                        ps.Play();
                    }
                });
            }
        }

        AudioManager.Instance.PlaySound(E_SoundType.Item, true);
    }


    #endregion external_method
    public abstract void BeCreated(Action<Enemy> beCreatedCallback);//生成された時呼び出す函数
    public abstract void OnStart();
    public abstract void DoUpdate();
    public abstract void DoFixedUpdate();
    public abstract void OnCollision(Collision collision);
    public abstract void GetDamage(int damage);
    public abstract void OnDestroyS();

}
