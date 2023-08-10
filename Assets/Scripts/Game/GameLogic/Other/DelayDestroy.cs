//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// 自身を自動的に削除する
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDestroy : MonoBehaviour
{
    public float delayTime;
    float timePass;
    [SerializeField]bool startCount = false;
    // Start is called before the first frame update
    void Start()
    {
        timePass = 0;
        startCount = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (startCount == true)
        {
            timePass += Time.deltaTime;
            if (timePass >= delayTime)
            {
                DestroyImmediate(this.gameObject);
            }
        }

    }

    public void StartDestroy()
    {
        startCount = true;
        timePass = 0;
    }

    public void StopDestroy()
    {
        startCount = false;
        timePass = 0;
    }
}
