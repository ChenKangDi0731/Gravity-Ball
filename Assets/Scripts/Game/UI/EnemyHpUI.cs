using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    public Canvas uiCanvas;//キャンパス
    public Slider hpSlider;//HPスライダー

    [SerializeField] Vector3 followOffset = Vector3.zero;
    Transform followObjT;
    [SerializeField] string owner;
    private void Start()
    {
        //初期化
        if (uiCanvas != null)
        {
            uiCanvas.worldCamera = CameraManager.Instance.MainCamera;
        }

        if (hpSlider != null)
        {
            hpSlider.value = 1.0f;
        }
    }

    private void Update()
    {
        if (followObjT != null)
        {
            this.transform.position = followObjT.position + followOffset;
        }
        if (CameraManager.Instance.MainCamera != null)
        {
            this.transform.LookAt(CameraManager.Instance.MainCamera.transform);
        }
    }

    /// <summary>
    /// スライダーの値を設定する　0～1まで
    /// </summary>
    /// <param name="value"></param>
    public void SetHpValue(float value)
    {
        if (hpSlider == null) return;
        value = Mathf.Clamp01(value);

        hpSlider.value = value;
    }

    public void SetFollowObj(Transform objTransform)
    {
        owner = objTransform.name;
        followObjT = objTransform;
    }

    public void SetPosOffset(Vector3 offset)
    {
        followOffset = offset;
    }
}
