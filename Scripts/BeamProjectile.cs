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
    private GameObject rightHand;

    public AudioClip HapticBlock;
    public AudioClip HapticBreak;

    private OVRHapticsClip hapticClip;

    private bool playedHaptic;

	void Start () {
        player = GameObject.Find("OVRPlayerController");
        lineScript = GetComponent<VolumetricLineBehavior>();
        layerMask = (1 << 10);    // Stop at hittable layer
        lastTimeHit = 0.0f;
        playedHaptic = false;
        rightHand = GameObject.Find("RightShield");
        /*
        Vector3 target = GameObject.Find("CenterEyeAnchor").transform.position;
        target.y -= 0.1f;
        this.transform.rotation = Quaternion.LookRotation(target - this.transform.position);*/
    }

    private Vector3 getEndLocation(out bool hitPlayer, out bool brokeShield)
    {
        RaycastHit hitInfo;
        Debug.DrawRay(transform.position, transform.forward, Color.red, 50.0f, true);
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 50.0f, layerMask))
        {
            //Vector3 endPos = hitInfo.point;//transform.position + (hitInfo.distance * transform.forward);
            Vector3 endPos = new Vector3(0.0f, 0.0f, hitInfo.distance);
            GameObject hitObj = hitInfo.collider.gameObject;
            hitPlayer = hitObj.tag == "Player";
            string hitObjTag = hitObj.tag;
            brokeShield = ((hitObjTag == "Shield" || hitObjTag == "Pointer") && !rightHand.GetComponent<Shield>().IsOverlapping());
            if (!playedHaptic && (hitObjTag == "Shield" || hitObjTag == "Pointer"))
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
        //Debug.Log("ERROR: Beam didn't find player or shield!");
        hitPlayer = brokeShield = false;
        return new Vector3(0.0f, 0.0f, 0.0f);
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

    public void des()
    {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void FixedUpdate () {
        bool hitPlayer, brokeShield;
        Vector3 tmp = getEndLocation(out hitPlayer, out brokeShield);
        if (tmp.x != 0.0f || tmp.y != 0.0f || tmp.z != 0.0f)
        {
            lineScript.EndPos = tmp;
        }
        if(hitPlayer && canDamagePlayer())
        {
            damagePlayer();
            Invoke("des", 2);
        }
        else if(brokeShield && canDamagePlayer())
        {
            damagePlayer();
            Instantiate(ShieldBreakSound, player.transform.position, player.transform.rotation);
            Invoke("des", 2);
        }
	}
}
