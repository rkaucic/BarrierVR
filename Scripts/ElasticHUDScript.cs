using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElasticHUDScript : MonoBehaviour {

    public GameObject RotationAnchor;
    public float LinearForce = 10.0f;
    public float MinForce = 0.5f;

    private Transform rotAnchor;

	void Start () {
        rotAnchor = RotationAnchor.transform;
	}
	
	void Update () {
        float targetRot = rotAnchor.rotation.eulerAngles.y;
        float curRot = transform.rotation.eulerAngles.y;
        float rotOffset = targetRot - curRot;
        if(Mathf.Abs(rotOffset) > 180.0f)
        {
            if(rotOffset < 0)
            {
                rotOffset = (360.0f - curRot) + targetRot;
            }
            else
            {
                rotOffset = ((360.0f - targetRot) + curRot) * -1;
            }
        }
        float force = rotOffset * LinearForce;
        if (Mathf.Abs(force) < MinForce)
            force = force < 0 ? MinForce * -1 : MinForce;
        force *= Time.deltaTime;
        if (Mathf.Abs(force) > Mathf.Abs(rotOffset))
        {
            transform.rotation = rotAnchor.rotation;
        }
        else
        {
            transform.Rotate(new Vector3(0.0f, force, 0.0f));
        }
	}
}
