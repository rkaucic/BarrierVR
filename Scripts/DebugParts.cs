using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugParts : MonoBehaviour {
    public float Radius = 10.0f;

	void Start () {
        var shape = this.transform.Find("DebugParticles").GetComponent<ParticleSystem>().shape;
        shape.radius = Radius;
	}
	
	void Update () {
		
	}
}
