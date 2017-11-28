using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public float Radius = 10.0f;
    public GameObject DebugSphere;
    public AlertPosition ap;
	void Start () {
        ap = GameObject.Find("OVRPlayerController").GetComponent<AlertPosition>();
    }
	
	void Update () {
		
	}

    public Vector3 GetSpawnLocationFromAngle(float pitch, float yaw, float ratioToShell)
    {
        Quaternion skew = Quaternion.Euler(-1 * pitch, yaw, 0);
        Vector3 relativePos = skew * (Vector3.forward * (ratioToShell * Radius));
        return relativePos + transform.position;
    }

    /** @brief Spawn a projectile traveling toward center at angle off of center
     */
    public GameObject SpawnProjectileAtAngle(GameObject projectile, float x, float y, float ratioToShell)
    {
        Quaternion skew = Quaternion.Euler(-1 * x, y, 0);
        return SpawnProjectileAtAngle(projectile, skew, ratioToShell);
    }

    /** @brief Spawn a projectile traveling toward center at angle off of center
     */
    public GameObject SpawnProjectileAtAngle(GameObject projectile, Quaternion quat, float ratioToShell)
    {
        Vector3 relativePos = quat * (Vector3.forward * (ratioToShell * Radius));
        return SpawnProjectileAtPos(projectile, relativePos);
    }

    /** @brief Spawn a projectile traveling toward center at relative offset
     */
    public GameObject SpawnProjectileAtPos(GameObject projectile, Vector3 relativePos)
    {
        Vector3 lookVec = relativePos * -1;
        Quaternion rot = Quaternion.LookRotation(lookVec);
        GameObject c =  (GameObject)Instantiate(projectile, transform.position + relativePos, rot);
        //ap.CreateAlertBall(c);
        return c;
    }
}
