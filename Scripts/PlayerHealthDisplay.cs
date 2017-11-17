using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthDisplay : MonoBehaviour {

    private TextMesh healthText;
    private GameObject bar;
    private PlayerInfo playerInfo;
    private int maxHealth;
	void Start () {
        healthText = transform.Find("HealthText").GetComponent<TextMesh>();
        bar = transform.Find("Bar").gameObject;
        //playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();
        playerInfo = GameObject.Find("OVRPlayerController").GetComponent<PlayerInfo>();
        maxHealth = playerInfo.GetMaxHealth();
	}
	
	// Update is called once per frame
	void Update () {
        if(playerInfo.IsGodModeActive())
        {
            healthText.text = "∞";
            bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            return;
        }
        int curHealth = playerInfo.GetHealth();
        maxHealth = playerInfo.MaxHealth;
        healthText.text = curHealth.ToString();
        float healthRatio = (float)curHealth / (float)maxHealth;
        bar.transform.localScale = new Vector3(healthRatio, 1.0f, 1.0f);
        bar.transform.localPosition = new Vector3(Mathf.Lerp(-3.73f, 7.26f, healthRatio), -0.3f, 0.0f);   // 7.26f -> -3.73f
	}
}
