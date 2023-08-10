///////////////////////////////////////////////////////////////////
///
/// 敵の弾
/// 
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour,IFloatObject
{
    public Rigidbody rb;
    public Collider c;

    [Header("スケールアニメーション")]
    public Vector3 defaultScale = Vector3.zero;
    public Vector3 targetScale = Vector3.one;
    public float beCreatedScaleAnimTime = 0.5f;

    public bool activeGravityAtBeginning = false;//生成された時の重力の設定

    Action<Bullet> beCreatedCallback;

    Tweener bornScaleTweener;

    [SerializeField] bool defaultEnemyOwner = true;
    bool playerOwner;//False：敵の弾　True：プレイヤーの弾（無重力になったらプレイヤーが攻撃者になる

    #region lifeCycle_method

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (c == null) c = GetComponent<Collider>();

        playerOwner = !defaultEnemyOwner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == GameDefine.Instance.Tag_Terrain)
        {
            //hit effect
        }
        else
        {
            if (playerOwner == false)
            {
                if (other.tag == GameDefine.Instance.Tag_Player)
                {
                    PlayerController.Instance.GetHit(-10);
                }
            }
            else
            {
                if (other.tag == GameDefine.Instance.Tag_Enemy)
                {
                    Enemy enemy = other.gameObject.GetComponent<Enemy>();
                    if (enemy != null && enemy.isFloating == false)
                    {
                        Vector3 hitDir = other.gameObject.transform.position - transform.position;
                        enemy.GetDamage(-5);
                        enemy.GetHit(hitDir,other.ClosestPoint(this.transform.position));
                    }
                }
            }
        }
    }

    #endregion lifeCycle_method

    void ActiveCollider(bool active)
    {
        if (c == null) return;

        c.enabled = active;
    }

    void ActiveGravity(bool active)
    {
        if (rb == null) return;

        rb.useGravity = active;
    }

    void StopTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
    }

    #region external_method

    public virtual void DoInit()
    {
        ActiveGravity(activeGravityAtBeginning);
    }

    public void BeCreated(Action<Bullet> callback)
    {
        //born anim
        StopTweener(bornScaleTweener);
        transform.localScale = defaultScale;
        bornScaleTweener = transform.DOScale(targetScale, beCreatedScaleAnimTime);
        bornScaleTweener.OnComplete(BeCreatedCB);

        beCreatedCallback = callback;
    }

    public void SetTransform(Vector3 pos,Vector3 dir)
    {
        transform.position = pos;
        transform.forward = dir;
    }

    public virtual void AddForce(Vector3 force,ForceMode mode)
    {
        if (rb == null) return;
        rb.AddForce(force, mode);
    }

    #region interface_method

    public void FloatingStart(FlowObjCell cell)
    {
        DelayDestroy dd = gameObject.GetComponent<DelayDestroy>();
        if (dd != null)
        {
            dd.StopDestroy();
        }

        playerOwner = true;
        rb.velocity = Vector3.zero;
    }

    public void FloatingHit(FlowObjCell cell)
    {
        if (cell != null)
        {
            cell.SetCanFloat(false);
        }
    }

    public void FloatRelease(FlowObjCell cell)
    {
        if (cell != null)
        {
            cell.SetCanFloat(false);
        }
    }

    public void FloatingEnd(FlowObjCell cell)
    {
        DestroyImmediate(this.gameObject);
    }

    public void Floating(FlowObjCell cell)
    {

    }

    #endregion interface_method


    #endregion external_method

    #region callback_method

    public void BeCreatedCB()
    {
        beCreatedCallback?.Invoke(this);
    }

    #endregion callback_method
}
