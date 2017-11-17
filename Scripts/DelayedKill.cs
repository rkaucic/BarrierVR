using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedKill : MonoBehaviour {
    public float TimeAlive = 5.0f;
	void Start () {
        Invoke("kill", TimeAlive);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void kill()
    {
        Object.Destroy(this.gameObject);
    }
}
