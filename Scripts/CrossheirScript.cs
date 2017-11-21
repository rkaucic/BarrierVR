using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossheirScript : MonoBehaviour {
    private AILibrary parentLibrary;
    private int posNegMod;
    private Vector3 origPos;
	void Start () {
        origPos = transform.position;
	}
	
	void Update () {
        if (parentLibrary == null)
            return;
        transform.position = origPos +  new Vector3(parentLibrary.GetCrossheirPosition() * posNegMod, 0, 0);
	}

    public void RegisterLibrary(AILibrary parent, bool isPositiveX)
    {
        parentLibrary = parent;
        posNegMod = isPositiveX ? 1 : -1;
    }
}
