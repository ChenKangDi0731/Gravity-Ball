///////////////////////////////////////////////////////////////////
///
/// 照星
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Crosshair : UIBase
{
    public GameObject rootObj;

    public Image left;
    public Image right;
    public RectTransform leftOffsetRT;
    public RectTransform rightOffsetRT;
    [SerializeField] Image forbiddenImage;//Xの画像（ボール生成できない時に表示する
    [SerializeField] Text lb_buttonText;//操作ヒント；マウス左クリック（移動床に当たった時に表示する
    [SerializeField] Text f_buttonText;//操作ヒント；Fキー

    //エイムモードに切り替え時のアニメーションのパラメータ
    public float baseAimScaleTime;
    public float aimTime;
    public float timePass = 0;
    public Vector3 leftDefaultPos;
    public Vector3 rightDefaultPos;
    public Vector3 aimLeftOffset;
    public Vector3 aimRightOffset;
    public float defaultWidth;
    public float aimWidthOffset;

    public Color baseColor;//デフォルト色
    public Color color_1;//オブジェクトに当たった時の色

    public bool aimMode;
    public bool aimAnimEnd = false;
    public bool colorChange = true;
    Tweener aimLeftTweener;
    Tweener aimRightTweener;

    #region lifeCycle_method

    private void Awake()
    {
        if (rootObj == null) rootObj = this.gameObject;

        if(left==null || right == null)
        {
            Debug.LogError("[Crosshair]sprite is null");
            return;
        }
        leftDefaultPos = left.rectTransform.anchoredPosition3D;
        rightDefaultPos = right.rectTransform.anchoredPosition3D;

        aimLeftOffset = leftDefaultPos;
        aimRightOffset = rightDefaultPos;

        aimLeftOffset.x += aimWidthOffset;
        aimRightOffset.x -= aimWidthOffset;

        aimAnimEnd = false;
        aimTime = baseAimScaleTime;
        timePass = 0;

        ChangeColor(false);
        SetForbiddenState(false);
        SetButtonTextState(E_CrosshairPart.LB_Text, false, true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //エイムモード
        if (Input.GetMouseButtonDown(1))
        {
            EnterAimMode(true);
        }

        //ノーマルモード
        if (Input.GetMouseButtonUp(1))
        {
            EnterAimMode(false);
        }

        if (aimMode && aimAnimEnd == false)
        {
            if (timePass < aimTime)
            {
                timePass += Time.deltaTime;
            }
            else
            {
                aimAnimEnd = true;
                timePass = aimTime;
            }

        }
    }

    void ResetTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
    }

    #endregion lifeCycle_method

    #region external_method

    public override void DoInit()
    {
        base.DoInit();
    }

    /// <summary>
    /// エイムモードに切り替え（照星
    /// </summary>
    /// <param name="enter"></param>
    public void EnterAimMode(bool enter)
    {
        if (aimMode == enter || left==null || right==null) return;


        ResetTweener(aimLeftTweener);
        ResetTweener(aimRightTweener);
        if (aimAnimEnd)
        {
            aimTime = baseAimScaleTime;
        }
        else
        {
            aimTime = baseAimScaleTime - timePass;
        }
        aimAnimEnd = false;
        timePass = 0;

        if (enter)
        {
            aimMode = true;

            aimLeftTweener = left.rectTransform.DOAnchorPos3DX(aimLeftOffset.x , aimTime);
            aimRightTweener = right.rectTransform.DOAnchorPos3DX(aimRightOffset.x, aimTime);
        }
        else
        {
            aimMode = false;

            aimLeftTweener = left.rectTransform.DOAnchorPos3DX(leftDefaultPos.x, aimTime);
            aimRightTweener = right.rectTransform.DOAnchorPos3DX(rightDefaultPos.x, aimTime);
        }
    }

    /// <summary>
    /// 色を変える
    /// </summary>
    /// <param name="state"></param>
    public void ChangeColor(bool state)
    {
        if (left == null || right == null || state ==colorChange) return;
        if (state)
        {
            left.color = color_1;
            right.color = color_1;
        }
        else
        {
            left.color = baseColor;
            right.color = baseColor;
        }
        colorChange = state;
    }

    /// <summary>
    /// XのUIを表示（ボール生成できない時に
    /// </summary>
    /// <param name="forbiddenState"></param>
    public void SetForbiddenState(bool forbiddenState)
    {
        if (forbiddenImage == null) return;
        forbiddenImage.enabled = forbiddenState;
    }

    /// <summary>
    /// 操作ヒントUIを表示
    /// </summary>
    /// <param name="part"></param>
    /// <param name="state"></param>
    /// <param name="both"></param>
    public void SetButtonTextState(E_CrosshairPart part,bool state,bool both = false)
    {
        if(part == E_CrosshairPart.LB_Text)
        {
            if (lb_buttonText != null)
            {
                lb_buttonText.enabled = state;
            }

            if (f_buttonText != null)
            {
                f_buttonText.enabled = (!state)^both;
            }
        }
        else if(part ==E_CrosshairPart.F_Text)
        {
            if (f_buttonText != null)
            {
                f_buttonText.enabled = state;
            }

            if (lb_buttonText != null)
            {
                lb_buttonText.enabled = (!state) ^ both;
            }
        }
    }

    #endregion external_method
}
