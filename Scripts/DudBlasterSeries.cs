using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DudBlasterSeries : MonoBehaviour {

    public float XSkew;
    public float YSkew;
    public float ZSkew;
    public float AngleSkew;
    public float SweepTime;
    public float IdleTime;
    public GameObject SweepSound;

    private float phaseStartTime;
    private bool isSweeping;
    private bool isIdle;
    private bool isFading;

    private Vector3 initPos;
    private Quaternion initRot;
    private Vector3 destPos;
    private Quaternion destRot;
    
    private Transform muzzleTrans;

    private SpriteRenderer sprite;

    public void OverrideTimes(float sweep, float idle)
    {
        SweepTime = sweep;
        IdleTime = idle;
    }

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        destPos = transform.position;
        destRot = transform.rotation;
        transform.Translate(new Vector3(XSkew, YSkew, ZSkew));
        transform.Rotate(new Vector3(0.0f, 0.0f, AngleSkew));
        initPos = transform.position;
        initRot = transform.rotation;
        isIdle = false;
        isSweeping = true;
        muzzleTrans = transform.Find("LaserMuzzle");
        phaseStartTime = Time.fixedTime;
        Instantiate(SweepSound, muzzleTrans.position, muzzleTrans.rotation);
    }
    
    void Update()
    {
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
        if (isIdle)
        {
            if (Time.fixedTime >= phaseStartTime + IdleTime)
            {
                Object.Destroy(muzzleTrans.gameObject);
                isFading = true;
                isIdle = false;
                phaseStartTime = Time.fixedTime;
            }
        }
        else if (isFading)
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
}
