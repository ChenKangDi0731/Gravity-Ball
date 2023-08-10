///////////////////////////////////////////////////////////////////
///
/// 移動床の親クラス
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class SpecialAnchor : MonoBehaviour
{

    public enum E_AnchorType
    {
        None=-1,
        Floating=0,
        Attract=1,
    }

    public E_AnchorType anchorType = E_AnchorType.None;//種類

    /// <summary>
    /// HDR色の強さ
    /// </summary>
    [SerializeField] [Range(0.0f, 10.0f)] float defaultHDRFactor;
    [SerializeField][Range(0.0f,10.0f)] float selectedHDRFactor;

    public bool isSelect = false;//狙われてるかどうか
    public bool anchorActive = false;
    public bool anchorActionActive = false;

    public virtual void Awake()
    {
        SelectAnchor(false, true);
    }

    // Update is called once per frame
    void Update()
    {
        AnchorUpdate(Time.deltaTime);
    }

    /// <summary>
    /// 狙われてる時に色を変える
    /// </summary>
    /// <param name="active"></param>
    void SetOutline(bool active)
    {
        this.gameObject.MapAllComponent<Renderer>(r =>
        {
            if (r != null)
            {
                if (r.materials != null && r.materials.Length != 0)
                {
                    for (int index = 0; index < r.materials.Length; index++)
                    {
                        if (r.materials[index] == null) continue;

                        Color tempColor = r.material.GetColor("_Color");
                        Color eColor = r.material.GetColor("_EmissionColor");

                        float eFactor = active ? selectedHDRFactor : defaultHDRFactor;
                        eColor.r *= eFactor;
                        eColor.g *= eFactor;
                        eColor.b *= eFactor;
                        eColor.a = tempColor.a;

                        int colorId = Shader.PropertyToID("_Color");
                        int emissionColorId = Shader.PropertyToID("_EmissionColor");
                        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                        propertyBlock.SetColor(colorId, tempColor);
                        propertyBlock.SetColor(emissionColorId, eColor);
                        r.SetPropertyBlock(propertyBlock);

                    }
                }
            }
        });
    }

    /// <summary>
    /// 狙われてるとき色を変える
    /// </summary>
    /// <param name="select"></param>
    /// <param name="forceInvoke"></param>
    public virtual void SelectAnchor(bool select,bool forceInvoke =false)
    {
        if ((isSelect == select ||anchorActive==true) && forceInvoke == false) return;
        isSelect = select;
        if (select)
        {
            SetOutline(true);
        }
        else
        {
            SetOutline(false);
        }
    }

    public virtual void ActiveAnchor(bool active)
    {
        if (anchorActive == false) return;

        anchorActive = active;

        SetOutline(active);
    }


    public virtual bool IsActive()
    {
        return anchorActive;
    }

    public virtual bool IsActionActive()
    {
        return anchorActionActive;
    }

    public abstract void ActiveAnchorAction(bool active, bool overwrite = false);
    public abstract void AnchorUpdate(float deltatime);
    public abstract void SetParam(params object[] paramList);
    public abstract void ResetAnchor();

    #region other_param

    public void StopTweener(Tweener t)
    {
        if (t == null) return;
        t.Kill();
    }

    #endregion other_param

}
