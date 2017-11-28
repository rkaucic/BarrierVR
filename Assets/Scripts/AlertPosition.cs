using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertPosition : MonoBehaviour {
    private GameObject[] projectiles;
    private int counter;
    public Camera cam;
    Object positionball;
    private float ballDistance = (float)0.75;

    //the map of Proj to Ball
	private Hashtable Mymap = new Hashtable();

    // Use this for initialization
    void Start () {
        positionball = Resources.Load("positionBall");
        cam = Camera.main;
    }
    private Vector3 getBallLocation(GameObject proj)
    {
        Vector3 proj_Postion_In_World = proj.transform.position;
        Vector3 viewPos = cam.WorldToViewportPoint(proj_Postion_In_World);
        //print(viewPos);

        float big_x = viewPos.x;
        float big_y = viewPos.y;
        if (big_x > 1 | big_y > 1 | big_x < 0 | big_y < 0) //if the ball is NOT in current view
        {
            float big_trig_y = big_y - (float)0.5;
            float big_trig_x = big_x - (float)0.5;
            float big_hyp = Mathf.Sqrt(big_trig_y * big_trig_y + big_trig_x * big_trig_x);
            float small_trig_x = (float)0.5 * big_trig_x / big_hyp;
            float small_trig_y = (float)0.5 * big_trig_y / big_hyp;
            if (viewPos.z < 0)
            {
                small_trig_x = -small_trig_x;
                small_trig_y = -small_trig_y;
            }
            return cam.transform.TransformPoint(small_trig_x, small_trig_y, ballDistance);
        }
        return new Vector3(-1, -1, -1);
    }

    /*public void CreateAlertBall(GameObject proj)
    {
        Vector3 thePosition = getBallLocation(proj);
        GameObject alertBall = (GameObject)Instantiate(positionball, thePosition, Quaternion.identity);
        Mymap[alertBall] = proj;  //map the alertBall to the Proj
        if (thePosition == new Vector3(-1, -1, -1))
        {
            alertBall.SetActive(false);
        }
    }*/

    void detectNewProj()
    {
        try
        {
            projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        }
        catch { }
        if (projectiles != null && projectiles.Length > 0)
        {
            foreach (GameObject proj in projectiles)
            {
                if (Mymap.ContainsValue(proj) == false) //if found new proj that's not in map
                {
                    Vector3 thePosition = getBallLocation(proj);    //get postion ball location 
                    GameObject alertBall = (GameObject)Instantiate(positionball, thePosition, Quaternion.identity); //create the postion ball
                    Mymap[alertBall] = proj;            //map the alertBall to the Proj
                    if (thePosition == new Vector3(-1, -1, -1)) //disable it if user could see it.
                    {
                        alertBall.SetActive(false);
                    }
                }
            }
        }
    }

    //remove the position balls from map when the proj corresponds died
    void removeBalls(List<GameObject> ballsToBeRemoved) 
    {
        if (ballsToBeRemoved.Count == 0)
        {
            return;
        }
        foreach (GameObject ball in ballsToBeRemoved)
        {
            GameObject theBall = ball;
            Mymap.Remove(ball);
            Object.Destroy(theBall);
        }

    }

    //update the position balls' location 
    void updataBallsPositions()
    {
        IDictionaryEnumerator denum = Mymap.GetEnumerator();
        List<GameObject> ballsToBeRemoved = new List<GameObject>();
        while (denum.MoveNext())
        {
            DictionaryEntry dentry = (DictionaryEntry)denum.Current;
            if (dentry.Value.Equals(null))
            { //the Proj is destroyed
                ballsToBeRemoved.Add((GameObject)dentry.Key);
            }
            else //update position
            {
                Vector3 thePosition = getBallLocation((GameObject)dentry.Value);
                GameObject PositionBall = (GameObject)dentry.Key;
                PositionBall.transform.position = thePosition;
                if (thePosition != new Vector3(-1, -1, -1))
                {
                    PositionBall.SetActive(true);
                }
                else
                {
                    PositionBall.SetActive(false);

                }
            }

        }
        removeBalls(ballsToBeRemoved);
    }
    // Update is called once per frame
    void Update()
    {
        detectNewProj();
        updataBallsPositions();
    }


}
