//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 星アイテムのスクリプト
/// ‐Item親クラスから継承
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Star : Item
{
    [SerializeField]
    [Range(0,1)] float stateOffAlpha = 0.5f;//獲得できない時半透明にする
    [SerializeField]
    [Range(0, 1)] float stateOnAlpha = 1.0f;//獲得できる時は不透明に設定する
    [SerializeField]
    [Range(0, 50)] float stateOffEmission = 0.2f;//獲得できない時Emissionの強さ
    [SerializeField]
    [Range(0, 50)] float stateOnEmission = 50.0f; //獲得できる時Emissionの強さ

    [SerializeField] GameObject effectPrefabs;//プレイヤーに拾われてから生成されるエフェクト
    //[SerializeField] float effectDelayDestroyTime = 3.0f;//一定時間後エフェクトが自動的に削除する

     Color emissionColor;
    MeshRenderer ms;

    public override void DoInit()
    {
        if (ms == null)
        {
            ms = this.gameObject.GetComponent<MeshRenderer>();
            if (ms.material != null)
            {
                emissionColor = ms.sharedMaterial.GetColor("_EmissionColor");
            }
        }
        base.DoInit();
    }

    public override void SetState(bool state)
    {
        //獲得できない状態であれば、不透明度を変える
        if (ms != null && ms.material!=null)
        {
            Color tempColor = ms.material.GetColor("_Color");
            Color eColor = ms.material.GetColor("_EmissionColor");

            tempColor.a = state ? stateOnAlpha : stateOffAlpha;
            float eFactor = state ? stateOnEmission : stateOffEmission;
            eColor.r *= eFactor;
            eColor.g *= eFactor;
            eColor.b *= eFactor;
            eColor.a = tempColor.a;

            int colorId = Shader.PropertyToID("_Color");
            int emissionColorId = Shader.PropertyToID("_EmissionColor");
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor(colorId, tempColor);
            propertyBlock.SetColor(emissionColorId, eColor);
            ms.SetPropertyBlock(propertyBlock);

        }
        this.ActiveCollider(state);
        base.SetState(state);
    }

    public override void TriggerEnter(Collider other)
    {
        if(other==null || other.gameObject.tag != "Player")
        {
            return;
        }
        SceneMgr.Instance.curScene.ChangeItemCount(1);

        if (effectPrefabs != null)
        {
            GameObject effect = GameObject.Instantiate(effectPrefabs);
            if (effect != null)
            {
                effect.transform.position = this.transform.position;

                DelayDestroy dd = effect.AddComponentOnce<DelayDestroy>();
                if (dd != null)
                {
                    dd.delayTime = 3.0f;
                    dd.StartDestroy();
                }
                effect.MapAllComponent<ParticleSystem>((ps) =>
                {
                    if (ps != null)
                    {
                        ps.Play();
                    }
                });
            }
        }

        Show(false);//プレイヤーに拾われ、非表示に設定する
        AudioManager.Instance.PlaySound( E_SoundType.Item, true);//サウンドの再生
    }

    public void OnStateActive()
    {
        SetState(true);
    }

}
