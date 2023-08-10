///////////////////////////////////////////////////////////////////
///
/// オブジェクト自動的回転
/// 
///////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotateSelf : MonoBehaviour
{
    [SerializeField] E_Axis rotateAxis;//回転軸
    [SerializeField] float rotateSpeed = 2.0f;//回転スピード

    Vector3 xAxis;
    Vector3 yAxis;
    Vector3 zAxis;

    private void Awake()
    {
        //ローカル座標系の軸を獲得
        xAxis = this.transform.TransformDirection(Vector3.right);
        yAxis = this.transform.TransformDirection(Vector3.up);
        zAxis = this.transform.TransformDirection(Vector3.forward);
    }

    private void Update()
    {
        if(rotateAxis == E_Axis.XAxis)
        {
            this.transform.Rotate(xAxis, Time.deltaTime * rotateSpeed);
        }else if(rotateAxis == E_Axis.YAxis)
        {
            this.transform.Rotate(yAxis, Time.deltaTime * rotateSpeed);

        }
        else if(rotateAxis == E_Axis.ZAxis)
        {
            this.transform.Rotate(zAxis, Time.deltaTime * rotateSpeed);

        }
    }
}
