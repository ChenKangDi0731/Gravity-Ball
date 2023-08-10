///////////////////////////////////////////////////////////////////
///
/// アイテムの親スクリプト
/// ‐表示処理
/// ‐初期化、終了処理
/// ‐プレイヤーとのインタラクト
/// 
///////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] Collider collider;

    [SerializeField] bool activeWhenStart = true;//ステージ初期化するとき表示するかどうか
    [SerializeField] protected bool defaultItemState = true;//アイテムのデフォルト状態（取れられるかどうか
    protected bool itemState = true;//アイテム状態（取れられるかどうか

    public virtual void DoInit()
    {
        if (activeWhenStart == false)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        itemState = defaultItemState;

        SetState(itemState);

    }

    /// <summary>
    /// 表示処理
    /// </summary>
    /// <param name="state"></param>
    public virtual void Show(bool state)
    {
        this.gameObject.SetActive(state);
    }

    /// <summary>
    /// アイテムの状態を設定（取れられるかどうか）
    /// </summary>
    /// <param name="state"></param>
    public virtual void SetState(bool state)
    {
        itemState = state;
    }

    public void ActiveCollider(bool state)
    {
        if (collider == null)
        {
            collider = GetComponent<Collider>();
        }

        if (collider != null)
        {
            collider.enabled = state;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter(other);
    }

    public abstract void TriggerEnter(Collider other);//当たり判定（プレイヤーに取られるとか
}
