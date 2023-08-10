//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 無重力オブジェクトスクリプト
/// 
/// ‐全て無重力になれるオブジェクトにはこのスクリプトを追加する必要がある（プレイヤーも含む
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public interface IFlowObj
{
    void Flow(bool state, Vector3 targetPos,bool isTerrainPoint,int delayRate = 0);
    void Flow_2();
    void RandomTransform();
    void SetCenterPoint(Vector3 point);
}

public interface IFloatObject
{
    void FloatingStart(FlowObjCell cell);//無重力になる時呼び出される（初期化
    void FloatingEnd(FlowObjCell cell);//無重力重力が戻った時に呼び出される
    void FloatingHit(FlowObjCell cell);//投げ出されたとき呼び出される
    void FloatRelease(FlowObjCell cell);//重力状態から解放されるとき呼び出される
    void Floating(FlowObjCell cell);//無重力になる時呼び出される
}

public class FlowObjCell : MonoBehaviour,IFlowObj
{

    #region static_method
    /// <summary>
    /// マスクチェック（オブジェクトフィルター。登録されたマスクをつけたオブジェクトは無重力になれない
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="cellType"></param>
    /// <returns></returns>
    public static bool CheckMask(int mask, E_FloatCellType cellType)
    {
        if (mask <= 0)
        {
            return false;
        }

        return (mask & 1 << (int)cellType) != 0;
    }

    /// <summary>
    /// オブジェクトのスピードを取得
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    public static Vector3 GetVelocity(Rigidbody rb)
    {
        if (rb == null)
        {
            return Vector3.zero;
        }
        else
        {
            return rb.velocity;
        }
    }

    #endregion static_method

    public FloatingZone parentZone;//無重力になる時所属してるボール
    public Ease floatEaseMode = Ease.Linear;//移動タイプ
    public E_FloatCellType cellType = E_FloatCellType.None;//オブジェクトのタイプ
    public int floatTimes;//複数のポイントに沿って移動するとき使われる（今の移動回数
    public int drawLineTimes;//軌道の描画に使われる

    //軌道線パラメータ
    public GameObject connectLinePrefabs;//軌道オブジェクトのプリハブ
    LineRenderer connectLine;//軌道
    Vector3 lineStartPos;//軌道の始点
    Vector3 lineEndPos;//軌道の終点
    float zLength;

    public bool activeGravityAtBeginning = true;//初期化するとき重力オンにするかどうか
    public bool useDefaultHit = true;
    Rigidbody r;
    Collider c;

    public float followSpeed = 3f;//無重力になる時、空中に浮いていくスピード
    [SerializeField]Vector3 targetPos;
    Vector3 lastPos;

    //移動パラメータ
    public float startFloatHeightOffsetFactor = 1.0f;//無重力になって空中に浮いてる時、位置を少しずれさせ
    public float floatSpeed;//移動スピード
    Vector3 floatEndPos = Vector3.zero;//移動先

    //回転パラメータ
    Vector3 centerPoint;//無重力ボールのセンター位置
    Vector3 obj2CenterPoint;//オブジェクトから無重力ボールまでのベクトル
    public float revolutionSpeed;//自転スピード
    public float rotationSpeed;//無重力ボールを中心に回転するスピード
    Vector3 revolutionDeltaEuler;
    Vector3 recordrevolutionEuler = Vector3.zero;

    Vector3 rotationDeltaEuler;
    Tweener moveTweener;
    Tweener moveTweener2;
    [SerializeField]bool flowStart = false;//無重力になったかどうか
    bool isFloat = false;

    //軌道線アニメーションパラメータ
    public float lineAnimTime_1 = 0.2f;
    public float lineAnimTime_2 = 0.2f;
    public float delayLineAnimTime = 0.1f;
    float lineAnimTimePass = 0;

    bool isSelect = false;//狙われてるかどうか
    Coroutine connectingAnim_Coroutine;//軌道線アニメーション

    public float groundDetectDist = 0.5f;

    public int damage = 3;

    //アウトライン効果パラメータ
    public Color baseColor = Color.white;
    public Color selectColor = Color.blue;
    public Color floatColor = Color.red;
    public float baseOutline;
    public float selectOutline;

