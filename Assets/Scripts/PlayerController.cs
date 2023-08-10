//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// プレイヤー制御するスクリプト
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour, IFloatObject
{
    static PlayerController instance;
    public static PlayerController Instance
    {
        get { return instance; }
    }

    public GameObject playerModel;//モデルオブジェクト（子オブジェクト
    public List<Renderer> playerMeshList = new List<Renderer>();//モデルメッシュ（表示用

    [Header("デフォルトパラメータ")]
    public bool hideModelAtFirst = false;
    public bool initColliderAtFirst = false;
    public bool initGravityAtFirst = false;
    public bool init = false;
    [Header("zone prefabs")]
    public GameObject lookAtObj;//カメラが注目する位置
    Vector3 lookAtObjDefaultPos = Vector3.zero;

    public Collider c;
    public Rigidbody rb;
    public CameraController cameraInstance;//カメラスクリプト

    //移動パラメータ
    public AnimationCurve speedFactorCurve = new AnimationCurve();//加速度パラメータ（移動スピードがだんだん早くなる
    public float moveSpeed = 5.0f;//最大移動スピード
    public float startMoveTime = 0;
    public float runSpeed = 6.0f;//走る時のスピード
    public float jumpSpeed;
    public float jumpSpeedFactor = 0.9f;//走ってない場合ジャンプの高さを少し下げる

    public bool activeGroundDetect = true;//着地チェック
    public float groundDetectDist = 0.3f;//着地チェックの距離
    Vector3 preMovement;
    Vector3 movement = Vector3.zero;

    //anim param
    [Header("アニメーションパラメータ")]
    public Animator playerAnimator;
    public string idleParamName = "Idle";
    public string idleTypeParamName = "IdleType";
    public string walkParamName = "Walk";
    public string runParamName = "Run";
    public string dirXParamName = "Dir_x";
    public string dirYParamName = "Dir_y";
    public string jumpParamName = "Jump";
    public string jumpingParamName = "Jumping";

    [Header("アニメーション標識")]
    public bool isMove = false;
    public bool isRun = false;
    public bool isJump = false;
    public bool isFloating = false;
    public bool intoFloatZone = false;
    Vector2 dir_v2 = Vector2.zero;

    //入力パラメータ
    public E_ActionButton curLongPressButton = E_ActionButton.None;
    bool longPressCommandDone = false;
    public float showUITime = 0.1f;
    public float longPressTime = 1.0f;
    public float longPressTimePass = 0;

    //プレイヤーが無重力になった時、カメラの位置を変える（遠くなる
    public float enterZoneTriggerCheckTime = 0;
    float enterZoneTimePass = 0;
    bool enterZone;

    [SerializeField] SpecialAnchor_Floating curMovingFloor;


    bool mouseLock = false;//マウス表示かどうか

    private void Awake()
    {
        curMovingFloor = null;

        ShowModel(!hideModelAtFirst);
        ActiveCollider(initColliderAtFirst , false);
        ActiveGravity(initGravityAtFirst);

        instance = this;
        if (playerAnimator == null)
        {
            playerAnimator = gameObject.GetComponentInChildren<Animator>();
        }
        InitAnimator();//アニメーション初期化

        if (lookAtObj != null)
        {
            lookAtObjDefaultPos = lookAtObj.transform.localPosition;
        }
    }

    /// <summary>
    /// アニメーションパラメータの初期化
    /// </summary>
    void InitAnimator()
    {
        if (playerAnimator == null) return;
        playerAnimator.SetBool(idleParamName, true);
        playerAnimator.SetBool(idleTypeParamName, true);
        playerAnimator.SetBool(walkParamName, false);
        playerAnimator.SetBool(runParamName, false);
        playerAnimator.SetFloat(dirXParamName, 0.0f);
        playerAnimator.SetFloat(dirYParamName, 0.0f);
        playerAnimator.SetBool(jumpParamName, false);
        playerAnimator.SetBool(jumpingParamName, false);
    }

    /// <summary>
    /// アニメーションリセット
    /// </summary>
    void ResetAllAnim()
    {
        if (playerAnimator == null) return;
        playerAnimator.SetBool(idleParamName, false);
        playerAnimator.SetBool(idleTypeParamName, true);
        playerAnimator.SetBool(walkParamName, false);
        playerAnimator.SetBool(runParamName, false);
        playerAnimator.SetFloat(dirXParamName, 0.0f);
        playerAnimator.SetFloat(dirYParamName, 0.0f);
        playerAnimator.SetBool(jumpParamName, false);
        playerAnimator.SetBool(jumpingParamName, false);

        isMove = false;
        isRun = false;
        isJump = true;
    }

    // Update is called once per frame
    void Update()
    {

        //enter zone check
        if (enterZone)
        {
            enterZoneTimePass += Time.deltaTime;
            if (enterZoneTimePass >= enterZoneTriggerCheckTime)
            {
                enterZoneTimePass = 0;
                enterZone = false;

                if (isFloating == false)
                {
                    cameraInstance.EnterFloatMode(false);
                }
            }
        }

        //着地チェック
        if (activeGroundDetect && isFloating == false)
        {
            if (CheckGround())
            {
                if (isJump == true)
                {
                    isJump = false;
                    startMoveTime -= 0.6f;
                    playerAnimator.SetBool(idleParamName, !(isRun || isMove));
                    playerAnimator.SetBool(walkParamName, (!isRun && isMove));
                    playerAnimator.SetBool(runParamName, isRun);
                    playerAnimator.SetBool(jumpParamName, false);
                    playerAnimator.SetBool(jumpingParamName, false);
                }
                isJump = false;
            }
            else
            {
                if (isJump == false)
                {
                    isJump = true;
                    playerAnimator.SetBool(jumpParamName, true);
                    playerAnimator.SetBool(jumpingParamName, true);
                    playerAnimator.CrossFade("jump_mid", 0.3f);
                }
            }
        }

        //ゲームオーバー処理（落ちたらゲームオーバーになる
        if (this.gameObject.transform.position.y < -10)
        {
            if (SceneMgr.Instance.curScene != null && SceneMgr.Instance.curScene.CheckSceneClear() == false)
            {

                if (isFloating == false)//無重力状態が終わったあと終了処理
                {
                    transform.position = SceneMgr.Instance.curScene.GetPlayerStartPos();//プレイヤー位置をリセット
                    transform.rotation = SceneMgr.Instance.curScene.GetPlayerStartRot();
                    if (playerModel != null)
                    {
                        playerModel.transform.localPosition = Vector3.zero;
                        playerModel.transform.localRotation = Quaternion.identity;
                    }

                    ActiveGroundDetect(true);

                    ShowModel(false);
                    InputManager.Instance.SetInputMode(E_InputMode.Menu);
                    InputManager.Instance.ShowCursor(true);

                    UIManager.Instance.ShowUI(E_UIType.HUD, false, true);//HUDを隠す
                    UIManager.Instance.ShowUI(E_UIType.Crosshair, false, true);
                    UIManager.Instance.ShowUI(E_UIType.Retry, true, true);//ゲームオーバーメニューを表示

                    FloatingManager.Instance.UndoAll();//ボールを削除

                    //サウンドの再生
                    AudioManager.Instance.StopAllSound();
                    AudioManager.Instance.PlaySound(E_SoundType.GameOver, false);
                }
            }
            else
            {
                transform.position = SceneMgr.Instance.curScene.GetPlayerStartPos();
                transform.rotation = SceneMgr.Instance.curScene.GetPlayerStartRot();
                if (playerModel != null)
                {
                    playerModel.transform.localPosition = Vector3.zero;
                    playerModel.transform.localRotation = Quaternion.identity;
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(enterZone==false && other.tag == GameDefine.Instance.Tag_Zone)
        {

            enterZone = true;
            enterZoneTimePass = 0;

            cameraInstance.EnterFloatMode(true);//無重力ボールに入った時カメラの位置が遠くなる（はっきり周りが見えるため
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(enterZone && other.tag == GameDefine.Instance.Tag_Zone)
        {
            enterZoneTimePass = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //empty
    }

    /// <summary>
    /// 入力処理
    /// </summary>
    /// <param name="deltatime"></param>
    public void PlayerInput(float deltatime)
    {
        PlayerMove();

        MouseInput();
        KeyboardInput();
    }

    /// <summary>
    /// プレイヤー移動処理
    /// </summary>
    void PlayerMove()
    {
        if (isFloating) return;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        movement.x = x;
        movement.z = y;

        if (cameraInstance != null)
        {
            cameraInstance.VectorTransform(ref movement);
        }

        if (movement.sqrMagnitude != 0)
        {
            startMoveTime = Mathf.Clamp01(startMoveTime + Time.deltaTime);
            if (playerModel != null)
            {
                playerModel.transform.forward = Vector3.Lerp(playerModel.transform.forward, movement.normalized, moveSpeed * 6 * Time.deltaTime);
            }
            dir_v2.x = movement.normalized.x;
            dir_v2.y = movement.normalized.z;

            if (isMove == false)
            {
                isMove = true;
                //アニメーション設定
                playerAnimator.SetBool(idleParamName, false);
                playerAnimator.SetBool(runParamName, isRun);
                playerAnimator.SetBool(walkParamName, !isRun);
            }
        }
        else
        {
            if (startMoveTime != 0)
            {
                startMoveTime = Mathf.Clamp01(startMoveTime - Time.deltaTime * 3f);
            }
            if (isMove == true)
            {
                isMove = false;
                //アニメーション設定
                playerAnimator.SetBool(idleParamName, true);
                playerAnimator.SetBool(walkParamName, false);
                playerAnimator.SetBool(runParamName, false);
            }
        }
        float speedFactor = isRun? 1 : speedFactorCurve.Evaluate(startMoveTime);//スピード曲線を利用してスピードを求め出す
        float speed = isRun ? runSpeed : moveSpeed;
        speed = isJump && isRun == false ? speed * jumpSpeedFactor : speed;
        Vector3 scaleMovementV = Vector3.ClampMagnitude(movement * speed, speed);

        //移動床に立ってるとき、床と一緒に移動する
        if (curMovingFloor != null)
        {
            transform.position = transform.position + scaleMovementV * speedFactor * Time.deltaTime + curMovingFloor.GetMovement();
        }
        else
        {
            transform.position = transform.position + scaleMovementV * speedFactor * Time.deltaTime;
        }
        preMovement = movement;
    }

    /// <summary>
    /// マウス入力
    /// </summary>
    void MouseInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && mouseLock == false)//カーソルを表示する
        {
            SetMouseState(true);
            mouseLock = true;
        }
        else if (Input.GetMouseButtonUp(0) && mouseLock == true)
        {
            SetMouseState(false);
            mouseLock = false;
        }

        if (Input.GetMouseButtonDown(0) && cameraInstance.CanCraeteZone())
        {
            FloatingManager.Instance.CreateFloatingZone(false,cameraInstance.GetCurViewHitPoint());
        }
        else if (Input.GetMouseButtonDown(1))//エイムモードに切り替え
        {
            if (cameraInstance != null)
            {
                cameraInstance.EnterAimMode(true);
            }
        }

        if (Input.GetMouseButtonUp(1))//ノーマルモードに戻る
        {
            if (cameraInstance != null)
            {
                cameraInstance.EnterAimMode(false);
            }
        }
    }

    /// <summary>
    /// キーボード入力
    /// </summary>
    void KeyboardInput()
    {

        if (Input.GetKeyDown(KeyCode.LeftShift))//走る
        {
            isRun = true;
            if (isMove)
            {
                playerAnimator.SetBool(walkParamName, false);
                playerAnimator.SetBool(runParamName, true);
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRun = false;
            if (isMove)
            {
                playerAnimator.SetBool(walkParamName, true);
                playerAnimator.SetBool(runParamName, false);
            }
            else
            {
                playerAnimator.SetBool(runParamName, false);
                playerAnimator.SetBool(walkParamName, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))//ボールを削除
        {
            if (curLongPressButton == E_ActionButton.None)
            {
                curLongPressButton = E_ActionButton.R;
                SetLongPressState(E_ActionButton.R);
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))//ボールを削除
        {
            if (curLongPressButton == E_ActionButton.R)
            {
                curLongPressButton = E_ActionButton.None;
                SetLongPressState(E_ActionButton.None);
            }

            FloatingManager.Instance.Undo();
        }

        if (Input.GetKey(KeyCode.R) && curLongPressButton == E_ActionButton.R)//全てのボールを削除（長押し
        {
            if (longPressTimePass >= longPressTime && longPressCommandDone == false)
            {
                longPressCommandDone = true;
                #region undo_all_zone

                FloatingManager.Instance.UndoAll();

                #endregion undo_all_zone
            }
            longPressTimePass += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Space) && curLongPressButton == E_ActionButton.Space)
        {
            if (longPressTimePass >= longPressTime && longPressCommandDone == false)
            {
                ActiveGroundDetect(false);

                longPressCommandDone = true;

                FloatingManager.Instance.PlayerFloating(this,this.gameObject.transform.position);//プレイヤー自体が無重力になる（長押し

            }
            longPressTimePass += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.F) && curLongPressButton == E_ActionButton.F)
        {
            if (longPressTimePass >= longPressTime && longPressCommandDone == false)
            {
                longPressCommandDone = true;

            }
            longPressTimePass += Time.deltaTime;
            UIManager.Instance.UpdateChargeBar(longPressTimePass);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (curLongPressButton == E_ActionButton.None)
            {
                curLongPressButton = E_ActionButton.F;
                SetLongPressState(E_ActionButton.F);
            }
            //チャージバーを表示
            UIManager.Instance.UpdateChargeBar(0f);
            UIManager.Instance.ShowUI(E_UIType.ChargeBar, true);
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            if (cameraInstance.CanCraeteZone())
            {
                if (curLongPressButton == E_ActionButton.F && longPressCommandDone)
                {
                    //重力ボールを生成（時間制限がある　＆　時間制限内無限に使える
                    FloatingManager.Instance.CreateTimerAttractPoint(false, cameraInstance.GetCurViewHitPoint(), cameraInstance.IsTerrain());
                }
                else
                {
                    //重力ボールを生成（一回しか使えない　＆　時間制限がない
                    FloatingManager.Instance.CreateOnceAttractPoint(false, cameraInstance.GetCurViewHitPoint(), cameraInstance.IsTerrain());
                }
            }

            if (curLongPressButton == E_ActionButton.F)
            {
                curLongPressButton = E_ActionButton.None;
                SetLongPressState(E_ActionButton.None);
            }
            //チャージバーを隠す
            UIManager.Instance.ShowUI(E_UIType.ChargeBar, false);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (cameraInstance != null && cameraInstance.curSA != null)
            {
                cameraInstance.curSA.ActiveAnchor(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isFloating == false)//ジャンプ
        {
            if (isJump == false)
            {
                isJump = true;

                playerAnimator.SetBool(jumpParamName,true);
                playerAnimator.SetBool(jumpingParamName, true);
                ActiveGravity(true);
                ActiveGroundDetect(false);
            }

            if (curLongPressButton == E_ActionButton.None)
            {
                curLongPressButton = E_ActionButton.Space;
                SetLongPressState(E_ActionButton.Space);
            }
        }else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (curLongPressButton == E_ActionButton.Space)
            {
                curLongPressButton = E_ActionButton.None;
                SetLongPressState(E_ActionButton.None);
            }
        }
    }

    /// <summary>
    /// 着地チェック
    /// </summary>
    /// <returns></returns>
    bool CheckGround()
    {
        RaycastHit info;
        if (Physics.Raycast(transform.position, Vector3.down, out info, groundDetectDist, GameDefine.Instance.layer_Terrain)) 
        {
            if(info.collider.tag == GameDefine.Instance.Tag_SpecialAnchor)
            {
                SpecialAnchor_Floating floor = info.collider.gameObject.GetComponent<SpecialAnchor_Floating>();
                if (floor != null)
                {
                    curMovingFloor = floor;
                }
            }
            else
            {
                curMovingFloor = null;
            }
            return true;
        }
        else
        {
            curMovingFloor = null;
        }
        return false;
    }

    void ActiveCollider(bool active, bool triggerOnly = false)
    {
        if (c == null) return;
        if (triggerOnly == false)
        {
            c.enabled = active;
        }
        else
        {
            c.isTrigger = active;
        }
    }

    public void ActiveGravity(bool active)
    {
        if (rb == null) return;
        rb.useGravity = active;
    }

    #region other_method

    void StopTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
    }

    void SetMouseState(bool state = false)
    {
        InputManager.Instance.ShowCursor(state);
    }

    /// <summary>
    /// 長押し処理
    /// </summary>
    /// <param name="buttonType"></param>
    void SetLongPressState(E_ActionButton buttonType = E_ActionButton.None)
    {
        longPressCommandDone = false;
        longPressTime = GameDefine.Instance.GetLongPressTime(buttonType);
        longPressTimePass = 0;
    }

    #endregion other_method

    #region external_method

    public void DoInit()
    {
        ShowModel(true);
        if (UIManager.Instance.hud != null)
        {
            UIManager.Instance.hud.ResetHp();
        }
    }

    /// <summary>
    /// カメラ設定
    /// </summary>
    /// <param name="cameraC"></param>
    public void SetCamera(Camera cameraC)
    {
        if (cameraC == null)
        {
            Debug.LogError("[PlayerController]Set Camera failed, CameraController is null");
            return;
        }

        CameraController tempC = cameraC.gameObject.GetComponent<CameraController>();
        if (tempC == null)
        {
            Debug.LogError("[PlayerController]Get CameraController Component failed");
            return;
        }

        if (lookAtObj != null)
        {
            lookAtObj.transform.localPosition = lookAtObjDefaultPos;
        }

        cameraInstance = tempC;
        cameraInstance.SetFollowObj(lookAtObj);
    }

    /// <summary>
    /// カメラスクリプト設定
    /// </summary>
    /// <param name="cameraC"></param>
    public void SetCamera(CameraController cameraC)
    {
        if (cameraC == null)
        {
            Debug.LogError("[PlayerController]Set Camera failed, CameraController is null");
            return;
        }

        if (lookAtObj != null)
        {
            lookAtObj.transform.localPosition = lookAtObjDefaultPos;
        }

        cameraInstance = cameraC;
        cameraInstance.SetFollowObj(lookAtObj);
    }

    /// <summary>
    /// プレイヤーモデルを表示
    /// </summary>
    /// <param name="show"></param>
    public void ShowModel(bool show)
    {
        //モデルのメッシュをオフにする
        if (playerMeshList == null || playerMeshList.Count == 0) return;
        for(int index = 0; index < playerMeshList.Count; index++)
        {
            if (playerMeshList[index] == null) continue;
            playerMeshList[index].enabled = show;
        }

        if (playerModel != null)
        {
            playerModel.SetActive(show);
        }
        if (rb != null)
        {
            rb.useGravity = show;
        }
    }

    public void　SetTransform(Vector3 pos,Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;

        if (playerModel != null)
        {
            playerModel.transform.localPosition = Vector3.zero;
            playerModel.transform.localRotation = Quaternion.identity;
        }
    }

    public void ResetFollowObj()
    {
        if (lookAtObj != null)
        {
            lookAtObj.transform.localPosition = lookAtObjDefaultPos;
        }
    }

    public void ActiveGroundDetect(bool active)
    {
        activeGroundDetect = active;
    }

    public void SetCanJumpState(bool state)
    {
        isJump = !state;
    }

    #endregion external_method

    #region interface_method
    //==============================================================無重力状態処理
    public void FloatingStart(FlowObjCell cell)
    {
        if (cameraInstance != null)
        {
            cameraInstance.EnterFloatMode(true);//無重力状態になる時カメラの位置を変える
        }

        //アニメーション設定
        ResetAllAnim();
        playerAnimator.SetBool(jumpParamName, true);
        playerAnimator.SetBool(jumpingParamName, true);
        playerAnimator.CrossFade("jump_mid", 0.3f);

        ActiveGroundDetect(false);//着地チェックをオフにする
        isJump = true;
        isFloating = true;

        //reset movement param
        startMoveTime = 0;
    }

    public void FloatingHit(FlowObjCell cell)
    {

    }

    public void Floating(FlowObjCell cell)
    {

    }

    public void FloatRelease(FlowObjCell cell)
    {

    }

    public void FloatingEnd(FlowObjCell cell)
    {
        ActiveGroundDetect(true);
        cameraInstance.EnterFloatMode(false);

        isFloating = false;
    }

    #endregion interface_method

    #region animation_event_method

    public void JumpUp()
    {
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    }

    #endregion animation_event_method

    #region other_param

    public bool CheckSA(SpecialAnchor anchor, SpecialAnchor.E_AnchorType anchorType)
    {
        return (anchor != null && anchor.anchorType == anchorType && anchor.IsActive() == false);
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="damage"></param>
    public void GetHit(int damage)
    {
        if (UIManager.Instance.hud != null)
        {
            UIManager.Instance.hud.ChangeHp(damage);

            //今のHPをチェック
            if (UIManager.Instance.hud.IsPlayerDead() == true)
            {
                if (SceneMgr.Instance.curScene != null && SceneMgr.Instance.curScene.CheckSceneClear() == false)
                {

                    if (isFloating == false)//無重力状態が終わったあと終了処理
                    {
                        transform.position = SceneMgr.Instance.curScene.GetPlayerStartPos();//プレイヤー位置をリセット
                        transform.rotation = SceneMgr.Instance.curScene.GetPlayerStartRot();
                        if (playerModel != null)
                        {
                            playerModel.transform.localPosition = Vector3.zero;
                            playerModel.transform.localRotation = Quaternion.identity;
                        }

                        ActiveGroundDetect(true);

                        ShowModel(false);
                        InputManager.Instance.SetInputMode(E_InputMode.Menu);
                        InputManager.Instance.ShowCursor(true);

                        UIManager.Instance.ShowUI(E_UIType.HUD, false, true);//HUDを隠す
                        UIManager.Instance.ShowUI(E_UIType.Crosshair, false, true);
                        UIManager.Instance.ShowUI(E_UIType.Retry, true, true);//ゲームオーバーメニューを表示

                        FloatingManager.Instance.UndoAll();//ボールを削除

                        AudioManager.Instance.StopAllSound();
                        AudioManager.Instance.PlaySound(E_SoundType.GameOver, false);
                    }
                }
            }
        }

        cameraInstance.CameraShake(0.25f,0.1f);

    }


    #endregion other_param

    #region debug_method

    public void OnDrawGizmos()
    {
        if (lookAtObj != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(lookAtObj.transform.position, Vector3.one * 0.2f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundDetectDist);
    }

    #endregion debug_method

}
