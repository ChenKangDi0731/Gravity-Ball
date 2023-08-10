//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 遠距離攻撃の敵のスクリプト
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ElectricTurret_Two : Enemy
{
    public GameObject bulletPrefab;//弾のプリハブ
    public Transform shootPoint;//弾が生成される位置
    public AimingLine aimingLine;//狙い線
    [Header("AI パラメータ")]
    public E_ElectricTurretAIState curState = E_ElectricTurretAIState.None;//今の状態

    public float sreachRadius;//索敵範囲
    public bool ignoreYAxis;//高低差を無視するかどうか（索敵

    //待機状態のパラメータ
    public float baseIdleTime;
    public Vector2 idleTimeRange = Vector2.zero;
    public float idleTime = 0;
    public float idleTimePass = 0;
    //狙いを定める時のパラメータ
    public float baseShootInterval;
    public Vector2 shootIntervalOffsetRange = Vector2.zero;
    public float shootInterval;
    public float shootTimePass;
    //攻撃準備状態のパラメータ
    public float preparationTime;
    public float preparationTimePass;
    //攻撃処理のパラメータ
    public float attackTime;
    public float attackTimePass;

    [Header("Attack param")]
    public Vector3 curAimPoint;//今狙ってる位置
    public float shootForce;//弾のスピード

    public GameObject playerObj;
    public Vector3 shootPointOffset;

    public bool foundPlayer = false;//プレイヤーを見つけたかどうか

    [Header("ステータスパラメータ")]
    [SerializeField] int maxHp;//Hp最大値
    [SerializeField] int curHp;//今のHP

    [Header("UI")]
    GameObject uiObj;
    EnemyHpUI hpUIScript;
    [SerializeField] Vector3 hpUIOffset;

    [SerializeField]UnityEvent destroyEvent;


    #region lifeCycle_method

    #endregion lifeCycle_method

    /// <summary>
    /// プレイヤーを探す（索敵
    /// </summary>
    /// <returns></returns>
    bool SreachPlayer()
    {
        if (playerObj == null) return false;
        Vector3 obj2PlayerV3 = playerObj.transform.position - transform.position;
        obj2PlayerV3.y = ignoreYAxis ? 0 : obj2PlayerV3.y;

        if (obj2PlayerV3.sqrMagnitude > sreachRadius * sreachRadius)//索敵範囲内にいるかどうか
        {
            return false;
        }

        foundPlayer = true;
        return true;
    }

    /// <summary>
    /// ステートを切り替え
    /// </summary>
    /// <param name="targetState"></param>
    void UpdateState(E_ElectricTurretAIState targetState)
    {
        switch (targetState)
        {
            case E_ElectricTurretAIState.Idle://待機状態
                idleTime = baseIdleTime + UnityEngine.Random.Range(idleTimeRange.x, idleTimeRange.y);
                idleTimePass = 0;

                if (aimingLine != null)
                {
                    aimingLine.ActiveLine(false);
                    aimingLine.ActiveAimAnim(false);
                }
                break;
            case E_ElectricTurretAIState.Aim://狙いを定め
                if (aimingLine != null && playerObj!=null)
                {
                    aimingLine.ActiveLine(true);//狙い線を表示

                    //プレイヤーを追跡
                    aimingLine.ActiveAimAnim(true);
                    curAimPoint = playerObj.transform.position + new Vector3(0, 1f, 0);
                    aimingLine.SetAimPoint(curAimPoint, true);
                }

                shootTimePass = 0;
                shootInterval = baseShootInterval + UnityEngine.Random.Range(shootIntervalOffsetRange.x, shootIntervalOffsetRange.y);//攻撃間隔
                break;
            case E_ElectricTurretAIState.Preparation://攻撃準備
                preparationTimePass = 0;
                if (aimingLine != null)
                {
                    aimingLine.SetStopAnimSign();//追跡停止
                }

                break;
            case E_ElectricTurretAIState.Attack://攻撃
                attackTimePass = 0;

                //弾を生成
                if (bulletPrefab != null)
                {
                    GameObject bulletGo = GameObject.Instantiate(bulletPrefab);
                    if (bulletGo != null)
                    {
                        Bullet bullet = bulletGo.GetComponent<Bullet>();
                        if (bullet != null)
                        {
                            bullet.SetTransform(shootPoint.position, shootPoint.forward);
                            bullet.DoInit();
                            bullet.BeCreated(b =>
                            {
                                if (b != null)
                                {
                                    Vector3 shootDir = (curAimPoint - shootPoint.position).normalized;
                                    b.AddForce(shootDir * shootForce, ForceMode.Impulse);
                                }
                            });
                        }
                    }
                }
                break;
            default:
                break;
        }
        curState = targetState;
    }

    /// <summary>
    /// ステートのアップデート
    /// </summary>
    void UpdateCurState()
    {
        switch (curState)
        {
            case E_ElectricTurretAIState.Idle://待機
                idleTimePass += Time.deltaTime;
                if (idleTimePass >= idleTime)
                {
                    if (SreachPlayer())//プレイヤーを見つけた場合、プレイヤーを追跡する
                    {
                        UpdateState(E_ElectricTurretAIState.Aim);
                    }
                    else
                    {
                        idleTime = baseIdleTime + UnityEngine.Random.Range(idleTimeRange.x, idleTimeRange.y);
                        idleTimePass = 0;
                    }
                }
                break;
            case E_ElectricTurretAIState.Aim://狙いを定め
                if (SreachPlayer())
                {
                    shootTimePass += Time.deltaTime;
                    if(shootTimePass>= shootInterval)
                    {
                        UpdateState(E_ElectricTurretAIState.Preparation);//追跡完了、攻撃準備ステートに移行
                    }
                    else
                    {
                        if (aimingLine != null && playerObj != null)
                        {
                            curAimPoint = playerObj.transform.position + new Vector3(0, 1f, 0);
                            aimingLine.SetAimPoint(curAimPoint, true);
                        }
                    }
                }
                else
                {
                    UpdateState(E_ElectricTurretAIState.Preparation);
                }
                break;
            case E_ElectricTurretAIState.Preparation://攻撃準備
                preparationTimePass += Time.deltaTime;
                if (preparationTimePass >= preparationTime)
                {
                    UpdateState(E_ElectricTurretAIState.Attack);
                }
                break;
            case E_ElectricTurretAIState.Attack://攻撃
                attackTimePass += Time.deltaTime;
                if (attackTimePass >= attackTime)
                {
                    UpdateState(E_ElectricTurretAIState.Idle);
                }
                break;
            default:
                break;
        }
    }

    #region external_method

    public override void BeCreated(Action<Enemy> beCreatedCallback)
    {
    }

    public override void OnStart()
    {
        if (shootPoint == null)
        {
            GameObject tempGo = new GameObject("shoot_point");
            tempGo.transform.SetParent(this.transform);
            tempGo.transform.localPosition = new Vector3(0, 1.5f, 0);
            tempGo.transform.forward = this.transform.forward;

            shootPoint = tempGo.transform;
        }

        playerObj = GameConfig.Instance.playerObj;
        if (aimingLine != null)
        {
            aimingLine.DoInit();
            aimingLine.SetBasePoint(shootPoint.position, true);
            aimingLine.ActiveLine(false);
            aimingLine.ActiveAimAnim(false);
        }

        curHp = maxHp;

        //UI初期化
        if (UIManager.Instance.enemyHpPrefabs != null)
        {
            //UIオブジェクトを生成し、UIスクリプトを獲得する
            uiObj = GameObject.Instantiate(UIManager.Instance.enemyHpPrefabs, UIManager.Instance.worldSpaceCanvas.transform);
            if (uiObj != null)
            {
                hpUIScript = uiObj.GetComponent<EnemyHpUI>();
                if (hpUIScript == null)
                {
                    DestroyImmediate(uiObj);//コンポーネント存在しない場合は、オブジェクトを削除する
                }
                else
                {
                    //初期化
                    hpUIScript.SetFollowObj(this.transform);
                    hpUIScript.SetPosOffset(hpUIOffset);
                    if (maxHp == 0)
                    {
                        hpUIScript.SetHpValue(0.0f);
                    }
                    else
                    {
                        hpUIScript.SetHpValue((float)curHp / maxHp);
                    }
                }
            }
        }

        UpdateState(E_ElectricTurretAIState.Idle);
    }

    public override void DoUpdate()
    {
        UpdateCurState();//ステートのアップデート（AI
    }

    public override void DoFixedUpdate()
    {
    }

    public override void OnCollision(Collision collision)
    {
    }

    public override void GetDamage(int damage)
    {
        //Debug.LogError(gameObject.name + ": Get Damage");

        curHp = Mathf.Clamp(curHp + damage, 0, maxHp);
        if (hpUIScript != null)
        {
            hpUIScript.SetHpValue((float)curHp / maxHp);
        }

        if (curHp <= 0)
        {
            OnDestroyS();
        }
    }

    public override void OnDestroyS()
    {
        destroyEvent.Invoke();
        this.gameObject.SetActive(false);
        if (uiObj != null)
        {
            Destroy(uiObj);
        }
    }

    #endregion external_method

}
