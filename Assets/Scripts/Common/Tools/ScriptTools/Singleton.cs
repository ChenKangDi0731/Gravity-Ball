using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Singleton<T> where T:new()
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }

    static bool initOnce = false;

    protected Singleton()
    {
        if (initOnce) return;

        if (instance == null)
        {
            initOnce = true;

            T local = default(T);
            Singleton<T>.instance = (local == null) ? System.Activator.CreateInstance<T>() : local;
        }
    }
}

