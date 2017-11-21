using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class BeamProjectile : MonoBehaviour {

    public int DamagePerSec = 5;
    public GameObject ShieldBreakSound;
    public GameObject PlayerHitSound;

    private VolumetricLineBehavior lineScript;
    private int layerMask;
    private float lastTimeHit;
    private GameObject player;

    public AudioClip HapticBlock;
    public AudioClip HapticBreak;

    private OVRHapticsClip hapticClip;

    private bool playedHaptic;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        lineScript = GetComponent<VolumetricLineBehavior>();
        layerMask = (1 << 8) | (1 << 9);    // Stop at either the player or a shield
        lastTimeHit = 0.0f;
        playedHaptic = false;
	}

    private Vector3 getEndLocation(out bool hitPlayer, out bool brokeShield)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 50.0f, layerMask))
        {
            //Vector3 endPos = hitInfo.point;//transform.position + (hitInfo.distance * transform.forward);
            Vector3 endPos = new Vector3(0.0f, 0.0f, hitInfo.distance);
            GameObject hitObj = hitInfo.collider.gameObject;
            hitPlayer = hitObj.tag == "Player";
            string hitObjTag = hitObj.tag;
            brokeShield = (hitObjTag == "Shield" && !hitObj.GetComponent<Shield>().IsOverlapping());
            if (!playedHaptic && hitObjTag == "Shield")
            {
                playedHaptic = true;
                if (!brokeShield)
                {
                    hapticClip = new OVRHapticsClip(HapticBlock);
                    OVRHaptics.Channels[0].Mix(hapticClip);
                    OVRHaptics.Channels[1].Mix(hapticClip);
                }
                else
                {
                    hapticClip = new OVRHapticsClip(HapticBreak);
                    OVRHaptics.Channels[hitObj.name == "LeftShield" ? 0 : 1].Mix(hapticClip);
                }
            }
            return endPos;
        }
        Debug.Log("ERROR: Beam didn't find player or shield!");
        hitPlayer = brokeShield = false;
        return new Vector3();
    }

    private bool canDamagePlayer()
    {
        return Time.fixedTime - (float)lastTimeHit >= 1.0f;
    }

    private void damagePlayer()
    {
        player.GetComponent<PlayerInfo>().Damage(DamagePerSec);
        lastTimeHit = Time.fixedTime;
        Instantiate(PlayerHitSound, player.transform.position, player.transform.rotation);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        bool hitPlayer, brokeShield;
        lineScript.EndPos = getEndLocation(out hitPlayer, out brokeShield);
        if(hitPlayer && canDamagePlayer())
        {
            damagePlayer();
        }
        else if(brokeShield && canDamagePlayer())
        {
            damagePlayer();
            Instantiate(ShieldBreakSound, player.transform.position, player.transform.rotation);
        }
	}
}
