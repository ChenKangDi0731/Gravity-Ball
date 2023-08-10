//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 重力/無重力ボールを管理するマネージャー
/// ‐ボールの生成の処理
/// ‐無重力になったものが重力ボールに引き寄せられる処理
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class FloatingManager : Singleton<FloatingManager>
{
    CameraController cameraInstance;

    public GameObject detectZonePrefab;//無重力ボールプリハブ
    public GameObject attractPointPrefab;//重力ボールプリハブ

    public SpecialAnchor curSA;
    public bool aimMode = false;//エイムモードのフラグ
    public Vector3 curRayHitPosition = Vector3.zero;
    public Vector3 curHitPointNormal = Vector3.zero;

    //player param
    public List<FloatingZone> floatZoneList = new List<FloatingZone>();
    public List<int> floatZoneIDList = new List<int>();
    public Dictionary<int, FloatingZone> floatZoneDic = new Dictionary<int, FloatingZone>();
    public FloatingZone curFloatZone;//今の無重力ボール（一番最後に生成された
    public AttractPoint curAttractPoint;//今の重力ボール（一番最後に生成された

    public SpecialAnchor_Floating sa_floating;
    public SpecialAnchor_Attract sa_attract;

    int maxFloat = 3;//無重力ボールが生成できる最大の数
    int maxAttract = 3;//重力ボールが生成できる最大の数

    float debugTime = 1f;
    float debugTimePass = 0;

    public int maxFloatingZoneCount
    {
        get { return maxFloat; }
        set { maxFloat = value;
        }
    }
    public int maxAttractPointCount
    {
        get { return maxAttract; }
        set
        {
            maxAttract = value;
        }
    }
    int _curAttract;
    int curAttractPointCount
    {
        get
        {
            return _curAttract;
        }
        set
        {
            _curAttract = Mathf.Clamp(value, 0, maxAttractPointCount);
        }
    }

    //other param
    int maxFloatIdCount = 1 << 13;
    int floatIdIndex = 0;
    int ZoneId
    {
        get
        {
            return (floatIdIndex++ % maxFloatIdCount);
        }
    }

    bool needUpdateAttractPoint;

    #region lifeCycle_method

    public void DoInit()
    {
        //GameConfigからプリハブを取得
        if (GameConfig.Instance != null)
        {
            detectZonePrefab = GameConfig.Instance.detectZonePrefab;
            attractPointPrefab = GameConfig.Instance.attractPointPrefab;
        }

        needUpdateAttractPoint = false;

        maxFloatingZoneCount = GameConfig.Instance.maxFloatingZoneCount;
        maxAttractPointCount = GameConfig.Instance.maxAttractPointCount;

        curAttractPointCount = 0;
    }

    /// <summary>
    /// アップデート
    /// </summary>
    /// <param name="deltaTime"></param>
    public void DoUpdate(float deltaTime)
    {
        //重力ボールと無重力ボールが同時に存在する場合、無重力になったオブジェクトを重力ボールへ移動する
        if (curAttractPoint != null)
        {
            if (floatZoneDic.Count != 0)
            {
                bool activeAttract = false;

                foreach (var temp in floatZoneDic)
                {
                    if (temp.Value == null)
                    {
                        continue;
                    }
                    FloatingZone zone = temp.Value;

                    if (zone.beginAttract == true)
                    {
                        //Debug.LogError("Found unregister active floatzone");
                        continue;
                    }

                    activeAttract = true;
                }
                if (activeAttract)
                {
                    //オブジェクトの移動軌道を生成
                    AttractPointData startData = new AttractPointData();
                    curAttractPoint.FillAttractPointData(ref startData);

                    AttractPoint tempAP = curAttractPoint;

                    AttractPointData tempData = startData;
                    AttractPointData nextData = null;

                    //複数の重力ボールが存在する場合は軌道を作る
                    while (tempAP.GetNextAttractZone() != null)
                    {
                        tempAP = tempAP.GetNextAttractZone();

                        nextData = new AttractPointData();
                        tempAP.FillAttractPointData(ref nextData);//データをコピー

                        tempData.isLastData = false;
                        tempData.nextPoint = nextData;//次ののデータとして設定
                        tempData = nextData;
                    }
                    tempData.isLastData = true;//最後のポイント

                    FloatingZone[] zoneArray = floatZoneDic.Values.ToArray();
                    if (zoneArray != null && zoneArray.Length != 0)
                    {
                        for(int index = 0; index < zoneArray.Length; index++)
                        {
                            if (zoneArray == null) continue;
                            zoneArray[index].SetFloat(startData);//無重力になったオブジェクトを重力ボールへ移動
                            zoneArray[index].DestroyZone();
                        }
                        UnregisterFloatingZone(null, true);//全ての無重力ボールを削除

                        //UIを更新
                        if (UIManager.Instance.hud != null)
                        {
                            UIManager.Instance.hud.SetFlowPointCount(maxFloatingZoneCount - floatZoneDic.Count);
                        }
                    }
                    needUpdateAttractPoint = true;//重力ボールリストを更新
                }
            }
        }

        //sa attract detect
        if (sa_floating != null)
        {
            if (sa_attract != null && sa_attract.IsActive())
            {
                sa_floating.SetParam(0, sa_attract.GetAttractPoint());
                sa_floating.SetParam(1, sa_attract);

                sa_floating.ActiveAnchorAction(true);

                sa_floating = null;//release
                sa_attract = null;
            }
        }

        //エイムモードの時は　軌道線を表示する
        if (aimMode)
        {
            //軌道線を表示
            if (curFloatZone != null)
            {
                curFloatZone.ActivePreviewLink(true, curRayHitPosition);
            }
            if (floatZoneDic != null && floatZoneDic.Count != 0)
            {
                foreach (var temp in floatZoneDic)
                {
                    if (temp.Value == null)
                    {
                        continue;
                    }
                    temp.Value.ActivePreviewLink(true, curRayHitPosition);
                }

            }
            else if (curAttractPoint != null)
            {
                curAttractPoint.ActivePreviewLink(true, true, curRayHitPosition);
            }
        }
        else
        {
            if (curFloatZone != null && curFloatZone.previewStart)
            {
                curFloatZone.ActivePreviewLink(false, Vector3.zero);
            }
            if (floatZoneDic != null && floatZoneDic.Count != 0)
            {

                foreach (var temp in floatZoneDic)
                {
                    if (temp.Value == null)
                    {
                        continue;
                    }
                    temp.Value.ActivePreviewLink(false, Vector3.zero);
                }

            }
            else if (curAttractPoint != null && curAttractPoint.previewStart)
            {
                curAttractPoint.ActivePreviewLink(true, false, Vector3.zero);
            }
        }
#if UNITY_EDITOR
        if (debugTimePass >= debugTime)
        {
            debugTimePass = 0;
            if(floatZoneIDList!=null && floatZoneIDList.Count != 0)
            {
                string tempStr = string.Empty;
                for(int index = 0; index < floatZoneIDList.Count; index++)
                {
                    tempStr += floatZoneIDList[index].ToString() + " , ";
                }

            }
        }
        else
        {
            debugTimePass += deltaTime;
        }
#endif
    }

    public void DoFixedUpdate(float fixedDeltaTIme)
    {

    }

    public void DoLateUpdate(float deltaTime)
    {
        //無重力ボールリストを更新する（削除されたボールをリストの中から取り除く
        if (needUpdateAttractPoint)
        {
            needUpdateAttractPoint = false;
            if (curAttractPoint != null)
            {

                AttractPoint tempStartPoint = null;

                AttractPoint tempAP = curAttractPoint;
                AttractPoint nextAP = null;

                List<AttractPoint> attractPointList = new List<AttractPoint>();

                while (tempAP != null)
                {
                    nextAP = tempAP.GetNextAttractZone();
                    if (tempAP.attractType == E_AttractType.Once)
                    {
                        tempAP.SetNextAttractZone(null);
                        tempAP.SetAttractState(true);
                    }
                    else if (tempAP.attractType == E_AttractType.Timer)
                    {

                        attractPointList.Add(tempAP);
                    }

                    tempAP = nextAP;
                }

                curAttractPoint = tempStartPoint;

                if (attractPointList.Count != 0)
                {
                    curAttractPointCount = attractPointList.Count;
                    if (attractPointList.Count == 1)
                    {
                        if (attractPointList[0] != null)
                        {
                            curAttractPoint = attractPointList[0];
                            curAttractPoint.ResetNextAttractZone(null);
                        }
                    }
                    else
                    {
                        for (int index = attractPointList.Count - 1; index >= 1; index--)
                        {
                            if (attractPointList[index] == null || attractPointList[index - 1] == null)
                            {
                                continue;
                            }
                            attractPointList[index - 1].ResetNextAttractZone(null);
                            attractPointList[index - 1].ResetNextAttractZone(attractPointList[index]);//??
                        }
                        curAttractPoint = attractPointList[0];
                        attractPointList[attractPointList.Count - 1].ResetNextAttractZone(null);
                    }
                }
                else
                {
                    curAttractPointCount = 0;
                }

                //無重力ボールUIを更新
                if (UIManager.Instance.hud != null)
                {
                    UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount - curAttractPointCount);
                }
            }
        }
    }

    #endregion lifeCycle_method

    public bool CheckSA(SpecialAnchor anchor, SpecialAnchor.E_AnchorType anchorType)
    {
        return (anchor != null && anchor.anchorType == anchorType/* && anchor.IsActive() == false*/);
    }

    #region external_method

    public void SetAimMode(bool state)
    {
        aimMode = state;
    }

    /// <summary>
    /// 生成された無重力ボールをリストに追加
    /// </summary>
    /// <param name="zone"></param>
    public void RegisterFloatingZone(FloatingZone zone)
    {
        if (zone == null)
        {
            Debug.LogError("[PlayerController]Register float zone failed, zone is null");
            return;
        }

        if (floatZoneDic == null) floatZoneDic = new Dictionary<int, FloatingZone>();
        if (floatZoneDic.ContainsKey(zone.zoneId))
        {
            Debug.LogError("[PlayerController]Register float zone failed, zone already registered");
            return;
        }

        //最大数になった時はまず一番古いボールを削除してから　新しいボールを作る
        if (floatZoneDic.Count >= maxFloatingZoneCount)
        {
            DestroyFirstFloatingZone();
            curFloatZone = zone;
        }

        floatZoneDic.Add(zone.zoneId, zone);
        floatZoneIDList.Add(zone.zoneId);

        //UIを更新
        if (UIManager.Instance.hud != null)
        {
            UIManager.Instance.hud.SetFlowPointCount(maxFloatingZoneCount - floatZoneDic.Count);
        }
    }

    /// <summary>
    /// 無重力ボールを削除
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="operateAll">全ての無重力ボールを削除するフラグ</param>
    public void UnregisterFloatingZone(FloatingZone zone,bool operateAll = false)
    {
        if (floatZoneDic == null || floatZoneDic.Count == 0) return;

        if (operateAll)
        {
            floatZoneDic.Clear();
            floatZoneIDList.Clear();
            curFloatZone = null;

            //UIを更新
            if (UIManager.Instance.hud != null)
            {
                UIManager.Instance.hud.SetFlowPointCount(maxFloatingZoneCount - floatZoneDic.Count);
            }
        }
        else
        {
            if (floatZoneDic.ContainsKey(zone.zoneId))
            {
                floatZoneDic.Remove(zone.zoneId);
                RemoveFloatZoneID(zone.zoneId);
                if (floatZoneDic.Count != 0)
                {
                    if (floatZoneIDList.Count != 0)
                    {
                        int tempID = floatZoneIDList[floatZoneIDList.Count - 1];
                        if (floatZoneDic.ContainsKey(tempID))
                        {
                            curFloatZone = floatZoneDic[tempID];//reset curFloatZone
                        }
                        else
                        {
                            Debug.LogError("------------------- Set CurFloatZone error");
                            int[] temp = floatZoneDic.Keys.ToArray();
                            curFloatZone = floatZoneDic[temp[temp.Length - 1]];
                        }
                    }
                }
                else
                {
                    curFloatZone = null;
                }

                //UIを更新
                if (UIManager.Instance.hud != null)
                {
                    UIManager.Instance.hud.SetFlowPointCount(maxFloatingZoneCount - floatZoneDic.Count);
                }
            }
        }
    }

    public bool CanCreateFloatingZone()
    {
        return maxFloatingZoneCount > floatZoneList.Count;
    }

    /// <summary>
    /// 無重力ボールを生成する
    /// </summary>
    /// <param name="useCameraParam"></param>
    /// <param name="createPos"></param>
    /// <returns></returns>
    public FloatingZone CreateFloatingZone(bool useCameraParam,Vector3 createPos)
    {
        if (CheckSA(curSA, SpecialAnchor.E_AnchorType.Floating))
        {
            if (sa_floating != null)
            {
                if (sa_floating != curSA)
                {
                    sa_floating.ActiveAnchorAction(false);
                    sa_floating.ActiveAnchor(false);
                }
                else
                {
                    if (sa_floating.IsActionActive() == false)
                    {
                        sa_floating.ActiveAnchor(!sa_floating.IsActive());
                        return null;
                    }
                }
            }
            sa_floating = curSA as SpecialAnchor_Floating;
            sa_floating.ActiveAnchor(true);

            AudioManager.Instance.PlaySound(E_SoundType.FloatShot, true);
        }
        else
        {
            if (detectZonePrefab == null)//プリハブNULLチェック
            {
                Debug.LogError("[CharacterMove]detect zone prefabs is null");
            }
            else
            {
                GameObject newGO = GameObject.Instantiate(detectZonePrefab);
                if (newGO != null)
                {
                    curFloatZone = newGO.GetComponent<FloatingZone>();
                    if (curFloatZone != null)
                    {
                        curFloatZone.SetFloatMask(E_FloatCellType.Player,true);//マスクを設定（プレイヤーには影響されない
                        curFloatZone.DoInit(this.ZoneId);

                        //生成位置を設定
                        if (useCameraParam)
                        {
                            curFloatZone.SetStartPos(curRayHitPosition);
                        }
                        else
                        {
                            curFloatZone.SetStartPos(createPos);
                        }

                        RegisterFloatingZone(curFloatZone);

                        AudioManager.Instance.PlaySound(E_SoundType.FloatShot, true);


                        return curFloatZone;
                    }
                    else
                    {
                        Debug.LogError("Get Zone Script failed");
                    }
                }
            }
        }

        return null;
    }

    public void ResetSP_FloatingPoint()
    {
        sa_attract = null;
    }

    public void ResetSP_AttractPoint()
    {
        sa_floating = null;
    }

    /// <summary>
    /// 重力ボールを生成（時間制限がある　＆　時間制限内無限に使える
    /// </summary>
    /// <param name="useCameraParam"></param>
    /// <param name="pos"></param>
    /// <param name="isTerrain"></param>
    /// <returns></returns>
    public AttractPoint CreateTimerAttractPoint(bool useCameraParam,Vector3 pos,bool isTerrain = false)
    {
        return CreateAttractPoint(useCameraParam, pos, E_AttractType.Timer,isTerrain);
    }

    /// <summary>
    /// 重力ボールを生成（一回しか使えない　＆　時間制限がない
    /// </summary>
    /// <param name="useCameraParam"></param>
    /// <param name="pos"></param>
    /// <param name="isTerrain"></param>
    /// <returns></returns>
    public AttractPoint CreateOnceAttractPoint(bool useCameraParam,Vector3 pos,bool isTerrain = false)
    {
        return CreateAttractPoint(useCameraParam, pos, E_AttractType.Once,isTerrain);
    }

    /// <summary>
    /// 重力ボールを生成
    /// </summary>
    /// <param name="useCameraParam"></param>
    /// <param name="pos"></param>
    /// <param name="attractType"></param>
    /// <param name="isTerrain"></param>
    /// <returns></returns>
    AttractPoint CreateAttractPoint(bool useCameraParam,Vector3 pos , E_AttractType attractType,bool isTerrain=false)
    {
        if(isTerrain == true)
        {
            int a = 0;
        }
        if (CheckSA(curSA, SpecialAnchor.E_AnchorType.Attract))
        {
            if (sa_attract != null)
            {
                if (curSA != sa_attract)
                {
                    sa_attract.ActiveAnchorAction(false);
                    sa_attract.ActiveAnchor(false);
                }
                else
                {
                    sa_attract.ActiveAnchor(!sa_attract.IsActive());
                    return null;
                }
            }

            sa_attract = curSA as SpecialAnchor_Attract;
            sa_attract.ActiveAnchor(true);

            AudioManager.Instance.PlaySound(E_SoundType.AttractShot, true);

        }
        else
        {
            //最大数になる場合は一番古いボールを削除してから　新しいボールを作る
            if (curAttractPointCount >= maxAttractPointCount)
            {
                DestroyFirstAttractPoint();
            }

            if (attractPointPrefab != null)
            {
                GameObject temp = GameObject.Instantiate(attractPointPrefab);
                if (temp != null)
                {
                    AttractPoint tempAP = temp.GetComponent<AttractPoint>();
                    if (tempAP == null)
                    {
                        Debug.LogError("Get AttractPoint Component failed , gameObject = " + temp.name);
                        GameObject.DestroyImmediate(temp);
                    }
                    else
                    {
                        tempAP.DoInit(attractType);

                        //初期位置を設定
                        if (useCameraParam)
                        {
                            tempAP.SetStartPos(curRayHitPosition,isTerrain);
                        }
                        else
                        {
                            tempAP.SetStartPos(pos,isTerrain);
                        }
                        if (curAttractPoint == null)
                        {
                            curAttractPoint = tempAP;
                        }
                        else
                        {
                            curAttractPoint.SetNextAttractZone(tempAP);
                        }

                        curAttractPointCount++;//数を増やす

                        //UIを更新
                        if (UIManager.Instance.hud != null)
                        {
                            UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount -curAttractPointCount);
                        }

                        AudioManager.Instance.PlaySound( E_SoundType.AttractShot, true);

                        return tempAP;
                    }
                }
                else
                {
                    Debug.LogError("Create AttractPoint failed");
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 巻き戻し（ボールを削除
    /// </summary>
    public void Undo()
    {
        if (curAttractPoint != null)
        {
            if (curAttractPoint.GetNextAttractZone() != null)
            {
                AttractPoint temp = curAttractPoint;
                AttractPoint tempChild = curAttractPoint;
                while (tempChild != null && tempChild.GetNextAttractZone() != null)
                {
                    temp = tempChild;
                    tempChild = temp.GetNextAttractZone();
                }
                temp.SetNextAttractZone(null);
                tempChild.DestroyPoint();
            }
            else
            {
                curAttractPoint.DestroyPoint();
                curAttractPoint = null;
            }
            curAttractPointCount--;//数を更新
            //UIを更新
            if (UIManager.Instance.hud != null)
            {
                UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount - curAttractPointCount);
            }
        }
        else if (curFloatZone != null)
        {
            FloatingZone tempZone = curFloatZone;

            UnregisterFloatingZone(curFloatZone);

            tempZone.ReleaseAllObj();
            tempZone.DestroyZone();
            tempZone = null;
        }
    }

    /// <summary>
    /// 全てのボールを削除
    /// </summary>
    public void UndoAll()
    {
        #region undo_all_zone
        if (curAttractPoint != null)
        {
            AttractPoint temp = curAttractPoint;
            AttractPoint tempChild = curAttractPoint;
            while (tempChild != null)
            {
                temp = tempChild;
                tempChild = temp.GetNextAttractZone();

                temp.DestroyPoint();
            }
            curAttractPoint = null;

            curAttractPointCount = 0;

            //UIを更新
            if (UIManager.Instance.hud != null)
            {
                UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount - curAttractPointCount);
            }
        }

        FloatingZone[] zoneArray = floatZoneDic.Values.ToArray();
        if(zoneArray!=null && zoneArray.Length != 0)
        {
            for (int index = 0; index < zoneArray.Length; index++) {
                if (zoneArray[index] == null) continue;
                zoneArray[index].ReleaseAllObj();
                zoneArray[index].DestroyZone();
            }
        }
        UnregisterFloatingZone(null, true);
       
        #endregion undo_all_zone
    }

    /// <summary>
    /// 無重力ボールを削除
    /// </summary>
    /// <param name="zone"></param>
    public void DestroyFloatingZone(FloatingZone zone)
    {
        if (zone == null) return;
        if (floatZoneDic.ContainsKey(zone.zoneId))
        {
            UnregisterFloatingZone(zone);
        }

        zone.ReleaseAllObj();
        zone.DestroyZone();
    }

    /// <summary>
    /// 重力ボールを削除
    /// </summary>
    /// <param name="point"></param>
    public void DestroyAttractPoint(AttractPoint point)
    {
        if (point == null || curAttractPoint==null) return;

        if (curAttractPoint == point)
        {
            if (curAttractPoint.GetNextAttractZone() != null)
            {
                curAttractPoint = curAttractPoint.GetNextAttractZone();
            }
            else
            {
                curAttractPoint = null;
            }
            Debug.LogError("Unregister AttractPoint 1");
            curAttractPointCount--;

            if (UIManager.Instance.hud != null)
            {
                UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount - curAttractPointCount);
            }
            return;
        }

        AttractPoint tempPoint = curAttractPoint;
        AttractPoint nextPoint = tempPoint.GetNextAttractZone();
        while (nextPoint != null)
        {
            if (nextPoint == point)
            {
                tempPoint.ResetNextAttractZone(nextPoint.GetNextAttractZone());
                Debug.LogError("Unregister AttractPoint 2");
                curAttractPointCount--;

                if (UIManager.Instance.hud != null)
                {
                    UIManager.Instance.hud.SetAttractPointCount(maxAttractPointCount - curAttractPointCount);
                }
                break;
            }

            tempPoint = nextPoint;
            nextPoint = tempPoint.GetNextAttractZone();
        }
    }

    /// <summary>
    /// 一番古いボールを削除
    /// </summary>
    public void DestroyFirstAttractPoint()
    {
        if (curAttractPoint == null) return;

        AttractPoint temp = curAttractPoint;

        if (curAttractPoint.GetNextAttractZone() != null)
        {
            curAttractPoint = curAttractPoint.GetNextAttractZone();
        }

        temp.DestroyPoint();

        curAttractPointCount--;
    }

    /// <summary>
    /// プレイヤーを無重力にする処理
    /// </summary>
    /// <param name="player"></param>
    /// <param name="pos"></param>
    public void PlayerFloating(PlayerController player,Vector3 pos)
    {

        if (player == null)
        {
            Debug.LogError("[FloatingManager]Create float zone failed , player is null");
            return;
        }
        FlowObjCell playerCell = player.gameObject.GetComponent<FlowObjCell>();
        if (playerCell == null)
        {
            Debug.LogError("[FloatingManager]Create float zone failed, cannot not found FlowObjCell component , obj = " + player.gameObject.name);
            return;
        }

        #region floating_self

        if (detectZonePrefab == null)//float
        {
            Debug.LogError("[CharacterMove]detect zone prefabs is null");
        }
        else
        {
            GameObject newGO = GameObject.Instantiate(detectZonePrefab);
            if (newGO != null)
            {
                curFloatZone = newGO.GetComponent<FloatingZone>();
                if (curFloatZone != null)
                {
                    curFloatZone.SetFloatMask(-1);
                    curFloatZone.DoInit(ZoneId);
                    curFloatZone.SetStartPos(pos);
                    RegisterFloatingZone(curFloatZone);

                    curFloatZone.RegisterObj(playerCell, true);

                    AudioManager.Instance.PlaySound(E_SoundType.FloatShot, true);

                }
                else
                {
                    Debug.LogError("Get Zone Script failed");
                }
            }
        }
        #endregion floating_self
    }

    #endregion external_method

    void RemoveFloatZoneID(int id)
    {
        if (floatZoneIDList == null || floatZoneIDList.Count == 0) return;
        int index = 0;
        for (; index < floatZoneIDList.Count; index++)
        {
            if (floatZoneIDList[index] == id)
            {
                break;
            }
        }
        if (index < floatZoneIDList.Count)
        {
            floatZoneIDList.RemoveAt(index);
        }
    }

    /// <summary>
    /// 一番古いボールを削除
    /// </summary>
    void DestroyFirstFloatingZone()
    {
        int index = GetFirstFloatZoneID();
        if (index == -1)
        {
            Debug.LogError("[FloatingManager]Destroy first float zone failed, cannot found id");
            return;
        }

        DestroyFloatingZone(floatZoneDic[index]);
    }

    int GetFirstFloatZoneID()
    {
        if ((floatZoneIDList == null && floatZoneDic == null) || floatZoneIDList.Count == 0)
        {
            return -1;
        }

        int id = floatZoneIDList[0];
        floatZoneIDList.RemoveAt(0);

        if(curFloatZone!=null && curFloatZone.zoneId == id && floatZoneIDList.Count!=0)
        {
            int tempID = floatZoneIDList[floatZoneIDList.Count - 1];
            if (floatZoneDic.ContainsKey(tempID))
            {
                curFloatZone = floatZoneDic[tempID];
            }
        }

        return id;
    }

}
