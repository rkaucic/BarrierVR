using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public float Speed = 1.0f;
    public int Damage = 3;
    public GameObject BlockedSound;
    public GameObject HitSound;

    public AudioClip HapticBlock;

    private OVRHapticsClip hapticClip;


    void Start () {
        Vector3 target = GameObject.Find("CenterEyeAnchor").transform.position;
        target.y -= 0.1f;
        this.transform.rotation = Quaternion.LookRotation(target - this.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        Vector3 p = this.transform.position;
        if(p.x > 15 || p.x < -15 || p.y > 10 || p.y < -2 || p.z > 10 || p.z < -12)
        {
            Object.Destroy(this.gameObject);
        }
	}

    void OnTriggerEnter(Collider collider)
    {
        GameObject other = collider.gameObject;
        //print(other.name);
        //print(other.name);
        if(other.tag == "Shield" || other.tag == "Pointer")
        {
            if(BlockedSound != null)
                Instantiate(BlockedSound, transform.position, transform.rotation);
            hapticClip = new OVRHapticsClip(HapticBlock);
            OVRHaptics.Channels[other.name == "LeftShield" ? 0 : 1].Mix(hapticClip);
            Object.Destroy(this.gameObject);
        }
        else if (other.tag == "Player")
        {
            GameObject.Find("OVRPlayerController").GetComponent<PlayerInfo>().Damage(Damage);
            if(HitSound != null)
                Instantiate(HitSound, transform.position, transform.rotation);
            Object.Destroy(this.gameObject);
        }
    }
}
