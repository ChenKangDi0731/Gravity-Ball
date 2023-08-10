using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy_Diamond : MonoBehaviour
{

    public enum E_SimpleEnemyType
    {
        ShootHorizontal,
        ShootPlayer,
    }

    public E_SimpleEnemyType enemyType = E_SimpleEnemyType.ShootHorizontal;

    public GameObject bulletPrefabs;
    public GameObject playerObj;
    public Collider c;

    public Transform shootPoint;
    public int defaultHP;
    public int curHP;

    //attack param
    public float baseShootInterval;
    public float shootInterval;
    float shootTimePass;
    public float bulletForce;

    [Header("Be Detected Param")]
    public Color baseColor;
    public Color beDetectedColor;
    bool beDetected = false;
    public float resetColorTime = 1f;
    public float resetColorTimePass = 0;
    public float baseOutline;
    public float selectOutline;
    //oother param
    public UnityEvent onDisable;
    public UnityEvent onDestroy;


    private void Awake()
    {
        if (c == null) c = gameObject.GetComponent<Collider>();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        curHP = defaultHP;
        ActiveCollider(true);
    }

    private void Update()
    {
        shootTimePass += Time.deltaTime;
        if (shootTimePass >= shootInterval)
        {
            ShootBullet();
        }

        if (beDetected)
        {
            resetColorTimePass += Time.deltaTime;
            if (resetColorTimePass >= resetColorTime)
            {
                beDetected = false;
                resetColorTimePass = 0;
                SetOutline(false, baseColor);
            }
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(30, 30, 200, 30), curHP + " / " + defaultHP);
    }

    void ShootBullet()
    {
        if (bulletPrefabs == null || playerObj == null)
        {
            return;
        }

        Vector3 dir2Player = Vector3.zero;
        Transform shootTransform;
        if (shootPoint == null)
        {
            shootTransform = this.transform;
            dir2Player = playerObj.transform.position - this.transform.position;
        }
        else
        {
            shootTransform = shootPoint;
            dir2Player = playerObj.transform.position - shootPoint.position;
        }

        GameObject bullet = GameObject.Instantiate(bulletPrefabs);
        if (bullet == null)
        {
            Debug.LogError("Create bullet failed");
            return;
        }

        bullet.transform.position = shootTransform.position;
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb == null)
        {
            Debug.LogError("Get bullet Rigidbody component failed");
            DestroyImmediate(bullet);
            return;
        }

        switch (enemyType)
        {
            case E_SimpleEnemyType.ShootPlayer:
                bulletRb.AddForce(dir2Player.normalized * bulletForce, ForceMode.Impulse);
                break;
            case E_SimpleEnemyType.ShootHorizontal:
                bulletRb.AddForce(shootTransform.right * bulletForce, ForceMode.Impulse);
                break;
            default:
                break;
        }

        //calc next interval
        shootInterval = baseShootInterval + UnityEngine.Random.Range(-0.5f, 0.5f);
        shootTimePass = 0;

    }

    public void Damage(int value)
    {
        curHP -= value;
        //Debug.LogWarning("Enemy Hp = " + curHP);
        if (curHP <= 0)
        {
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        OnEnemyDisable();
        this.gameObject.SetActive(false);
    }

    void ActiveCollider(bool active)
    {
        if (c == null) return;
        c.enabled = active;
    }

    void SetOutline(bool active, Color color)
    {
        float outline = active == false ? baseOutline : selectOutline;
        this.gameObject.MapAllComponent<Renderer>(r =>
        {
            if (r != null)
            {
                if (r.materials != null && r.materials.Length != 0)
                {
                    for (int index = 0; index < r.materials.Length; index++)
                    {
                        if (r.materials[index] == null) continue;
                        //r.materials[index].color = color;
                        int outlineColorPropertyId = Shader.PropertyToID("_OutlineColor");
                        int outlinePropertyId = Shader.PropertyToID("_Outline");
                        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                        propertyBlock.SetFloat(outlinePropertyId, outline);
                        propertyBlock.SetColor(outlineColorPropertyId, color * 3);
                        r.SetPropertyBlock(propertyBlock);
                        //r.material.SetColor("_Color", color);
                    }
                }
            }
        });
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (shootPoint == null) return;

        switch (enemyType)
        {
            case E_SimpleEnemyType.ShootHorizontal:
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(shootPoint.position, shootPoint.position + shootPoint.right * 20);
                break;
            case E_SimpleEnemyType.ShootPlayer:
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(shootPoint.position, playerObj.transform.position);
                break;
            default:
                break;
        }
    }

#endif

    #region other_method

    public void DetectEnterCB()
    {
        if (beDetected == false)
        {
            beDetected = true;
            SetOutline(true, beDetectedColor);
        }
        resetColorTimePass = 0;
    }

    public void DetectExitCB()
    {
        if (beDetected)
        {
            SetOutline(true, baseColor);
        }
        beDetected = false;
    }
    public void OnEnemyDestroy()
    {
        onDestroy?.Invoke();
    }

    public void OnEnemyDisable()
    {
        onDisable?.Invoke();
    }

    #endregion other_method
}
