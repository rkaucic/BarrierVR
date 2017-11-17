using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleportation : MonoBehaviour {
	public GameObject OVRPlayerController;
	public GameObject eye;
    public GameObject menu;
    public GameObject teleportationSign;

    public GameObject playerLeftHand;
    public float coldDown;
    
    private bool XPressed;
    private GameObject sign;
    private float timeElapsed;

	// Use this for initialization
	void Start () {
        XPressed = false;
        timeElapsed = 0.0f;
    }

    // Update is called once per frame
    void Update() {
        timeElapsed += Time.deltaTime;
        if(timeElapsed < 2.0f)
        {
            return;
        }
        //print(timeElapsed);
        //Debug.Log(LayerMask.LayerToName(10));
        /*
		if (OVRInput.GetDown (OVRInput.RawButton.X)) {
			//print ("X pressed");
			Vector3 p = OVRPlayerController.transform.position;
			Vector3 d = eye.transform.forward;
			if (d.y < 0.0f) {
				d /= -1 * d.y;
				d *= p.y;

				Vector3 hitPoint = p + d;

				if (hitPoint.x > 10.0f) {
					hitPoint.x = 10.0f;
				}
				if (hitPoint.x < -10.0f) {
					hitPoint.x = -10.0f;
				}

				if (hitPoint.z > 2.0f) {
					hitPoint.z = 2.0f;
				}
				if (hitPoint.z < -8.0f) {
					hitPoint.z = -10.0f;
				}
				this.transform.position = new Vector3 (hitPoint.x, 1, hitPoint.z);
                menu.transform.position = new Vector3(hitPoint.x, 1, hitPoint.z);
            }
        }
        */

        /*
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (XPressed == false)
            {
                XPressed = true;
                sign = Instantiate(teleportationSign, this.transform.position, this.transform.rotation);
            }
            else
            {
                XPressed = false;
                Vector3 p = sign.transform.position;
                p.y = 1.0f;
                this.transform.position = p;
                menu.transform.position = p;
                Destroy(sign.gameObject);
            }
        }

        if (XPressed == true)
        {
            Vector3 d = Vector3.Normalize(playerLeftHand.transform.position - eye.transform.position);
            Vector3 p = eye.transform.position;
            if (d.y < 0.0f)
            {
                d /= -1 * d.y;
                d *= p.y;

                Vector3 hitPoint = p + d;

                if (hitPoint.x > 10.0f)
                {
                    hitPoint.x = 10.0f;
                }
                if (hitPoint.x < -10.0f)
                {
                    hitPoint.x = -10.0f;
                }

                if (hitPoint.z > 2.0f)
                {
                    hitPoint.z = 2.0f;
                }
                if (hitPoint.z < -8.0f)
                {
                    hitPoint.z = -10.0f;
                }
                sign.transform.position = new Vector3(hitPoint.x, 0, hitPoint.z);
            }
        }
        */

        //print(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick));
        //print(OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick));

        float leftX = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
        float leftY = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
        float rightX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
        float rightY = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;

        //print(rightX);

        float diff = Mathf.Sqrt(Mathf.Pow(leftX - rightX, 2.0f) + Mathf.Pow(leftY - rightY, 2.0f));
        //print(Mathf.Sqrt(Mathf.Pow(leftX, 2.0f) + Mathf.Pow(leftY, 2.0f)));
        if( Mathf.Sqrt(Mathf.Pow(leftX, 2.0f) + Mathf.Pow(leftY, 2.0f)) < 0.9f || Mathf.Sqrt(Mathf.Pow(rightX, 2.0f) + Mathf.Pow(rightY, 2.0f)) < 0.9f ) {
            //print(new Vector2 ( Mathf.Sqrt(Mathf.Pow(leftX, 2.0f) + Mathf.Pow(leftY, 2.0f)), Mathf.Sqrt(Mathf.Pow(rightX, 2.0f) + Mathf.Pow(rightY, 2.0f))) );
            return;
        }
        if(diff < 0.5f)
        {
            float x = (leftX + rightX) / 2;
            float y = (leftY + rightY) / 2;
            if(y < -0.5)
            {
                y = -1.0f;
            }
            else if(y > 0.5)
            {
                y = 1.0f;
            }
            else
            {
                y = 0.0f;
            }

            if(x < -0.5)
            {
                x = -1.0f;
            }
            else if(x > 0.5)
            {
                x = 1.0f;
            }
            else
            {
                x = 0.0f;
            }

            float tmp = Mathf.Sqrt( Mathf.Pow(x, 2.0f) + Mathf.Pow(y, 2.0f) );
            x = this.transform.position.x + 5 * x / tmp;
            y = this.transform.position.z + 5 * y / tmp;

            if (x > 10.0f)
            {
                x = 10.0f;
            }
            if (x < -10.0f)
            {
                x = -10.0f;
            }

            if (y > 2.0f)
            {
                y = 2.0f;
            }
            if (y < -8.0f)
            {
                y = -10.0f;
            }

            //print(new Vector2(x, y));
            this.transform.position = new Vector3(x, 1.0f, y);
            menu.transform.position = new Vector3(x, 1.0f, y);
            timeElapsed = 0.0f;
        }
    }
}