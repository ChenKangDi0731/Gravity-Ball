using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create Zipper Mouth
/// </summary>
public class ElectricTurret_One : MonoBehaviour
{
    public GameObject createEnemyPrefab;
    public Transform createEnemyPoint;

    public float createInterval;
    public float timePass;

    public int createOrderCount;

    public float baseThrowYOffset;
    public Vector2 throwYOffsetRange;
    public float throwDirYRot;
    public Vector2 throwForceRange = Vector2.zero;
    //List<Enemy> childEnemyList;

    bool init;
    bool creating;
    bool startCreate
    {
        get
        {
            return init && createOrderCount > 0;
        }
    }

    Coroutine createCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        DoInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (startCreate)
        {
            timePass += Time.deltaTime;
            if (timePass >= createInterval)
            {
                timePass = 0;
                Create();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            CreateEnemy();
        }
    }

    void Create()
    {
        if (createEnemyPrefab == null)
        {
            ResetCreateOrder();
            return;
        }
        //ResetCoroutine();

        //create
        GameObject newGO = GameObject.Instantiate(createEnemyPrefab);
        if (newGO == null)
        {
            Debug.LogError("[ElectricTurret_One]Create enemy go failed");
            return;
        }
        Enemy e = newGO.GetComponent<Enemy>();
        if (e == null)
        {
            Debug.LogError("[ElectricTurret_One]Create enemy failed, get Enemy Component failed , go.name = " + newGO.name);
            return;
        }

        if (createEnemyPoint == null)
        {
            e.SetTransform(transform.position + new Vector3(0, 0.5f, 0), transform.forward);
            Debug.LogError(1);
        }
        else
        {
            Debug.LogError(2);
            e.SetTransform(createEnemyPoint.position, createEnemyPoint.forward);
        }
        e.ActiveGravity(false);
        e.ActiveCollider(false);
        e.BeCreated(enemy =>
        {
            if (enemy == null) return;
            if (transform == null) return;
            Vector3 forward = transform.forward;
            forward.y = baseThrowYOffset + UnityEngine.Random.Range(throwYOffsetRange.x, throwYOffsetRange.y);
            Vector3 throwDir = Quaternion.Euler(0, throwDirYRot, 0) * forward.normalized;

            float f = UnityEngine.Random.Range(throwForceRange.x, throwForceRange.y);
            enemy.ResetVelocity(true,true);
            enemy.AddForce(throwDir * f, ForceMode.Impulse);

            enemy.ActiveCollider(true);
            enemy.ActiveGravity(true);

        });
        createOrderCount = (createOrderCount - 1 < 0) ? 0 : createOrderCount - 1;
    }

    void ResetCoroutine()
    {
        if (createCoroutine!=null)
        {
            StopCoroutine(createCoroutine);
        }
    }

    #region external_method

    public void DoInit()
    {
        init = true;
        timePass = 0;
    }

    public void CreateEnemy()
    {
        createOrderCount++;
        Debug.LogWarning("order increase , count  = " + createOrderCount);
    }

    public void ResetCreateOrder()
    {
        createOrderCount = 0;
    }

    #endregion external_method


    #region debug_method

    public void OnDrawGizmos()
    {
        


    }

    #endregion debug_method
}
