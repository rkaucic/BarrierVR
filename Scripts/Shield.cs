using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public Material NormalMat;
    public Material OverlappedMat;

    private bool isOverlapped;
    private Renderer ren;
	
	void Start () {
        isOverlapped = false;
        ren = GetComponent<Renderer>();
    }
	
	
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.tag == "Shield")
        {
            isOverlapped = true;
            ren.material = OverlappedMat;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.tag == "Shield")
        {
            isOverlapped = false;
            ren.material = NormalMat;
        }
    }

    public bool IsOverlapping()
    {
        return isOverlapped;
    }
}
