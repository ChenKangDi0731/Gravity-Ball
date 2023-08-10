//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// サウンドマネージャー
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    Dictionary<int, AudioClip> fxDic;
    Dictionary<int, AudioClip> bgmDic;

    AudioSource fxSource;
    AudioSource bgmSource;

    int curFx;
    int curBgm;

    public void DoInit(AudioSource fx,AudioSource bgm)
    {
        if (fxDic == null) fxDic = new Dictionary<int, AudioClip>();
        if (bgmDic == null) bgmDic = new Dictionary<int, AudioClip>();

        fxSource = fx;
        bgmSource = bgm;

        fxDic.Clear();
        bgmDic.Clear();

        FillFxSoundData(GameConfig.Instance.fxDataList);
        FillBgmSoundData(GameConfig.Instance.bgmDataList);
    }

    public void FillFxSoundData(List<AudioData> fxList)
    {
        if (fxList == null) return;
        if (fxDic == null) fxDic = new Dictionary<int, AudioClip>();
        fxDic.Clear();

        for(int index = 0; index < fxList.Count; index++)
        {
            if (fxList[index] == null || fxList[index].clip == null) continue;
            int soundIndex = (int)fxList[index].soundType;

            if (fxDic.ContainsKey(soundIndex) == false)
            {
                fxDic.Add(soundIndex, fxList[index].clip);
            }
        }
    }

    public void FillBgmSoundData(List<AudioData> bgmList)
    {
        if (bgmList == null) return;
        if (bgmDic == null) bgmDic = new Dictionary<int, AudioClip>();
        bgmDic.Clear();

        for (int index = 0; index < bgmList.Count; index++)
        {
            if (bgmList[index] == null || bgmList[index].clip == null) continue;
            int soundIndex = (int)bgmList[index].soundType;

            if (bgmDic.ContainsKey(soundIndex) == false)
            {
                bgmDic.Add(soundIndex, bgmList[index].clip);
            }
        }
    }

    /// <summary>
    /// サウンド再生
    /// </summary>
    /// <param name="soundType"></param>
    /// <param name="addition"></param>
    public void PlaySound(E_SoundType soundType,bool addition = true)
    {
        int soundIndex = (int)soundType;
        if(fxDic==null || fxDic.ContainsKey(soundIndex) == false || fxDic[soundIndex] == null)
        {
            Debug.LogError("play sound fauled : " + soundType);
            return;
        }

        if (addition == true || fxSource == null)
        {
            AudioSource.PlayClipAtPoint(fxDic[soundIndex], CameraManager.Instance.MainCamera.transform.position);
        }
        else
        {
            fxSource.Stop();

            fxSource.clip = fxDic[soundIndex];
            fxSource.Play();
        }

    }

    /// <summary>
    /// BGM再生
    /// </summary>
    /// <param name="soundType"></param>
    public void PlayBGM(E_SoundType soundType)
    {
        int soundIndex = (int)soundType;
        if (bgmDic == null || bgmDic.ContainsKey(soundIndex) == false || bgmDic[soundIndex] == null)
        {
            Debug.LogError("play sound fauled : " + soundType);
            return;
        }

        if (bgmSource == null)
        {
            AudioSource.PlayClipAtPoint(bgmDic[soundIndex], CameraManager.Instance.MainCamera.transform.position);
        }
        else
        {
            bgmSource.Stop();

            bgmSource.clip = bgmDic[soundIndex];
            bgmSource.Play();
        }
    }

    public void StopSound()
    {
        if (fxSource != null)
        {
            fxSource.Stop();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void StopAllSound()
    {
        StopSound();
        StopBGM();
    }
}

[System.Serializable]
public class AudioData
{
    public E_SoundType soundType;
    public AudioClip clip;
}