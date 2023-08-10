//////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
/// プログラムエントリー
/// 
//////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    static Main instance;
    public static Main Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject temp = GameObject.Find("Main");
                if (temp != null)
                {
                    instance = temp.AddComponentOnce<Main>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        MainController.Instance.DoInit();
    }

    // Update is called once per frame
    void Update()
    {
        MainController.Instance.DoUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        MainController.Instance.DoFixedUpdate(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        MainController.Instance.DoLateUpdate(Time.deltaTime);
    }

    private void OnDestroy()
    {
        
    }
}
