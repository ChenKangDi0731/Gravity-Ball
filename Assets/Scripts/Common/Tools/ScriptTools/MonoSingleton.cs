using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject temp = GameObject.Find("Main");
                if (temp == null)
                {
                    temp = new GameObject("script_" + typeof(T).Name);
                }
                if (temp != null)
                {
                    instance = temp.AddComponentOnce<T>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
