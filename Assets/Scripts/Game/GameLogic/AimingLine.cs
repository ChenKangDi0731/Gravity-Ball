///////////////////////////////////////////////////////////////////
///
/// 敵の狙い線
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingLine : MonoBehaviour
{
    public LineRenderer line;

    Vector3 basePoint;//始点
    Vector3 aimPoint;//終点

    public Color aimColor;//プレイヤーを追跡してるときの色
    public Color aimDoneColor;//狙いを定めた色

    public float lineMaxDist = 150f;//最大距離

    //other param
    public AnimationCurve aimAnimCurve = new AnimationCurve();
    public float animInterval;
    public float animTimePass = 0;
    public float curAnimValue = 0;

    Coroutine aimLineAnimCoroutine;

    public bool animActive = false;
    public bool lineActive = false;
    public bool stopAnimSign = false;

    #region lifeCycle_method

    private void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (lineActive)
        {
            SetAimPoint(aimPoint, true);
        }
    }

    #endregion lifeCycle_method


    void SetLinePos(int index,Vector3 pos)
    {
        if (line == null||index<0||index >= line.positionCount) return;

        line.SetPosition(index, pos);
    }

    void UpdateLineRaycast()
    {
        RaycastHit info;
        Vector3 dir = (aimPoint - basePoint).normalized;
        if(Physics.Raycast(basePoint,dir,out info, lineMaxDist))
        {
            aimPoint = info.point;
        }
        else
        {
            aimPoint = basePoint + dir * lineMaxDist;
        }
    }

    void SetLineAlpha(float alpha)
    {
        if (line == null && (line.materials == null || line.materials.Length == 0))
        {
            return;
        }

        Color curColor = !stopAnimSign ? aimColor : aimDoneColor;
        Color emissionColor = new Color(curColor.r, curColor.g, curColor.b, 1) * 5.0f;
        emissionColor.a = alpha;
        curColor.a = alpha;
        int colorPropertyId = Shader.PropertyToID("_Color");
        int emissionColorId = Shader.PropertyToID("_EmissionColor");
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        propertyBlock.SetColor(colorPropertyId, curColor);
        propertyBlock.SetColor(emissionColorId, emissionColor);
        line.SetPropertyBlock(propertyBlock);
    }

    void StopAimAnim()
    {
        ResetCoroutine(aimLineAnimCoroutine);
    }

    void ResetCoroutine(Coroutine c)
    {
        if (c == null) return;
        StopCoroutine(c);
    }

    #region external_method

    /// <summary>
    /// 初期化
    /// </summary>
    public void DoInit()
    {
        ActiveLine(false);

        if (aimAnimCurve != null && aimAnimCurve.length >= 2)
        {
            animInterval = aimAnimCurve.keys[aimAnimCurve.keys.Length - 1].time;
        }
    }

    public void SetBasePoint(Vector3 point,bool update=false)
    {
        basePoint = point;
        if (update)
        {
            SetLinePos(0, basePoint);
        }
    }

    public void SetAimPoint(Vector3 point, bool update = false)
    {
        aimPoint = point;
        if (update)
        {
            UpdateLineRaycast();
            SetLinePos(1, aimPoint);
        }
    }

    public void ActiveLine(bool active)
    {
        if (line == null) return;

        if (active)
        {
            line.SetPosition(0, basePoint);
            line.SetPosition(1, aimPoint);
        }
        else
        {
            line.SetPosition(0, basePoint);
            line.SetPosition(1, basePoint);
        }

        line.enabled = active;
        lineActive = active;
    }

    public void ActiveAimAnim(bool active ,bool forceInvoke = false)
    {
        if (animActive == active && forceInvoke == false) return;

        if (active)
        {
            StopAimAnim();
            aimLineAnimCoroutine = StartCoroutine(AimLineAnim());
        }
        else
        {
            StopAimAnim();
        }

        animActive = active;
    }

    public void SetStopAnimSign()
    {
        stopAnimSign = true;
    }

    #endregion external_method

    #region other_method

    IEnumerator AimLineAnim()
    {
        if (aimAnimCurve == null)
        {
            Debug.LogError("[AimingLine]Aim AnimationCurve is null,start aim anim failed");
            yield break;
        }
        ActiveLine(true);

        animTimePass = 0;
        curAnimValue = 0;
        stopAnimSign = false;

        SetLineAlpha(0);

        while (stopAnimSign == false)
        {
            animTimePass += Time.deltaTime;
            if (animTimePass >= animInterval)
            {
                animTimePass = 0;
            }

            curAnimValue = Mathf.Clamp01(aimAnimCurve.Evaluate(animTimePass));

            SetLineAlpha(curAnimValue);

            yield return new WaitForEndOfFrame();
        }

        SetLineAlpha(1);

        yield break;
    }


    #endregion other_method
}
