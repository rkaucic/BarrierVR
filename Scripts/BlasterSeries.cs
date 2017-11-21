using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlasterSeries : MonoBehaviour {

    public float XSkew;
    public float YSkew;
    public float ZSkew;
    public float AngleSkew;
    public float SweepTime;
    public float IdleTime;
    public float BlastTime;
    public GameObject LaserObject;
    public GameObject SweepSound;
    public GameObject BlastSound;

    private float phaseStartTime;
    private bool isSweeping;
    private bool isIdle;
    private bool isFiring;
    private bool isFading;

    private Vector3 initPos;
    private Quaternion initRot;
    private Vector3 destPos;
    private Quaternion destRot;

    private GameObject laser;
    private Transform muzzleTrans;

    private SpriteRenderer sprite;

	void Start () {
        sprite = GetComponent<SpriteRenderer>();
        destPos = transform.position;
        destRot = transform.rotation;
        transform.Translate(new Vector3(XSkew, YSkew, ZSkew));
        transform.Rotate(new Vector3(0.0f, 0.0f, AngleSkew));
        initPos = transform.position;
        initRot = transform.rotation;
        isIdle = isFiring = false;
        if (SweepTime > 0.0f)
        {
            isSweeping = true;
        }
        else
        {
            isSweeping = false;
            isIdle = true;
            transform.position = destPos;
            transform.rotation = destRot;
        }
        muzzleTrans = transform.Find("LaserMuzzle");
        phaseStartTime = Time.fixedTime;
        Instantiate(SweepSound, muzzleTrans.position, muzzleTrans.rotation);
    }
	
	void Update () {
        if (isSweeping)
        {
            float lerpFactor = Mathf.Min((Time.fixedTime - phaseStartTime) / SweepTime, 1.0f);
            transform.position = Vector3.Lerp(initPos, destPos, lerpFactor);
            transform.rotation = Quaternion.Lerp(initRot, destRot, lerpFactor);
            if (lerpFactor == 1.0f)
            {
                isSweeping = false;
                isIdle = true;
                phaseStartTime = Time.fixedTime;
            }
        }
        if(isIdle)
        {
            if(Time.fixedTime >= phaseStartTime + IdleTime)
            {
                fire();
                isFiring = true;
                isIdle = false;
                phaseStartTime = Time.fixedTime;
            }
        }
        else if(isFiring)
        {
            if(Time.fixedTime - phaseStartTime >= BlastTime)
            {
                Object.Destroy(laser);
                Object.Destroy(muzzleTrans.gameObject);
                if(SweepTime <= 0.0f)
                {
                    SweepTime *= -1;
                }
                isFading = true;
                isFiring = false;
                phaseStartTime = Time.fixedTime;
            }
        }
        else if(isFading)
        {
            float lerpFactor = Mathf.Min((Time.fixedTime - phaseStartTime) / SweepTime, 1.0f);
            transform.position = Vector3.Lerp(destPos, initPos, lerpFactor);
            transform.rotation = Quaternion.Lerp(destRot, initRot, lerpFactor);
            sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - lerpFactor);
            if (lerpFactor == 1.0f)
            {
                Object.Destroy(this.gameObject);
            }
        }
	}

    private void fire()
    {
        Instantiate(BlastSound, muzzleTrans.position, muzzleTrans.rotation);
        laser = (GameObject)Instantiate(LaserObject, muzzleTrans.position, muzzleTrans.rotation);
    }
}
