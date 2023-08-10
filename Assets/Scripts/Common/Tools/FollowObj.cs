using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObj : MonoBehaviour
{
    public float followSpeed;
    public float maxDistance;
    GameObject followObj;
    bool activeFollow = false;
    // Start is called before the first frame update

    private void OnEnable()
    {
        activeFollow = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(activeFollow)
        {
            if (followObj != null)
            {
                Vector3 dir2FollowObj = followObj.transform.position - transform.position;
                if (dir2FollowObj.sqrMagnitude > maxDistance * maxDistance)
                {
                    transform.position = followObj.transform.position + dir2FollowObj.normalized * maxDistance;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, followObj.transform.position, followSpeed * Time.deltaTime);
                }
            }
            else
            {
                SetFollowActive(false);
            }
        }
    }

    public void SetFollowObj(GameObject obj,float speed,float maxDist)
    {
        if (obj == null)
        {
            Debug.LogError("[FollowObj]Set followObj failed , obj is null , this.gameObject.name = " + this.gameObject.name);
            return;
        }

        followSpeed = speed;
        followObj = obj;
        maxDistance = maxDist;
    }

    public void SetFollowActive(bool active)
    {
        if (followObj == null)
        {
            activeFollow = false;
            return;
        }

        activeFollow = active;
    }
}
