///////////////////////////////////////////////////////////////////
///
/// 敵のスクリプト
/// 
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class Enemy_ZipperMouth : Enemy,IFloatObject
{
    AnimatorStateInfo curAnimInfo;//アニメーションパラメータ
    GameObject playerObj;//プレイヤー

    [Header("ステータスパラメータ")]
    [SerializeField] int maxHp;//Hp最大値
    [SerializeField] int curHp;//今のHP

    [Header("移動アニメーションパラメータ")]
    [Range(0, 1)] public float idleP;//待機パターンが続く確率（次の行動は移動か待機か
    [Range(0, 1)] public float move2NextPosP;
    public float baseIdleTime = 3.0f;
    public Vector2 idleTimeOffsetRange = Vector2.zero;
    public float curIdleTime = 0;

    [Header("移動パラメータ")]
    public float moveSpeed;//移動スピード
    public float rotateSpeed;//回転スピート
    public float wanderingRadius;//パトロール半径
    public Vector3 startPos;
    public Vector3 targetPos;
    public Vector3 targetForward;
    public Vector3 resetEuler;
    Quaternion resetRotation;

    public float moveTime = 0;
    float moveTime_I;
    float moveProgress;

    float stateTimePass = 0;

    [Header("UI")]
    GameObject uiObj;
    EnemyHpUI hpUIScript;

    [Header("AI パラメータ")]
    public E_EnemyState curState = E_EnemyState.None;//今の行動パターン
    public E_EnemyState preState = E_EnemyState.None;//前の行動パターン
    [Range(0,1)]public float idleContinueP;
    public int sreachPosTimes;//一定時間を経てばパトロールの位置を更新する
    int curSreachPosTimes;//経過時間
    Vector3 tempOffset;
    public float tempOffsetDist = 0.5f;
    public float hitDetectRangeAngle = 90.0f;
    float hitDetectRange;//攻撃の有効範囲（角度
    public float hitDist = 0.7f;//攻撃範囲（距離

    public float updateStateParamInterval = 1.0f;
    float updateStateParamTimePass = 0;

    [Header("狙われてるときのパラメータ")]
    public Color baseColor;
    public Color beDetectedColor;
    bool beDetected = false;
    public float resetColorTime = 1f;
    public float resetColorTimePass = 0;

    Action<Enemy> beCreatedCallback;

    bool foundPlayer = false;//プレイヤーを見つけたかどうか

    // Start is called before the first frame update
    public override void OnStart()
    {
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

        playerObj = GameConfig.Instance.playerObj;//プレイヤーオブジェクトを取得

        if (animator == null)
        {
            animator = GetComponent<Animator>();//アニメーターコンポーネントを取得
        }
        //行動パターンを切り替えるに使う確率のパラメータ
        float totalP = idleP + move2NextPosP;
        if (totalP != 0)
        {
            idleP = idleP / totalP;
            move2NextPosP = 1 - idleP;
        }
        UpdateState(E_EnemyState.Idle);//初期行動パターンを設定（待機状態

        hitDetectRange = Mathf.Cos(hitDetectRangeAngle * Mathf.Deg2Rad);
    }

    public override void OnDestroyS()
    {
        this.gameObject.SetActive(false);
        if (uiObj != null)
        {
            Destroy(uiObj);
        }
    }

    // Update is called once per frame
    public override void DoUpdate()
    {
        UpdateCurState();//行動パターンを更新

        //体勢を戻す
        if (resetTrans)
        {
            if (Vector3.Dot(transform.eulerAngles.normalized, resetEuler.normalized) <= 0.95)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, resetRotation, 90 * Time.deltaTime);
            }
            else
            {
                resetTrans = false;
                transform.eulerAngles = resetEuler;

                ResetState();
            }
        }

        //プレイヤーに狙われた時、アウトライン効果をオンにする
        if (beDetected)
        {
            resetColorTimePass += Time.deltaTime;
            if (resetColorTimePass >= resetColorTime)
            {
                beDetected = false;
                resetColorTimePass = 0;
                SetOutline(false, baseColor);
            }
        }

        if (this.transform.position.y <= -5.0f)
        {
            this.transform.position = new Vector3(0.0f, 0.2f, 0.0f);
        }
    }

    public override void DoFixedUpdate()
    {

        if (updateStateParamInterval <= updateStateParamTimePass)
        {
            //プレイヤーへ移動する
            if (curState == E_EnemyState.Move2Player)
            {
                if (UnityEngine.Random.value > 0.8f)
                {
                    UpdateMove2PlayerParam();
                }
                else
                {
                    UpdateState(E_EnemyState.Idle);
                }
            }
            updateStateParamTimePass = 0;
        }
        else
        {
            updateStateParamTimePass += Time.fixedDeltaTime;
        }

        //投げ出されたら、地面にいるかどうかをチェックする。もし地面に着いたら速度をリセット
        if(isHit == false && isFloating == false && CheckGround())
        {
            ResetVelocity(true, true);
        }
    }

    public override void OnCollision(Collision collision)
    {
        if (isFloating == false) return;
        if (collision.gameObject.tag == "Enemy")
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null && enemy.isFloating == false)
            {
                Vector3 hitDir = collision.gameObject.transform.position - transform.position;
                enemy.GetDamage(-5);
                enemy.GetHit(hitDir,collision.GetContact(0).point);

                this.GetDamage(-5);
                //this.UpdateState(E_EnemyState.Damage);
                this.GetHit(-hitDir, this.transform.position);
            }
        }
    }

    public override void GetDamage(int damage)
    {
        if (isHit) return;
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

    public override void GetHit(Vector3 hitDir,Vector3 hitPos)
    {
        hitDir = Vector3.ClampMagnitude(hitDir, GameDefine.Instance.maxHitForce);
        hitDir.y = Mathf.Abs(hitDir.y);
        AddForce(hitDir * 7.0f, ForceMode.Impulse);
        UpdateState(E_EnemyState.Damage);

        base.GetHit(hitDir, hitPos);
    }

    #region ai_method

    void UpdateMoveStateParam()
    {
        updateStateParamTimePass = 0;

        startPos = transform.position;
        Vector3 start2TargetPos = targetPos - startPos;
        moveTime = (moveSpeed <= 0) ? 1 : start2TargetPos.magnitude / moveSpeed;
        moveTime_I = 1.0f / moveTime;
        moveProgress = 0;
    }

    void UpdateMove2PlayerParam(bool firstTime = false)
    {
        if (playerObj != null && canMove)
        {

            PlayAnim(walkAnimName);
            Vector3 newPos = playerObj.transform.position;
            if (firstTime)
            {
                tempOffset = UnityEngine.Random.insideUnitSphere;
                tempOffset.y = 0;
                tempOffset = Vector3.ClampMagnitude(tempOffset, tempOffsetDist);
            }
            else
            {
                //Debug.Log("update = " + gameObject.name);
            }
            newPos += tempOffset;

            MoveTo(newPos);
            if (CheckHasPath() == false)
            {
                UpdateState(E_EnemyState.Idle);
            }
        }
        else
        {
            UpdateState(E_EnemyState.Idle);
        }

        updateStateParamTimePass = 0;
    }

    /// <summary>
    /// アニメーションを切り替え
    /// </summary>
    /// <param name="animName"></param>
    /// <param name="crossFade"></param>
    void PlayAnim(string animName, float crossFade = 0.5f)
    {
        if (animator == null)
        {
            Debug.LogError("[Test_SimpleEnemy1]Play anim failed, animator is null");
            return;
        }

        animator.CrossFade(animName, crossFade,0);
    }

    /// <summary>
    /// 行動パターンを切り替え
    /// </summary>
    /// <param name="targetState"></param>
    void UpdateState(E_EnemyState targetState)
    {
        switch (targetState)
        {
            case E_EnemyState.Idle://待機状態
                //待機時間を設定
                idleState = true;
                curIdleTime = baseIdleTime + UnityEngine.Random.Range(idleTimeOffsetRange.x, idleTimeOffsetRange.y);
                stateTimePass = 0;
                //向いてる方向を設定
                float rotateEuler = UnityEngine.Random.Range(0, 360);
                targetForward = Quaternion.Euler(0, rotateEuler, 0) * transform.forward;

                PlayAnim(idleAnimName);//待機アニメーション

                break;
            case E_EnemyState.Move://移動
                idleState = false;

                curSreachPosTimes = 0;
                //目標位置を設定
                do
                {
                    curSreachPosTimes++;
                    targetPos = BehaviourUtility.GetRandomPos(transform.position, wanderingRadius);

                } while (curSreachPosTimes < sreachPosTimes && MoveTo(targetPos) == false);
                //目標位置がない場合は待機パターンに移行
                if (curSreachPosTimes >= sreachPosTimes)
                {
                    UpdateState(E_EnemyState.Idle); 
                }

                targetPos.y = transform.position.y;

                UpdateMoveStateParam();

                PlayAnim(walkAnimName);//移動アニメション

                break;
            case E_EnemyState.Move2Player://プレイヤーへ移動
                idleState = false;

                UpdateMove2PlayerParam(true);
                break;
            case E_EnemyState.Attack://攻撃
                idleState = false;
                PlayAnim(attackAnimName);

                break;
            case E_EnemyState.Damage://ダメージを受ける
                isHit = true;
                hitRecoveryTimePass = 0.0f;
                break;
            case E_EnemyState.None:
            default:
                UpdateState(E_EnemyState.Idle);
                break;
        }

        curState = targetState;
        //Debug.LogError("go { "+gameObject.name+" } change state " + curState);
    }
    /// <summary>
    /// 行動パターンのアップデート
    /// </summary>
    void UpdateCurState()
    {
        //switch state
        float p = UnityEngine.Random.value;//行動パターンを切り替える確率

        if (foundPlayer == false)
        {
            //sreach player
            foundPlayer = true;
        }

        switch (curState)
        {
            case E_EnemyState.Idle://待機状態
                if (canMove)
                {
                    if (idleState)//idle
                    {
                        //sreach player

                        stateTimePass += Time.deltaTime;
                        if (stateTimePass >= curIdleTime)
                        {
                            //プレイヤーを発見しない場合
                            //次のステートをランダムで決める（待機　OR　パトロール
                            if (p < idleContinueP && foundPlayer == false)//待機状態
                            {
                                curIdleTime = baseIdleTime + UnityEngine.Random.Range(idleTimeOffsetRange.x, idleTimeOffsetRange.y);
                                stateTimePass = 0;

                                PlayAnim(idleAnimName);
                            }
                            else // 目標位置へ移動する
                            {
                                //プレイヤーを見つけたら、プレイヤーへ移動する
                                if (foundPlayer)
                                {
                                    UpdateState(E_EnemyState.Move2Player);
                                }
                                else
                                {
                                    UpdateState(E_EnemyState.Move);
                                }
                            }
                        }
                        else
                        {
                            //回転する
                            if (Vector3.Dot(transform.forward, targetForward) < 0.98f)
                            {
                                transform.forward = Vector3.Lerp(transform.forward, targetForward, rotateSpeed * Time.deltaTime);
                            }
                        }
                    }
                }
                break;
            case E_EnemyState.Move://移動

                if (canMove)
                {

                    if (CheckMoveState() == false)
                    {
                        if (foundPlayer == false)
                        {
                            UpdateState(E_EnemyState.Idle);
                        }
                        else
                        {
                            //attack
                            UpdateState(E_EnemyState.Move2Player);
                        }
                    }
                }
                break;
            case E_EnemyState.Move2Player://プレイヤーへ移動する
                if (canMove)
                {
                    if (CheckMoveState() == false)
                    {
                        //attack
                        UpdateState(E_EnemyState.Attack);
                    }
                }
                break;
            case E_EnemyState.Attack:

                break;
            case E_EnemyState.Damage://ダメージを受ける
                hitRecoveryTimePass += Time.deltaTime;
                if(hitRecoveryTimePass > hitRecoveryTime)
                {
                    isHit = false;
                    UpdateState(E_EnemyState.Idle);
                }
                break;
            case E_EnemyState.None:
            default:
                UpdateState(E_EnemyState.Idle);
                break;
        }
    }

    void ResetState()
    {
        UpdateState(E_EnemyState.Idle);
        //enable navmesh
        ActiveNavmesh(true);
        SetMoveState(true);
    }

    /// <summary>
    ///アウトライン効果を設定
    /// </summary>
    /// <param name="active"></param>
    /// <param name="color"></param>
    void SetOutline(bool active, Color color)
    {
        float outline = active == false ? 0.01f : 0.5f;
        this.gameObject.MapAllComponent<Renderer>(r =>
        {
            if (r != null)
            {
                if (r.materials != null && r.materials.Length != 0)
                {
                    for (int index = 0; index < r.materials.Length; index++)
                    {
                        if (r.materials[index] == null) continue;

                        int outlineColorPropertyId = Shader.PropertyToID("_OutlineColor");
                        int outlinePropertyId = Shader.PropertyToID("_Outline");
                        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                        propertyBlock.SetFloat(outlinePropertyId, outline);
                        propertyBlock.SetColor(outlineColorPropertyId, color * 3);
                        r.SetPropertyBlock(propertyBlock);
                    }
                }
            }
        });
    }

    void SetRigidbodyConstraints(bool active, RigidbodyConstraints contraints)
    {
        if (rb == null)
        {
            return;
        }

        contraints = active ? contraints : ~contraints;

        rb.constraints &= contraints;
    }

    /// <summary>
    /// 地面にいるかどうかをチェックする
    /// </summary>
    /// <returns></returns>
    bool CheckGround()
    {
        RaycastHit info;
        if(Physics.Raycast(transform.position,Vector3.down,out info, 0.5f, GameDefine.Instance.layer_Terrain))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 移動状態を設定
    /// </summary>
    /// <param name="state"></param>
    /// <param name="resetPath"></param>
    public override void SetMoveState(bool state, bool resetPath = false)
    {
        if (agent == null || agent.enabled == false) return;//ナビゲーションコンポーネントがないなら移動不可能

        if (state)
        {
            if (resetPath && (MoveTo(targetPos) == false || CheckHasPath()==false))
            {
                UpdateState(E_EnemyState.Idle);
                return;
            }
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
        }
    }

    #endregion ai_method

    #region floating_method
    /// <summary>
    /// 生成される
    /// </summary>
    /// <param name="callback"></param>
    public override void BeCreated(Action<Enemy> callback)
    {
        StopTweener(bornScaleTweener);
        transform.localScale = defaultScale;
        bornScaleTweener = transform.DOScale(targetScale, beCreatedScaleAnimTime);
        bornScaleTweener.OnComplete(BeCreatedCB);

        beCreatedCallback = callback;
    }

    /// <summary>
    /// 無重力になる
    /// </summary>
    /// <param name="cell"></param>
    public void FloatingStart(FlowObjCell cell)
    {
        isFloating = true;

        //待機アニメーションに切り替える
        PlayAnim(idleAnimName);

        //スピードをリセット
        ResetVelocity(true, true);

        //Navmeshを無効にする
        SetMoveState(false);
        ActiveNavmesh(false);

    }

    public void FloatingHit(FlowObjCell cell)
    {
        this.GetDamage(-10);
    }

    public void FloatRelease(FlowObjCell cell)
    {

    }

    public void FloatingEnd(FlowObjCell cell)
    {
        isFloating = false;
        resetTrans = true;

        //回転リセット
        resetEuler = transform.eulerAngles;
        resetEuler.x = resetEuler.z = 0;
        resetRotation = Quaternion.Euler(resetEuler);

        //スピードを0にする
        rb.velocity = rb.angularVelocity = Vector3.zero;


    }

    public void Floating(FlowObjCell cell)
    {
        
    }

    #endregion floating_method

    #region callback_method

    public void DetectEnterCB()
    {
        if (beDetected == false)
        {
            beDetected = true;
            SetOutline(true, beDetectedColor);
        }
        resetColorTimePass = 0;

    }

    public void DetectExitCB()
    {
        if (beDetected)
        {
            SetOutline(true, baseColor);
        }
        beDetected = false;

    }

    void BeCreatedCB()
    {
        beCreatedCallback?.Invoke(this);
    }

    public void AttackCB()
    {
        //当たり判定
        if (playerObj == null) return;
        Vector3 v2Player = playerObj.transform.position - transform.position;
        if (Vector3.Dot(v2Player.normalized, transform.forward) > hitDetectRange && v2Player.magnitude < hitDist)
        {
            //hit
            PlayerController.Instance.GetHit(-10);
        }
    }
    /// <summary>
    /// 攻撃後のコールバック関数
    /// </summary>
    public void AttackEndCB()
    {
        UpdateState(E_EnemyState.Idle);
    }

    #endregion callback_method
}

/// <summary>
/// 敵の行動パターン
/// </summary>
public enum E_EnemyState
{
    None,
    Idle,//待機状態
    Move,//移動状態
    Attack,//攻撃
    Damage,//ダメージを受ける

    Move2Player,//プレイヤーへ移動

    ResetTransform,//体勢を戻す
}