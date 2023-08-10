///////////////////////////////////////////////////////////////////
///
/// カメラ制御スクリプト
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    GameObject followObj;//親オブジェクト

    [Header("camera follow")]
    public float rotateSpeed;//回転スピード
    public float maxFollowDist;
    float sqrMaxFollowDist;
    public float minFollowDist;
    float sqrMinFollowDist;
    public float maxDist;//親オブジェクトとの最大距離
    public float aimDist;//エイムモードで、カメラが親オブジェクトとの最短距離
    public float floatDist;//プレイヤーが無重力になった時、カメラと親オブジェクトの距離
    public float curDist;//今カメラが親オブジェクトとの距離
    float preDist;//補間動画用パラメータ（前回の距離
    Vector3 defaultDir;//デフォルト向き
    public Vector3 curDir;//今の向き

    //==================================カメラ回転パラメータ（カメラがプレイヤーの肩の所に位置するため、
    //                                                      X軸とY軸を分けて別々処理する）
    public Vector3 xRotateDir;
    public float xRotateDist;
    public Vector3 yRotateDir;
    public float yRotateDist;
    public Vector3 xRotateEuler;
    public Vector3 yRotateEuler;

    public Vector3 curEuler;
    public Vector3 offsetAngle = new Vector3(30, 0, 0);

    #region raycast_param
    //レイキャスティングパラメータ（狙い所のオブジェクトを取得
    public float maxRaycastDist = 10;
    Vector3 curRayHitPosition = Vector3.zero;//狙ってる位置
    Vector3 curHitPointNormal = Vector3.zero;//狙ってるオブジェクトの法線
    [SerializeField]bool isTerrain = false;//狙ってるオブジェクトは地形かどうか
    [SerializeField] bool isForbiddenZone = false;//狙ってるオブジェクトはボール生成できない地形かどうか
    //レイキャスティングのインタバル（毎フレームの呼び出しを避けるため
    float detectInterval = 0.6f;
    float detectTimePass = 0;

    //狙ってるオブジェクトのリスト
    public Collider[] colliders;
    public SpecialAnchor curSA;

    //エイムモードのパラメータ
    public float startAnimDist;
    public float endAnimDist;
    public float baseAimScaleTime;
    public float aimTime;
    public float aimTime_I;
    public float timePass = 0;
    public bool aimMode;
    public bool aimAnimEnd = false;

    //プレイヤーが無重力になった時に使うパラメータ（カメラ）
    public float baseFloatScaleTime;
    public float floatTime;
    public float floatTime_I;
    public float floatTimePass;
    public bool floatMode;
    public bool floatAnimEnd = false;
    #endregion raycast_param

    [Header("debug param")]
    public GameObject detectSphereGo;
    public LineRenderer debugLine;//照準器（デバグ用

    Coroutine cameraShakeC = null;
    Vector3 cameraShakeOffset;

    bool lookAtObjSign = false;
    bool followObjSign = false;

    #region lifeCycle_method

    // Start is called before the first frame update
    void Start()
    {
        defaultDir = curDir = Vector3.back;

        timePass = 0;
        aimMode = false;
        aimAnimEnd = true;
        floatMode = false;
        floatAnimEnd = true;
        curDist = maxDist;
        preDist = curDist;

        lookAtObjSign = false;
        followObjSign = false;
        sqrMaxFollowDist = maxFollowDist * maxFollowDist;
        sqrMinFollowDist = minFollowDist * minFollowDist;

        cameraShakeOffset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //補間アニメーション
        if (aimAnimEnd == false)
        {
            if (timePass < aimTime)
            {
                timePass += Time.deltaTime;
                curDist = MathUtility.Instance.EaseOutQuad(timePass, startAnimDist, endAnimDist, aimTime);
                preDist = curDist;
            }
            else
            {
                aimAnimEnd = true;
                timePass = aimTime;
            }
        }

        //補間アニメーション
        if (floatAnimEnd == false)
        {
            if (floatTimePass < floatTime)
            {
                floatTimePass += Time.deltaTime;
                curDist = MathUtility.Instance.EaseOutQuad(floatTimePass, startAnimDist, endAnimDist, floatTime);
                preDist = curDist;
            }
            else
            {
                floatAnimEnd = true;
                floatTimePass = floatTime;
            }
        }

        SceneObjRaycast(Time.fixedDeltaTime); //当たり判定（狙ってるところの辺りのオブジェクトを取得
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {

#if UNITY_EDITOR
        //if (debugLine != null && followObj != null)
        //{
        //    debugLine.SetPosition(0, transform.position + transform.right * 0.5f);
        //    debugLine.SetPosition(1, curRayHitPosition);
        //}
#endif
        if (debugLine != null)
        {
            debugLine.enabled = false;
        }

    }

    public void CameraLateInput(float deltatime)
    {
        if (followObj != null)
        {

            RaycastHit hitInfo;
            Vector3 dir2Camera = this.gameObject.transform.position - followObj.transform.position;
            if (Physics.Raycast(followObj.transform.position, dir2Camera.normalized, out hitInfo, preDist, GameDefine.Instance.layer_Terrain))
            {
                curDist = hitInfo.distance * 0.95f;
            }
            else
            {
                curDist = preDist;
            }

            //==================================================カメラ移動処理
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

            curEuler.x -= mouseY;
            curEuler.x = Mathf.Clamp(curEuler.x, -60, 60);
            curEuler.y += mouseX;

            curDir = Quaternion.Euler(curEuler) * defaultDir;

            yRotateEuler.y = (yRotateEuler.y + mouseX + 360) % 360;

            followObj.transform.localPosition = Quaternion.Euler(yRotateEuler) * yRotateDir * yRotateDist;

            Vector3 newPosV = curDir * curDist;
            transform.position = followObj.transform.position + newPosV + cameraShakeOffset;

            transform.LookAt(followObj.transform);
        }
        CameraRaycast();//狙ってる位置を取得
        if (detectSphereGo != null)
        {
            detectSphereGo.transform.position = curRayHitPosition;
        }
    }

    /// <summary>
    /// レイキャスティング（カメラ狙ってる位置
    /// </summary>
    void CameraRaycast()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(this.transform.position, transform.forward, out hitInfo, maxRaycastDist, GameDefine.Instance.Layer_CameraDetectable))
        {
            if (hitInfo.collider.tag == GameDefine.Instance.Tag_SpecialAnchor)
            {
                SpecialAnchor anchor = hitInfo.collider.gameObject.GetComponent<SpecialAnchor>();//detect special anchor
                if (anchor != null)
                {
                    if (curSA == null)
                    {
                        curSA = anchor;
                        if (curSA.isSelect == false)
                        {
                            curSA.SelectAnchor(true);
                        }
                    }
                    else if (curSA != anchor)
                    {
                        curSA.SelectAnchor(false);

                        curSA = anchor;
                        curSA.SelectAnchor(true);
                    }
                    if (UIManager.Instance.crosshair != null)
                    {
                        if (anchor.anchorType == SpecialAnchor.E_AnchorType.Attract)
                        {
                            UIManager.Instance.crosshair.SetButtonTextState(E_CrosshairPart.F_Text, true, false);
                        }
                        else if (anchor.anchorType == SpecialAnchor.E_AnchorType.Floating)
                        {
                            UIManager.Instance.crosshair.SetButtonTextState(E_CrosshairPart.LB_Text, true, false);
                        }
                    }

                }
                else
                {
                    if (curSA != null)
                    {
                        curSA.SelectAnchor(false);
                        curSA = null;
                    }

                    if (UIManager.Instance.crosshair != null)
                    {
                        UIManager.Instance.crosshair.SetButtonTextState(E_CrosshairPart.F_Text, false, true);
                    }
                }
            }
            else
            {
                if (curSA != null)
                {
                    curSA.SelectAnchor(false);
                    curSA = null;
                }

                if (UIManager.Instance.crosshair != null)
                {
                    UIManager.Instance.crosshair.SetButtonTextState(E_CrosshairPart.F_Text, false, true);
                }
            }
            curRayHitPosition = hitInfo.point;

            UIManager.Instance.CrosshairColorChange(true);//当たった時照星の色を変える

            //当たったオブジェクトは地形かどうか
            if (hitInfo.collider.tag == GameDefine.Instance.Tag_Terrain)
            {
                isTerrain = true;
            }
            else
            {
                isTerrain = false;
            }

            if(hitInfo.collider.tag == GameDefine.Instance.Tag_ForbiddenZone)
            {
                isForbiddenZone = true;
            }
            else
            {
                isForbiddenZone = false;
            }

            curHitPointNormal = hitInfo.normal;//法線を保存
        }
        else
        {
            if (curSA != null)
            {
                curSA.SelectAnchor(false);
                curSA = null;
            }
            curRayHitPosition = transform.position + transform.forward * maxRaycastDist;

            UIManager.Instance.CrosshairColorChange(false);
            isTerrain = false;
            isForbiddenZone = false;

            if (UIManager.Instance.crosshair != null)
            {
                UIManager.Instance.crosshair.SetButtonTextState(E_CrosshairPart.F_Text, false, true);
            }
        }

        if (UIManager.Instance.crosshair != null)
        {
            UIManager.Instance.crosshair.SetForbiddenState(isForbiddenZone);
        }

        FloatingManager.Instance.curRayHitPosition = curRayHitPosition;
        FloatingManager.Instance.curHitPointNormal = curHitPointNormal;
        FloatingManager.Instance.curSA = curSA;
    }

    /// <summary>
    /// 狙われたオブジェクトはアウトライン効果
    /// </summary>
    /// <param name="select"></param>
    void SelectDetectObj(bool select)
    {
        if (colliders != null && colliders.Length > 0)
        {
            for (int index = 0; index < colliders.Length; index++)
            {
                if (colliders[index] == null) continue;

                colliders[index].gameObject.MapAllComponent<FlowObjCell>(cell =>
                {
                    if (cell != null)
                    {
                        cell.Select(select);//アウトライン効果
                    }
                });
            }
        }

    }

    /// <summary>
    /// 当たり判定（狙ってるところの辺りのオブジェクトを取得
    /// </summary>
    /// <param name="deltatime"></param>
    void SceneObjRaycast(float deltatime)
    {
        detectTimePass += deltatime;
        if (detectTimePass >= detectInterval)
        {
            SelectDetectObj(false);
            detectTimePass = 0;
            colliders = Physics.OverlapSphere(curRayHitPosition, 4.0f, GameDefine.Instance.layer_SceneObj);
            SelectDetectObj(true);
        }
    }

    #endregion lifeCycle_method

    #region external_method

    public void DoInit()
    {

    }

    /// <summary>
    /// 親オブジェクトを設定
    /// </summary>
    /// <param name="go"></param>
    public void SetFollowObj(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("[CameraMove]set follow obj failed");
            return;
        }
        followObj = go;

        yRotateDir = go.transform.localPosition;
        yRotateDist = yRotateDir.magnitude;
        yRotateDir = yRotateDir.normalized;

        //カメラ初期位置を設定
        xRotateDir = Quaternion.Euler(offsetAngle) * Vector3.forward;
        Vector3 initPos = followObj.transform.TransformPoint(-xRotateDir * maxDist);

        transform.position = initPos;
        transform.LookAt(followObj.transform);

        //回転リセット
        curEuler = Vector3.zero;
        xRotateEuler = Vector3.zero;
        yRotateEuler = Vector3.zero;
    }

    /// <summary>
    /// 狙ってるオブジェクトを取得
    /// </summary>
    /// <returns></returns>
    public IFlowObj[] GetFlowObjArray()
    {
        if (colliders == null)
        {
            return null;
        }
        int elementIndex = 0;
        IFlowObj[] returnArray = new IFlowObj[colliders.Length];
        for (int index = 0; index < colliders.Length; index++)
        {
            if (colliders[index] == null || colliders[index].tag != "SceneObj")
            {
                continue;
            }
            IFlowObj curObj = colliders[index].GetComponent<IFlowObj>();
            if (curObj != null)
            {
                returnArray[elementIndex++] = curObj;
            }

        }

        return returnArray;
    }

    /// <summary>
    /// 狙ってる位置を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurViewHitPoint()
    {
        return curRayHitPosition;
    }

    /// <summary>
    /// 狙ってるオブジェクトの法線を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurHitPointNormal()
    {
        return curHitPointNormal;
    }

    /// <summary>
    /// 狙ってるオブジェクトは地形かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsTerrain()
    {
        return isTerrain;
    }

    /// <summary>
    /// 座標をワールド座標からローカル座標に変える
    /// </summary>
    /// <param name="v"></param>
    public void VectorTransform(ref Vector3 v)
    {
        Vector3 tempEuler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0, tempEuler.y, 0);
        v = transform.TransformVector(v);

        transform.eulerAngles = tempEuler;

    }



    public void SetFollowActive(bool active, bool activeLookAt = true)
    {
        if (followObj == null) return;
        followObjSign = active;
        lookAtObjSign = activeLookAt;
    }

    /// <summary>
    /// エイムモードに切り替え
    /// </summary>
    /// <param name="enter"></param>
    public void EnterAimMode(bool enter)
    {
        if (aimMode == enter || floatMode==true) return;

        //カメラ位置の補間アニメーションパラメータを設定
        if (aimAnimEnd)
        {
            aimTime = baseAimScaleTime;
        }
        else
        {
            aimTime = baseAimScaleTime - timePass;
        }
        if (aimTime == 0)
        {
            aimTime_I = 0;
        }
        else
        {
            aimTime_I = 1.0f / aimTime;
        }
        aimAnimEnd = false;
        timePass = 0;

        if (enter)
        {
            startAnimDist = curDist;
            endAnimDist = -(curDist - aimDist);

            aimMode = true;
        }
        else
        {
            startAnimDist = curDist;
            endAnimDist = maxDist - curDist;
            aimMode = false;
        }
        FloatingManager.Instance.aimMode = aimMode;

    }

    /// <summary>
    /// ノーマルモードに切り替え
    /// </summary>
    /// <param name="enter"></param>
    public void EnterFloatMode(bool enter)
    {
        if (floatMode == enter) return;

        //カメラ位置の補間アニメーションパラメータを設定
        if (floatAnimEnd)
        {
            floatTime = baseFloatScaleTime;
        }
        else
        {
            floatTime = baseFloatScaleTime - floatTimePass;
        }
        if (floatTime == 0)
        {
            floatTime_I = 0;
        }
        else
        {
            floatTime_I = 1.0f / floatTime;
        }
        floatAnimEnd = false;
        floatTimePass = 0;

        if (enter)
        {
            startAnimDist = curDist;
            endAnimDist = -(curDist - floatDist);

            floatMode = true;
        }
        else
        {
            startAnimDist = curDist;
            endAnimDist = maxDist - curDist;
            floatMode = false;
        }
    }

    /// <summary>
    /// ボール生成できるかどうか
    /// </summary>
    /// <returns></returns>
    public bool CanCraeteZone()
    {
        return !isForbiddenZone;
    }

    public void CameraShake(float duration,float magnitude)
    {
        if (cameraShakeC != null)
        {
            StopCoroutine(cameraShakeC);
            cameraShakeC = null;
        }
        cameraShakeC = StartCoroutine(CameraShakeCoroutine(duration, magnitude));
    }

    #endregion external_method

    #region debug_method

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(curRayHitPosition, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, curRayHitPosition);

    }

    #endregion debug_method

    IEnumerator CameraShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cameraShakeOffset.x = UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;
            cameraShakeOffset.y = UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;

            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
