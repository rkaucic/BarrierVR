using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public float Radius = 10.0f;
    public GameObject DebugSphere;
	void Start () {
		
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

    public GameObject SpawnFakeProjectileAtAngle(GameObject projectile, float x, float y, float ratioToShell)
    {
        Quaternion skew = Quaternion.Euler(-1 * x, y, 0);
        return SpawnFakeProjectileAtAngle(projectile, skew, ratioToShell);
    }

    /** @brief Spawn a projectile traveling toward center at angle off of center
     */
    public GameObject SpawnProjectileAtAngle(GameObject projectile, Quaternion quat, float ratioToShell)
    {
        Vector3 relativePos = quat * (Vector3.forward * (ratioToShell * Radius));
        return SpawnProjectileAtPos(projectile, relativePos);
    }

    public GameObject SpawnFakeProjectileAtAngle(GameObject projectile, Quaternion quat, float ratioToShell)
    {
        Vector3 relativePos = quat * (Vector3.forward * (ratioToShell * Radius));
        return SpawnFakeProjectileAtPos(projectile, relativePos);
    }

    /** @brief Spawn a projectile traveling toward center at relative offset
     */
    public GameObject SpawnProjectileAtPos(GameObject projectile, Vector3 relativePos)
    {
        Vector3 lookVec = relativePos * -1;
        Quaternion rot = Quaternion.LookRotation(lookVec);
        return (GameObject)Instantiate(projectile, transform.position + relativePos, rot);
    }

    public GameObject SpawnFakeProjectileAtPos(GameObject projectile, Vector3 relativePos)
    {
        Vector3 lookVec = relativePos * -1;
        Quaternion rot = Quaternion.LookRotation(lookVec);
        return (GameObject)Instantiate(projectile, transform.position + relativePos, rot);
    }
}
