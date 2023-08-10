//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// HUD管理スクリプト
/// ‐ステータスUI
/// ‐ステージ目標UI
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : UIBase
{
    [Header("ステータス")]
    [SerializeField] Slider hpSlider;//Hp
    [SerializeField] List<Image> fpList;//無重力ボールUI
    [SerializeField] List<Image> apList;//重力ボールUI

    [SerializeField]float maxHp = 100;//最大HP
    float curHp = 100;//今のHP

    int curFpCount = 0;//今使える無重力ボールの数
    int curApCount = 0;//今使える重力ボールの数

    [Header("アイテム")]
    [SerializeField] Text curCountText;//今入手したアイテムの数
    [SerializeField] Text targetCountText;//最大アイテム数

    [Header("ガイド")]
    [SerializeField] bool guideDefaultShow;
    [SerializeField] GameObject guideShowOnButton;
    [SerializeField] GameObject guideShowOffButton;
    [SerializeField] GameObject guidePanel;
    bool isGuideShow = true;


    /// <summary>
    /// 初期化
    /// </summary>
    public override void DoInit()
    {
        base.DoInit();
        if (fpList != null)
        {
            curFpCount = fpList.Count;
        }
        if (apList != null)
        {
            curApCount = apList.Count;
        }

        curHp = maxHp;

        ShowGuide(guideDefaultShow);
    }

    /// <summary>
    /// HPのUIを更新
    /// </summary>
    private void UpdateHp()
    {
        if (hpSlider == null || maxHp == 0) return;

        hpSlider.value = (float)curHp / maxHp;
    }

    /// <summary>
    /// 無重力ボールのUIを更新
    /// </summary>
    private void UpdateFlowPointUI()
    {
        if (fpList == null) return;

        for(int index = fpList.Count - 1; index>=0; index--)
        {
            if (fpList[index] != null)
            {
                if (index <= curFpCount - 1)
                {
                    fpList[index].gameObject.SetActive(true);
                }
                else
                {
                    fpList[index].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 重力ボールのUIを更新
    /// </summary>
    private void UpdateAttractPointUI()
    {
        if (apList == null) return;

        for (int index = apList.Count - 1; index >=0 ; index--)
        {
            if (apList[index] != null)
            {
                if (index <= curApCount - 1)
                {
                    apList[index].gameObject.SetActive(true);
                }
                else
                {
                    apList[index].gameObject.SetActive(false);
                }
            }
        }
    }

    #region external_method

    /// <summary>
    /// 今のHPを変える
    /// </summary>
    /// <param name="changeValue"></param>
    public void ChangeHp(int changeValue)
    {
        curHp = (int)Mathf.Clamp(curHp + changeValue, 0, maxHp);
        UpdateHp();
    }

    /// <summary>
    /// HPの数値をリセット
    /// </summary>
    public void ResetHp()
    {
        curHp = maxHp;
        UpdateHp();
    }

    /// <summary>
    /// 無重力ボールの数を変える
    /// </summary>
    /// <param name="count"></param>
    public void SetFlowPointCount(int count)
    {
        curFpCount = (int)(Mathf.Clamp(count, 0, fpList.Count) + 0.5f);
        UpdateFlowPointUI();
    }

    /// <summary>
    /// 重力ボールの数を変える
    /// </summary>
    /// <param name="count"></param>
    public void SetAttractPointCount(int count)
    {
        curApCount = (int)(Mathf.Clamp(count, 0, apList.Count) + 0.5f);
        UpdateAttractPointUI();
    }

    /// <summary>
    /// アイテム数を変える
    /// </summary>
    /// <param name="count"></param>
    public void SetCurStarCount(int count)
    {
        if (curCountText != null)
        {
            string tempStr = string.Empty;
            if((count/10) == 0)
            {
                tempStr = "0" + count.ToString();
            }
            else
            {
                tempStr = count.ToString();
            }
            curCountText.text = tempStr; 
        }
    }

    /// <summary>
    /// アイテムの数の最大値を変える
    /// </summary>
    /// <param name="count"></param>
    public void SetTargetStarCount(int count)
    {
        if (targetCountText != null)
        {
            string tempStr = string.Empty;
            if ((count / 10) == 0)
            {
                tempStr = "0" + count.ToString();
            }
            else
            {
                tempStr = count.ToString();
            }
            targetCountText.text = tempStr;
        }
    }

    /// <summary>
    ///　ガイドを表示/非表示
    /// </summary>
    /// <param name="show"></param>
    public void ShowGuide(bool show)
    {
        if(guideShowOnButton == null || guideShowOffButton == null || guidePanel == null)
        {
            return;
        }

        isGuideShow = show;

        guideShowOnButton.SetActive(!isGuideShow);
        guideShowOffButton.SetActive(isGuideShow);
        guidePanel.SetActive(isGuideShow);
    }

    public bool CheckGuideShow()
    {
        return isGuideShow;
    }

    public bool IsPlayerDead()
    {
        return curHp <= 0;
    }

    #endregion external_method
}