    [Header("体勢を戻すパラメータ")]
    public float recoveryTime = 3f;
    public float recoveryTimePass = 0;
    public bool recovery = false;

    public bool canFloat = true;
    public bool delayFloat;
    public Vector3 delayFloatPos;
    bool isCurAttractPointTerrain = false;

    [SerializeField] bool explodeEffect = true;//着地するとき飛ばされるかどうか

    #region lifecycle_method

    // Start is called before the first frame update
    void Start()
    {
        if (r == null)
        {
            r = GetComponent<Rigidbody>();
        }

        if (c == null)
        {
            c = GetComponent<Collider>();
        }

        ActiveGravity(activeGravityAtBeginning);//重力初期化
        ActiveCollider(true);//コライダー初期化

        //軌道線初期化
        if (connectLinePrefabs != null)
        {
            GameObject tempObj = GameObject.Instantiate<GameObject>(connectLinePrefabs);
            if (tempObj == null)
            {
                Debug.LogError("[FlowObjCell]Create ConnectLine failed");
            }
            else
            {
                tempObj.transform.SetParent(this.transform);
                connectLine = tempObj.GetComponentInChildren<LineRenderer>();
                ActiveLine(false, true);
            }
        }

        recovery = false;
        flowStart = false;
        isFloat = false;

        canFloat = true;
        isCurAttractPointTerrain = false;
    }
    public void DoInit()
    {
        isCurAttractPointTerrain = false;
    }
    public void DoUpdate(float deltatime)
    {
        if (flowStart)
        {
            //回転する（無重力シミュレーション
            ResetRigidbodyVelocity(true, false);

            recordrevolutionEuler += revolutionDeltaEuler * deltatime * revolutionSpeed;
            recordrevolutionEuler.x %= 360;
            recordrevolutionEuler.y %= 360;
            recordrevolutionEuler.z %= 360;

            targetPos = centerPoint + Quaternion.Euler(recordrevolutionEuler) * obj2CenterPoint;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * deltatime);
            transform.rotation = transform.rotation * Quaternion.Euler(rotationDeltaEuler * deltatime * rotationSpeed);
        }


    }

    void Update()
    {
        if (recovery)
        {
            recoveryTimePass += Time.deltaTime;
            if (recoveryTimePass >= recoveryTime)
            {
                recovery = false;
                IFloatObject floatObj = GetComponent<IFloatObject>();
                if (floatObj != null)
                {
                    floatObj.FloatingEnd(this);
                }
            }
        }

        if (cellType == E_FloatCellType.SceneObj)
        {
            if (transform.position.y < 0)
            {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain")
        {
            //移動アニメーションを停止
            if (moveTweener != null)
            {
                moveTweener.Kill();
            }
            if (moveTweener2 != null)
            {
                moveTweener2.Kill();
            }
            flowStart = false;
            IFloatObject floatObj = GetComponent<IFloatObject>();
            if (floatObj != null)
            {
                if (useDefaultHit)
                {
                    if (explodeEffect)
                    {
                        Explode();
                    }
                }
                else
                {
                    Hit();
                }
                floatObj.FloatingHit(this);
            }
            else
            {
                if (explodeEffect)
                {
                    Explode();
                }
            }
            ActiveGravity(true);

            Enemy_Diamond enemy = other.gameObject.GetComponent<Enemy_Diamond>();
            if (enemy != null)
            {
                enemy.Damage(damage);
            }
        }
    }

    void Hit()
    {
        recovery = true;
        recoveryTimePass = 0;

        canFloat = true;
        isFloat = false;
        flowStart = false;
        Select(false);
    }
    
    void Explode()
    {
        Vector3 curVelocity = GetVelocity(r);
        curVelocity.x *= -1.0f;
        curVelocity.y += UnityEngine.Random.value * 2.0f;
        curVelocity.z += UnityEngine.Random.Range(-0.5f,-0.5f);

        curVelocity = Vector3.ClampMagnitude(curVelocity, GameDefine.Instance.maxHitForce);

        AddForce(curVelocity, ForceMode.Impulse);

        recovery = true;
        recoveryTimePass = 0;

        canFloat = true;
        isFloat = false;
        flowStart = false;
        Select(false);
    }

    /// <summary>
    /// 移動完了処理
    /// </summary>
    public void ReleaseObj()
    {
        bool isTerrentPoint = false;
        if (parentZone != null)//次の移動先を取得
        {
            Vector3 nextPos;
            if (parentZone.GetNextZonePos(this,floatTimes, out nextPos,out isTerrentPoint))
            {
                isCurAttractPointTerrain = isTerrentPoint;
                Flow(false, nextPos,isCurAttractPointTerrain);
                return;
            }
        }

        parentZone = null;
        ActiveCollider(true);
        ActiveGravity(true);

        canFloat = true;
        flowStart = false;
        isFloat = false;
        Select(false);

        recovery = true;
        recoveryTimePass = 0;

        IFloatObject floatObj = GetComponent<IFloatObject>();
        if (floatObj != null)
        {
            floatObj.FloatRelease(this);//無重力終了処理
            if (isCurAttractPointTerrain == true)
            {
                floatObj.FloatingHit(this);
            }
        }

        if(isCurAttractPointTerrain == true)
        {
            if (explodeEffect)
            {
                Explode();
            }
        }
    }

    #endregion lifecycle_method

    /// <summary>
    /// 軌道線を表示
    /// </summary>
    /// <param name="active"></param>
    /// <param name="reset"></param>
    void ActiveLine(bool active,bool reset = true)
    {
        if (connectLine == null)
        {
            return;
        }

        connectLine.enabled = active;
        if (reset)
        {
            ResetConnectLine();
        }
    }

    /// <summary>
    /// 軌道線の位置を更新
    /// </summary>
    /// <returns></returns>
    bool UpdateLinePos()
    {
        if (parentZone == null) return false;
        bool tempTerrainPointSign = false;
        Vector3 nextPos;
        if (parentZone.GetNextZonePos(this,drawLineTimes, out nextPos,out tempTerrainPointSign))
        {
            lineStartPos = lineEndPos;
            lineEndPos = nextPos;
            drawLineTimes++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// アウトライン効果を設定
    /// </summary>
    /// <param name="active"></param>
    /// <param name="color"></param>
    void SetOutline(bool active,Color color)
    {
        float outline = active ==false ? baseOutline : selectOutline;
        this.gameObject.MapAllComponent<Renderer>(r =>
        {
            if (r != null)
            {
                if (r.materials != null && r.materials.Length != 0)
                {
                    for (int index = 0; index < r.materials.Length; index++)
                    {
                        if (r.materials[index] == null) continue;
                        //r.materials[index].color = color;
                        int outlineColorPropertyId = Shader.PropertyToID("_OutlineColor");
                        int outlinePropertyId = Shader.PropertyToID("_Outline");
                        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                        propertyBlock.SetFloat(outlinePropertyId, outline);
                        propertyBlock.SetColor(outlineColorPropertyId, color * 3);
                        r.SetPropertyBlock(propertyBlock);
                        //r.material.SetColor("_Color", color);
                    }
                }
            }
        });
    }

    void ResetConnectLine()
    {
        if (connectLine == null) return;
        connectLine.SetPosition(0, Vector3.zero);
        connectLine.SetPosition(1, Vector3.zero);
    }

    /// <summary>
    /// 軌道線の位置を設定
    /// </summary>
    /// <param name="index"></param>
    /// <param name="pos"></param>
    void SetLinePos(int index,Vector3 pos)
    {
        if (connectLine == null) return;
        if (index > connectLine.positionCount - 1)
        {
            Debug.LogError("[FlowObjCell]Wrong line index,index = " + index);
            return;
        }

        connectLine.SetPosition(index, pos);
    }

    void ResetCoroutine(Coroutine c,bool resetAll=false)
    {
        if (resetAll == false)
        {
            if (c == null) return;
            StopCoroutine(c);
        }
        else
        {

        }
    }

    #region external_method

    public void AddForce(Vector3 forceDir,ForceMode mode = ForceMode.Impulse)
    {
        if (r == null) return;
        r.AddForce(forceDir, mode);
    }

    /// <summary>
    /// 今所属してる無重力ボールを設定
    /// </summary>
    /// <param name="zone"></param>
    public void SetParentZone(FloatingZone zone)
    {
        if (zone == null)
        {
            Debug.LogError("[FlowObjCell]Set Parent zone failed , zone is null");
            return;
        }

        if (parentZone != null && parentZone != zone)
        {
            parentZone.ReleaseObj(this);
        }

        parentZone = zone;
    }

    /// <summary>
    /// 無重力状態処理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="pos"></param>
    /// <param name="isTerrainPoint"></param>
    /// <param name="delayRate"></param>
    public void Flow(bool state,Vector3 pos, bool isTerrainPoint = false,int delayRate = 0)
    {
        canFloat = false;
        recovery = false;
        //無重力状態になる
        if (state)
        {
            if (r != null)
            {
                r.velocity = Vector3.zero;
            }
            //アニメーションをリセット
            StopTweener(moveTweener);
            StopTweener(moveTweener2);

            ActiveGravity(false);//重力オフ
            ActiveCollider(false);//当たり判定オフ
            ResetRigidbodyVelocity(true, true);//スピードをリセット

            //宙に浮く処理（無重力
            UnityEngine.Random.InitState(gameObject.GetInstanceID()+ (int)(gameObject.transform.position.sqrMagnitude * 1000f));
            revolutionDeltaEuler.x = 0;
            revolutionDeltaEuler.y = UnityEngine.Random.Range(-10, 10);
            int sign = UnityEngine.Random.value >= 0.5f ? -1 : 1;
            revolutionDeltaEuler *= (UnityEngine.Random.value * 0.5f) * sign;

            rotationDeltaEuler = UnityEngine.Random.insideUnitSphere * 10;
            if (parentZone != null)
            {
                Vector3 dir2CenterPoint = transform.position - centerPoint;
                dir2CenterPoint.y = centerPoint.y + UnityEngine.Random.Range(parentZone.detectRadiu * 0.2f, parentZone.detectRadiu * 0.5f) * startFloatHeightOffsetFactor;
                if (dir2CenterPoint.sqrMagnitude >= parentZone.detectRadiu * parentZone.detectRadiu * 0.8f){
                    dir2CenterPoint = Vector3.ClampMagnitude(dir2CenterPoint, parentZone.detectRadiu) * 0.5f;
                }
                else
                {
                    dir2CenterPoint = Vector3.ClampMagnitude(dir2CenterPoint * UnityEngine.Random.Range(0.5f, 1.2f), parentZone.detectRadiu);
                }
                targetPos = centerPoint + dir2CenterPoint;
            }
            else
            {
                targetPos = transform.position + new Vector3(UnityEngine.Random.Range(-0.3f,0.3f), UnityEngine.Random.Range(0.5f,1.2f), UnityEngine.Random.Range(-0.3f, 0.3f));
            }
            moveTweener = transform.DOMove(targetPos, 0.8f, false).SetDelay(0.005f * delayRate);
            moveTweener.OnComplete(Flow_2);
            moveTweener.SetEase(Ease.OutCirc);

            flowStart = false;
            isFloat = true;
            floatTimes = 0;
            drawLineTimes = 0;

            Select(true);//アウトライン効果
        }
        else//重力ポイントに移動する
        {
            if (isFloat)
            {
                delayFloat = true;
                delayFloatPos = pos;
            }
            else
            {
                //移動アニメーションをリセット
                StopTweener(moveTweener);
                StopTweener(moveTweener2);

                flowStart = false;
                floatTimes++;
                drawLineTimes++;

                floatEndPos = pos;

                if (floatTimes > 1)
                {
                    Float_1();//二回目以降の移動（二番目以降の重力ポイントへ移動する処理
                }
                else
                {
                    isCurAttractPointTerrain = isTerrainPoint;
                    //一番最初に移動するとき、まず軌道を表示してから移動し始める。
                    if (drawLineTimes == 1)
                    {
                        lineStartPos = transform.position;
                        lineEndPos = pos;
                    }
                    floatEndPos = pos;
                    ResetCoroutine(connectingAnim_Coroutine);
                    connectingAnim_Coroutine = StartCoroutine(ConnectLineAnimation());//軌道線アニメーション
                    ActiveGravity(false);//移動中に重力オフ
                }
            }
        }
    }

    /// <summary>
    /// 移動先への移動処理
    /// </summary>
    public void Float_1()
    {
        flowStart = false;
        Vector3 offset = (targetPos - centerPoint) * -0.05f;
        SetCenterPoint(floatEndPos);
        float delayScale = (floatTimes > 1) ? 0 : 1;
        moveTweener2 = transform.DOMove(floatEndPos + offset, floatSpeed, false).SetDelay(0.05f * UnityEngine.Random.value * 5.0f * delayScale).SetEase(floatEaseMode);
        moveTweener2.OnComplete(ReleaseObj);
        ActiveCollider(true);//collision detect
    }
    

    public void Flow_2()
    {
        recordrevolutionEuler = Vector3.zero;
        obj2CenterPoint = transform.position - centerPoint;

        flowStart = true;

        isFloat = false;
        if (delayFloat)
        {
            delayFloat = false;
            Flow(false, delayFloatPos);
        }
    }

    public void SetCenterPoint(Vector3 point)
    {
        centerPoint = point;
    }

    /// <summary>
    /// 移動パラメータをリセット
    /// </summary>
    public void RandomTransform()
    {
        UnityEngine.Random.InitState((int)(Time.time + transform.position.x * 10));
        float randomX = UnityEngine.Random.Range(0, 2) - 1;
        float randomZ = UnityEngine.Random.Range(0, 2) - 1;
        transform.position = new Vector3(transform.position.x + randomX, transform.position.y , transform.position.z + randomZ);

        float randomEulerY = UnityEngine.Random.Range(-90, 90);
        transform.rotation *= Quaternion.Euler(new Vector3(0, randomEulerY, 0));
    }

    public void ActiveGravity(bool active)
    {
        if (r == null) return;
        r.useGravity = active;
    }

    public void ResetRigidbodyVelocity(bool resetVelocity,bool resetAngleVelocity)
    {
        if (r == null) return;
        if (resetVelocity)
        {
            r.velocity = Vector3.zero;
        }

        if (resetAngleVelocity)
        {
            r.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// アウトライン効果
    /// </summary>
    /// <param name="state"></param>
    public void Select(bool state)
    {
        if (isFloat == false)
        {
            if (isSelect == state) return;
            isSelect = state;

            if (isSelect)
            {
                SetOutline(true, selectColor);
            }
            else
            {
                SetOutline(false, baseColor);
            }
        }
        else
        {
            SetOutline(true, floatColor);
        }
    }

    public void SetCanFloat(bool state)
    {
        canFloat = state;
    }


    public void ActiveCollider(bool active)
    {
        if (c == null) return;
        c.enabled = active;
    }

    public void ActiveTrigger(bool active)
    {

    }

    public void StopTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
        t = null;
    }

    #endregion external_method

    #region coroutine_method

    /// <summary>
    /// 軌道線アニメーション
    /// </summary>
    /// <returns></returns>
    IEnumerator ConnectLineAnimation()
    {
        ActiveLine(true, true);
        do
        {
            SetLinePos(0, lineStartPos);
            SetLinePos(1, lineStartPos);
            float totalAnimTime = lineAnimTime_1 + lineAnimTime_2 - (lineAnimTime_1 - delayLineAnimTime);
            float lineAnimTime_1_I = lineAnimTime_1 == 0 ? 0 : 1.0f / lineAnimTime_1;
            float lineAnimTime_2_I = lineAnimTime_2 == 0 ? 0 : 1.0f / lineAnimTime_2;

            float timePass = 0;
            float timePass_2 = 0;
            bool startAnim_2 = false;
            while (timePass <= totalAnimTime)
            {
                timePass += Time.deltaTime;

                if (timePass <= lineAnimTime_1)
                {
                    if (timePass >= delayLineAnimTime && startAnim_2 == false)
                    {
                        startAnim_2 = true;
                    }

                    Vector3 value_1 = Vector3.Lerp(lineStartPos, lineEndPos, timePass * lineAnimTime_1_I);
                    SetLinePos(1, value_1);
                }

                if (startAnim_2 && timePass_2 <= lineAnimTime_2)
                {
                    timePass_2 += Time.deltaTime;

                    Vector3 value_2 = Vector3.Lerp(lineStartPos, lineEndPos, timePass_2 * lineAnimTime_2_I);
                    SetLinePos(0, value_2);
                }

                yield return new WaitForEndOfFrame();
            }

        } while (UpdateLinePos() == true);

        ActiveLine(false);

        Float_1();

        yield break;
    }
    #endregion coroutine_method
}
