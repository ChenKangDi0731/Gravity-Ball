///////////////////////////////////////////////////////////////////
///
/// GameObject拡張函数
/// 
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    /// <summary>
    /// コンポーネントを追加（1つしか追加しない
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T AddComponentOnce<T>(this GameObject go) where T : MonoBehaviour
    {
        T returnComponent = go.GetComponent<T>();
        if(returnComponent != null)
        {
            MonoBehaviour.DestroyImmediate(returnComponent);
            returnComponent = null;
        }

        returnComponent = go.AddComponent<T>();
        return returnComponent;
    }

    /// <summary>
    /// 指定されたコンポーネントを取得してから処理する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="action"></param>
    /// <param name="includeRoot"></param>
    public static void MapAllComponent<T>(this GameObject go,Action<T> action,bool includeRoot = false)
    {
        if (go == null || action == null) return;

        T[] components;

        components = go.GetComponentsInChildren<T>();
        if(components == null || components.Length == 0)
        {
            return;
        }

        T rootComponent = go.GetComponent<T>();

        for(int index = 0; index < components.Length; index++)
        {
            if (components[index] == null) continue;

            action?.Invoke(components[index]);
        }
    }

    /// <summary>
    /// オブジェクトのActive状態を設定
    /// </summary>
    /// <param name="go"></param>
    /// <param name="active"></param>
    public static void SetGOActive(this GameObject go, bool active)
    {
        if (go == null) return;

        if(go.activeSelf != active)
        {
            go.SetActive(active);
        }
    }
}
