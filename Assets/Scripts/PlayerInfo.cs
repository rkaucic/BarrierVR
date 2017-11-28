using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour {
    public int MaxHealth = 40;
    private int health;

    private bool isGodModeActive;

    private MainMenuScript mainMenu;

	void Start () {
        health = MaxHealth;
        isGodModeActive = false;
        mainMenu = GameObject.Find("MainMenu").GetComponent<MainMenuScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GodModeToggle()
    {
        isGodModeActive = !isGodModeActive;
    }

    public bool IsGodModeActive()
    {
        return isGodModeActive;
    }

    public int GetMaxHealth()
    {
        return MaxHealth;
    }

    public int GetHealth()
    {
        return health;
    }

    public void ResetHealth()
    {
        health = MaxHealth;
    }

    public void Damage(int amount)
    {
        if(IsGodModeActive() || health == 0)
        {
            return; // Nope.
        }
        health -= amount;
        health = Mathf.Max(health, 0);
        if (health == 0)
        {
            die();
        }
    }

    private void die()
    {
        Object.Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        GameObject[] projs = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject g in projs)
        {
            Object.Destroy(g);
        }
        mainMenu.GameOver();
    }
}
