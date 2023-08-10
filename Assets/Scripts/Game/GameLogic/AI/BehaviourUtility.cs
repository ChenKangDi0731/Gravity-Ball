using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BehaviourUtility
{
    public static Vector3 GetRandomPos(Vector3 basePos,float maxRadius, float minRadius = 0, float yValue = 0)
    {

        Vector3 dir = Vector3.zero;
        dir.x = UnityEngine.Random.Range(-1.0f, 1.0f);
        dir.y = yValue;
        dir.z = UnityEngine.Random.Range(-1.0f, 1.0f);

        dir = Vector3.Normalize(dir);

        float dist = UnityEngine.Random.Range(minRadius, maxRadius);

        return basePos + dir * dist;
    }
    
    public static Vector3 GetRandomForward(Vector3 curForward, float minAngle, float maxAngle)
    {
        float angle = UnityEngine.Random.Range(minAngle, maxAngle);

        curForward = Quaternion.Euler(new Vector3(0, angle, 0)) * curForward;
        return curForward;
    }

}
