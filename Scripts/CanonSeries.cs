using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonSeries : MonoBehaviour {
    
    public GameObject ProjectileToShoot;
    public float ShotDelay = 1.0f;
    public bool FireOnSpawn = false;
    public GameObject FireSound;
    public GameObject Muzzle;
    private Transform muzzleTrans;

    public float SweepPitch = 0.0f;
    public float SweepYaw = 40.0f;
    public float DepthScale = 1.0f;
    public float TimeAlive = 3.0f;

    private GameObject spawner;
    private Spawner spawnerScript;
    private Vector3 initRotEuler;
    private Vector3 finalRotEuler;
    private Quaternion finalRot;
    private float lastFireTime;
    private float spawnTime;

    private GameObject target;
	
	void Start () {
        muzzleTrans = Muzzle.transform;
        spawner = GameObject.FindGameObjectWithTag("Spawner");
        spawnerScript = spawner.GetComponent<Spawner>();
        /*initPos = transform.position;
        initRotEuler = transform.rotation.eulerAngles;
        initRot = transform.rotation;
        finalRot = Quaternion.Euler(initRotEuler.x + SweepPitch, initRotEuler.y + SweepYaw, initRotEuler.z);
        finalRotEuler = finalRot.eulerAngles;
        lastFireTime = spawnTime = Time.fixedTime;*/
        transform.rotation = Quaternion.LookRotation(spawner.transform.position - muzzleTrans.position);
        target = GameObject.Find("ForwardDirection");
        if (FireOnSpawn)
        {
            Fire();
        }
    }

    private Vector3 snapVectorTo180(Vector3 v)
    {
        if (v.x > 180)
            v.x = v.x - 360;
        //if (v.y > 180)
        //    v.y = v.y - 360;
        return v;
    }

    public void Init(float sPitch, float sYaw, float dScale, float tAlive, float sDelay)
    {
        SweepPitch = sPitch;
        SweepYaw = sYaw;
        DepthScale = dScale;
        TimeAlive = tAlive;
        ShotDelay = sDelay;


        initRotEuler = transform.rotation.eulerAngles;
        finalRot = Quaternion.Euler(initRotEuler.x + SweepPitch, initRotEuler.y + SweepYaw, initRotEuler.z);
        finalRotEuler = finalRot.eulerAngles;
        initRotEuler = snapVectorTo180(initRotEuler);
        finalRotEuler = snapVectorTo180(finalRotEuler);
        lastFireTime = spawnTime = Time.fixedTime;
        //Debug.Log("initRotEuler: " + initRotEuler);
        //Debug.Log("finalRotEuler: " + finalRotEuler);
    }
	
	private void tickMovement(out float lerpFactor)
    {
        lerpFactor = Mathf.Min((Time.fixedTime - spawnTime) / TimeAlive, 1.0f);
        float x = Mathf.Lerp(0, finalRotEuler.x - initRotEuler.x, lerpFactor);
        float y = Mathf.Lerp(0, finalRotEuler.y - initRotEuler.y, lerpFactor);
        Vector3 p = spawnerScript.GetSpawnLocationFromAngle(initRotEuler.x + x, (initRotEuler.y * -1) - y, DepthScale);
        p.z *= -1;
        transform.position = p;
        transform.rotation = Quaternion.LookRotation(spawner.transform.position - muzzleTrans.position);
    }

	void FixedUpdate () {
        float lerpFactor;
        tickMovement(out lerpFactor);
		if(canFire())
        {
            Fire();
        }
        if(lerpFactor == 1.0f)
        {
            Object.Destroy(this.gameObject);
        }
	}

    private bool canFire()
    {
        return Time.fixedTime - lastFireTime >= ShotDelay;
    }

    private void Fire()
    {
        Instantiate(FireSound, muzzleTrans.position, muzzleTrans.rotation);
        Instantiate(ProjectileToShoot, muzzleTrans.position, muzzleTrans.rotation);
        lastFireTime = Time.fixedTime;
    }
}
