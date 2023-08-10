//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 重力ボールスクリプト
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttractPoint : MonoBehaviour
{

    public LineRenderer previewLine;//軌道線

    public AttractPoint nextPoint;//次のボール

    public E_AttractType attractType = E_AttractType.None;//種類

    //スケールアニメション　パラメータ
    public Vector3 defaultScale = Vector3.zero;//生成された時のサイズ
    public Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);//ターゲットサイズ
    public float scaleAnimTime = 0.5f;//アニメーションの長さ

    public float delayDestroyTime = 0.5f;//一定時間後削除する（Onceタイプ
    public float delayDestroyTimePass = 0;

    public float delayDestroyAfterAttractTime = 0.7f;//一定時間後削除する（Timerタイプ
    public float delayDestroyAfterTimePass = 0;

    public bool init = false;
    bool isAttract = false;
    [SerializeField]bool isTerrain = false;
    public bool previewStart = false;
    //other param
    Tweener scaleTweener;

    #region lifeCycle_method

    // Update is called once per frame
    void Update()
    {

        if (init)
        {
            if (attractType == E_AttractType.Timer)
            {
                delayDestroyTimePass += Time.deltaTime;
                if (delayDestroyTimePass >= delayDestroyTime)//一定時間後削除する
                {
                    init = false;
                    DestroyPoint();
                    FloatingManager.Instance.DestroyAttractPoint(this);
                }
            }
            else if (attractType == E_AttractType.Once && isAttract)
            {
                delayDestroyAfterTimePass += Time.deltaTime;
                if (delayDestroyAfterTimePass >= delayDestroyAfterAttractTime)
                {
                    init = false;
                    DestroyPoint();
                    this.gameObject.SetActive( false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopTweener(scaleTweener);
    }

    #endregion lifeCycle_method

    #region external_method

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="pointType"></param>
    public void DoInit(E_AttractType pointType)
    {
        attractType = pointType;
        transform.localScale = defaultScale;

        init = false;
        isAttract = false;
        scaleTweener = transform.DOScale(targetScale, scaleAnimTime);
        scaleTweener.OnComplete(StartScaleCallback);//スケールアニメション終わった後のコールバック関数

        delayDestroyAfterTimePass = 0;
        delayDestroyTimePass = 0;

        ActivePreviewLink(false, false, Vector3.zero);
    }

    public void SetStartPos(Vector3 pos,bool _isTerrain)
    {
        transform.position = pos;
        isTerrain = _isTerrain;
    }

    public Vector3 GetAttractPos()
    {
        return this.transform.position;
    }

    public void Attract(FloatingZone floatZone)
    {
        if (floatZone == null)
        {
            Debug.LogError("[AttractPoint]Attract failed, floatZone is null");
            return;
        }

        floatZone.SetFloat(this.GetAttractPos(),isTerrain);
    }

    public void SetAttractState(bool state)
    {
        switch (attractType)
        {
            case E_AttractType.Once:
                isAttract = state;
                break;
            case E_AttractType.Timer:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 次の重力ボールを設定する
    /// </summary>
    /// <param name="point"></param>
    /// <param name="needUpdateLine"></param>
    public void SetNextAttractZone(AttractPoint point,bool needUpdateLine = true)
    {
        if (nextPoint != null && nextPoint == point)
        {
            return;
        }

        if (nextPoint == null)
        {
            nextPoint = point;
            if (needUpdateLine == false) return;

            //軌道線を更新する
            if (point == null)
            {
                ActivePreviewLink(true, false, transform.position);
            }
            else
            {
                ActivePreviewLink(true, false, point.GetAttractPos());
            }
        }
        else
        {
            nextPoint.SetNextAttractZone(point, needUpdateLine);
        }
    }

    public void ResetNextAttractZone(AttractPoint point)
    {
        if (nextPoint != null && nextPoint == point)
        {
            return;
        }

        nextPoint = point;
        if (nextPoint == null)
        {
            ActivePreviewLink(false, false, transform.position);
        }
        else
        {
            ActivePreviewLink(false, false, nextPoint.GetAttractPos());
        }
    }

    public AttractPoint GetNextAttractZone()
    {
        return nextPoint;
    }

    /// <summary>
    /// 最後の重力ボールを取得
    /// </summary>
    /// <returns></returns>
    public AttractPoint GetLastAttractZone()
    {
        AttractPoint temp = this;
        while (temp.nextPoint != null)
        {
            temp = temp.nextPoint;
        }
        return temp;
    }

    /// <summary>
    /// 重力ボールのデータを取得（無重力のオブジェクトを引き寄せ
    /// </summary>
    /// <param name="data"></param>
    public void FillAttractPointData(ref AttractPointData data)
    {
        if (data == null) data = new AttractPointData();
        data.attractPos = this.GetAttractPos();
        data.isGroundPoint = this.isTerrain;
    }

    /// <summary>
    /// ボールを削除
    /// </summary>
    public void DestroyPoint()
    {
        StopTweener(scaleTweener);

        //scaleTweener = transform.DOScale(defaultScale, scaleAnimTime).SetDelay(delayDestroyTime);
        scaleTweener = transform.DOScale(defaultScale, scaleAnimTime);
        scaleTweener.OnComplete(EndScaleCallback);

        //disable preview line
        ActivePreviewLink(false, false, transform.position);

    }

    /// <summary>
    /// 軌道線を表示
    /// </summary>
    /// <param name="lastZone"></param>
    /// <param name="active"></param>
    /// <param name="linkPos"></param>
    public void ActivePreviewLink(bool lastZone, bool active, Vector3 linkPos)
    {
        if (previewLine == null) return;
        if (active)
        {
            if (nextPoint == null) {
                previewLine.enabled = false;
                return; 
            }
            previewLine.enabled = true;
            if (previewStart)//update this point pos only 
            {
                if (lastZone == false)
                {
                    if (nextPoint != null)
                    {
                        previewLine.SetPosition(1, nextPoint.GetAttractPos());
                    }
                    else
                    {
                        previewLine.enabled = false;
                    }
                }
                else
                {
                    if (nextPoint != null)
                    {
                        previewLine.SetPosition(1, nextPoint.GetAttractPos());
                        nextPoint.ActivePreviewLink(lastZone, active, linkPos);
                    }
                    else
                    {
                        previewLine.enabled = false;
                    }
                }
            }
            else
            {
                previewLine.SetPosition(0, this.transform.position);
                if (nextPoint != null)
                {
                    previewLine.SetPosition(1, nextPoint.GetAttractPos());
                }
                else
                {
                    previewLine.enabled = false;
                    //previewLine.SetPosition(1, linkPos);
                }
                previewStart = true;
            }

        }
        else
        {
            previewStart = false;
            previewLine.enabled = false;
            previewLine.SetPosition(0, this.transform.position);
            previewLine.SetPosition(1, this.transform.position);

            if (lastZone == true && nextPoint != null)
            {
                nextPoint.ActivePreviewLink(lastZone, active, linkPos);
            }
        }
    }

    #endregion external_method


    void StopTweener(Tweener t)
    {
        if (t != null)
        {
            t.Kill();
        }
    }

    #region tweener_callback

    void StartScaleCallback()
    {
        init = true;
    }

    void EndScaleCallback()
    {
        DestroyImmediate(this.gameObject);
    }

    #endregion tweener_callback
}

/// <summary>
/// 重力ボールデータ
/// </summary>
public class AttractPointData
{

    public Vector3 attractPos = Vector3.zero;//ボールの位置
    public AttractPointData nextPoint = null;//次のボール
    public bool isLastData = false;//最後のボールかどうか
    public bool isGroundPoint = false;//地面にいるかどうか
    public AttractPointData()
    {

    }

    public AttractPointData(Vector3 pos,AttractPointData next)
    {
        attractPos = pos;
        nextPoint = next;
    }

    public AttractPointData GetNextAttractZone()
    {
        return nextPoint;
    }

    public Vector3 GetAttractPos()
    {
        return attractPos;
    }

}

public enum E_AttractType
{
    None,
    Once,
    Timer,
}