//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 無重力ボールスクリプト
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FloatingZone : Zone
{
    public int zoneId;//ID
    public LineRenderer previewLine;//軌道線

    public AttractPoint nextAttractPoint;//次の重力ボール
    public AttractPointData nextAttractData;//次の重力ボールのデータ

    public List<FlowObjCell> flowObjList;//無重力状態になったオブジェクト

    //スケールアニメーションのパラメータ
    public float startScale;
    public float targetScale;
    public float scaleAnimTime;
    public float scaleAnimTimePass;

    //当たり判定のパラメータ
    public float detectRadiu;//半径
    public float delayDetect;//数秒経った後当たり判定を行い
    public float delayTimePass;

    public Vector3 posOffset;//位置オフセット

    bool init = false;
    bool playScaleAnim = false;
    bool detect = false;
    public bool beginAttract = false;
    public bool previewStart = false;

    public int zoneMask = 0xff;//当たり判定フィルター

    Tweener scaleTweener;

    public float delayDestroyTime = 3f;
    bool isDestroy = false;

    #region lifeCycle_method

    // Start is called before the first frame update
    void OnEnable()
    {
        ResetSelf();//リセット
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroy)//削除
        {
            return;
        }

        if (init)
        {
            //スケールアニメーション
            if (playScaleAnim == false)
            {
                scaleTweener = transform.DOScale(new Vector3(targetScale, targetScale, targetScale), scaleAnimTime);
                scaleTweener.OnComplete(InitAnimCallback);//アニメーション終了コールバック
                playScaleAnim = true;
            }
        }

        if (detect == false)
        {
            delayTimePass += Time.deltaTime;

            if (delayDetect <= delayTimePass)
            {
                detect = true;
                DetectFlowObj();
            }
        }

        if (flowObjList != null && flowObjList.Count > 0)
        {
            for (int index = 0; index < flowObjList.Count; index++)
            {
                if (flowObjList[index] == null) continue;
                flowObjList[index].DoUpdate(Time.deltaTime);
            }
        }

    }

    private void OnDisable()
    {
        ResetSelf();
    }

    private void OnDestroy()
    {
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
        }
    }

    #endregion lifeCycle_method

    /// <summary>
    /// 当たり判定（周りのオブジェクトを無重力にする
    /// </summary>
    void DetectFlowObj()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectRadiu, GameDefine.Instance.layer_SceneObj);
        if (colliders != null && colliders.Length > 0)
        {
            for (int index = 0; index < colliders.Length; index++)
            {
                if (colliders[index] == null)
                {
                    continue;
                }

                FlowObjCell cell = colliders[index].GetComponent<FlowObjCell>();
                if (cell == null || cell.canFloat == false || FlowObjCell.CheckMask(zoneMask, cell.cellType)) continue;

                if (flowObjList.Contains(cell) == false)
                {
                    flowObjList.Add(cell);//リストに保存する
                    cell.SetParentZone(this);
                }

                IFloatObject floatObj = cell.gameObject.GetComponent<IFloatObject>();
                if (floatObj != null)
                {
                    floatObj.FloatingStart(cell);
                }
                //無重力になったオブジェクトを宙に浮かせる
                cell.SetCenterPoint(transform.position);
                cell.Flow(true, Vector3.zero,false, index);

                DelayDestroy dd = cell.gameObject.GetComponent<DelayDestroy>();
                if (dd != null)
                {
                    dd.StopDestroy();
                }

            }
        }
        detect = false;
    }

    /// <summary>
    /// リセット
    /// </summary>
    void ResetSelf()
    {
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
        }

        transform.localScale = new Vector3(startScale, startScale, startScale);
        scaleAnimTimePass = 0;
        delayTimePass = 0;

        if (flowObjList != null)
        {
            flowObjList.Clear();
        }

        init = false;
        playScaleAnim = false;
        detect = false;

        beginAttract = true;
    }


    #region external_method

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="id"></param>
    public void DoInit(int id)
    {
        zoneId = id;
    }

    /// <summary>
    /// 生成位置を設定する
    /// </summary>
    /// <param name="startPos"></param>
    public void SetStartPos(Vector3 startPos)
    {
        transform.position = startPos + posOffset;

        init = true;
    }

    /// <summary>
    /// 無重力状態になったオブジェクトを重力ボールへ移動する
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="isTerrainPoint"></param>
    /// <param name="delayRate"></param>
    public void SetFloat(Vector3 pos,bool isTerrainPoint ,int delayRate = 0)
    {
        if (flowObjList == null || flowObjList.Count == 0)
        {
            return;
        }

        for (int index = 0; index < flowObjList.Count; index++)
        {
            if (flowObjList[index] == null) continue;
            flowObjList[index].Flow(false, pos, isTerrainPoint,index);
        }
    }

    /// <summary>
    /// 無重力状態になったオブジェクトを重力ボールへ移動する
    /// </summary>
    public void SetFloat(AttractPointData data)
    {
        if (data == null)
        {
            Debug.LogError("[FloatingZone]Float failed, attractPoint is null");
            return;
        }
        else if (beginAttract)
        {
            return;
        }

        beginAttract = true;

        SetFloat(data.GetAttractPos(),data.isGroundPoint);

        if (data.GetNextAttractZone() != null)
        {
            nextAttractData = data.GetNextAttractZone();
        }
    }

    /// <summary>
    /// 削除
    /// </summary>
    public void DestroyZone()
    {
        isDestroy = true;
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
        }
        ActivePreviewLink(false, Vector3.zero);
        scaleTweener = transform.DOScale(new Vector3(startScale, startScale, startScale), delayDestroyTime);
        scaleTweener.OnComplete(DestroySelf);

        FloatingManager.Instance.UnregisterFloatingZone(this);
    }

    public void SetFloatMask(int mask)
    {
        if (mask < 0)
        {
            zoneMask = 0;
            return;//no mask
        }
        zoneMask = mask;
    }

    public void SetFloatMask(E_FloatCellType maskType, bool overwrite = false)
    {
        if (maskType == E_FloatCellType.None)
        {
            zoneMask = 0;
            return;
        }
        if (overwrite)
        {
            zoneMask = 1 << (int)maskType;
        }
        else
        {
            zoneMask |= 1 << (int)maskType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    public void ReleaseObj(FlowObjCell cell)
    {
        if (cell == null) return;
        if (flowObjList == null || flowObjList.Count == 0) return;

        if (flowObjList.Contains(cell))
        {
            flowObjList.Remove(cell);
        }
    }

    public void ReleaseAllObj()
    {
        if (flowObjList == null || flowObjList.Count == 0)
        {
            return;
        }

        for (int index = 0; index < flowObjList.Count; index++)
        {
            if (flowObjList[index] == null) continue;
            flowObjList[index].ReleaseObj();
        }

        flowObjList.Clear();
    }

    public void RegisterObj(FlowObjCell cell, bool startFloat = true)
    {
        if (cell == null) return;

        if (flowObjList.Contains(cell)) return;
        cell.SetParentZone(this);
        flowObjList.Add(cell);

        if (startFloat)
        {
            if (FlowObjCell.CheckMask(zoneMask, cell.cellType)) return;

            cell.SetCenterPoint(transform.position);
            cell.Flow(true, Vector3.zero,false, 1);

            DelayDestroy dd = cell.gameObject.GetComponent<DelayDestroy>();
            if (dd != null)
            {
                dd.StopDestroy();//TODO
            }

            IFloatObject floatObj = cell.gameObject.GetComponent<IFloatObject>();
            if (floatObj != null)
            {
                floatObj.FloatingStart(cell);
            }
        }
    }

    /// <summary>
    /// 次の移動先を取得
    /// </summary>
    /// <param name="cell">TODO</param>
    /// <param name="countingTime"></param>
    /// <param name="nextPos"></param>
    /// <returns></returns>
    public bool GetNextZonePos(FlowObjCell  cell,int countingTime ,out Vector3 nextPos,out bool isTerrain)
    {
        if (cell == null || flowObjList.Contains(cell) == false || nextAttractData == null)
        {
            nextPos = Vector3.zero;
            isTerrain = false;
            return false;
        }

        int floatTimes = countingTime;
        int count = 1;
        AttractPointData nextAP = nextAttractData;
        while (floatTimes >= count && nextAP != null)
        {
            if (floatTimes == count)
            {
                nextPos = nextAP.GetAttractPos();
                isTerrain = nextAP.isGroundPoint;
                return true;
            }

            nextAP = nextAP.GetNextAttractZone();
            count++;
        }

        nextPos = Vector3.zero;
        isTerrain = false;
        return false;
    }

    /// <summary>
    /// 軌道線を表示
    /// </summary>
    /// <param name="active"></param>
    /// <param name="linkPos"></param>
    public void ActivePreviewLink(bool active, Vector3 linkPos)
    {
        if (previewLine == null) return;
        if (active)
        {
            if (previewStart)
            {
                previewLine.SetPosition(1, linkPos);
            }
            else
            {
                if (previewLine.enabled == false)
                {
                    previewLine.enabled = true;
                }
                previewLine.SetPosition(0, this.transform.position);
                previewLine.SetPosition(1, linkPos);

                previewStart = true;
            }

        }
        else
        {
            previewStart = false;
            previewLine.enabled = false;
            previewLine.SetPosition(0, this.transform.position);
            previewLine.SetPosition(1, this.transform.position);
        }
    }

    #endregion external_method

    #region callback_method

    void InitAnimCallback()
    {
        beginAttract = false;
    }

    void TestCallBack()
    {
        Debug.LogError("Test callback");
    }

    /// <summary>
    /// 自分を削除
    /// </summary>
    void DestroySelf()
    {
        DelayDestroy dd = this.gameObject.AddComponentOnce<DelayDestroy>();//一定の時間が経った後削除する
        if (dd == null)
        {
            DestroyImmediate(this.gameObject);
        }
        else
        {
            dd.delayTime = delayDestroyTime;
            dd.StartDestroy();
        }
    }

    #endregion callback_method


}
