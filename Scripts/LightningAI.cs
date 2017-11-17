using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copied from UndyneAI.cs
public class LightningAI : MonoBehaviour
{
    public delegate void ProjectileFireEvent();

    public AILibrary lib;
    private Spawner spawner;
    private GameObject basicLaser;
    private GameObject laser;
    private GameObject fakeLaser;
    private GameObject lightningBlaster;
    private GameObject basicCanon;
    private GameObject canon;
    private int prevPlayerHealth;
    private int turnCount;
    private bool attackActive;
    private bool enemyDead;
    private float lastTimeMoved;
    private float lastPos;

    private int totalKillCount;

    private PlayerInfo playerInfo;

    // Use this for initialization
    void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        laser = (GameObject)Resources.Load("LightningLaser");
        fakeLaser = (GameObject)Resources.Load("FakeLaser");
        lightningBlaster = (GameObject)Resources.Load("LightningBlaster");
        basicCanon = (GameObject)Resources.Load("CanonBlaster");
        canon = (GameObject)Resources.Load("UndyneCanonBlaster");

        playerInfo = GameObject.Find("OVRPlayerController").GetComponent<PlayerInfo>();
        lib.RegisterCrossheirPositionFunc(CrossheirPosition);
        lib.SetVitals(10.0f);
        lib.SetLightning();
        //lib.SetVitals(11.0f);
        attackActive = false;
        enemyDead = false;
        turnCount = 0;
        lastTimeMoved = 0f;
        lastPos = 0f;
    }

    public void RegisterTotalKills(int kills)
    {
        totalKillCount = kills;
        Invoke("begin", 0.5f);
    }

    private void begin()
    {
        lib.PlayMusic((AudioClip)Resources.Load("Battle Against a True Hero"));
        lib.AddTextToQueue("*Fight intro text here*");
        lib.AddTextToQueue("I am Barry Allen, the fastest man alive!");
        lib.AddTextToQueue("There's no way you're fast enough to beat me!");
        lib.AddTextToQueue("*More intro text*");
        lib.AddTextToQueue("*Perhaps some story building*");
        lib.AddTextToQueue("*Maybe some jokes?*");
        lib.AddTextToQueue("*Perhaps it's conditional on who the player has killed*", attackSelect);
    }

    private void playerTurn()
    {
        //lib.solidifyEnemy(); they're already solid
        if(!attackActive && !enemyDead)
        {
            attackActive = true;
            turnCount++;
            lib.lightningAttack(enableNewAttack);
        }
    }

    private void enableNewAttack()
    {
        attackActive = false;
        lethalCheck();
    }
    private void attackSelect()
    {
        // this check prevents the AI from attacking even when they've already died
        if(lib.GetAIHealth() != 0.0f)
        {
            getRandomChat();
        }
    }
    private void getRandomChat()
    {
        lib.AddTextToQueue("*Creative, funny, and thought-provoking chat lines!*", getRandomAttack());
    }

    private AILibrary.OnSpeechFinishedEvent getRandomAttack()
    { 
        int choice = (int)(Random.value * 6);
        switch (choice)
        {
            case 0:
                return easyBulletHell;
            case 1:
                return easyBulletHellFakes;
            case 2:
                return hardBulletHell;
            case 3:
                return hardBulletHellFakes;
            case 4:
                return zigZagBlaster;
            default:
                return mostlyFakes;
        }
    }

    private void onlyFakes()
    {
        float delay = 1.0f;
        float attackLength = 10.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < attackLength; i += 0.5f)
        {
            StartCoroutine(spawnProjectiles("fakeLaserEasy_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }
    private void easyBulletHell()
    {
        //lib.fadeEnemy(); enemy is still vulnerable!
        float delay = 1.0f;
        float attackLength = 10.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < attackLength; i += 0.5f)
        {
            StartCoroutine(spawnProjectiles("easyBulletHell_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }

    private void easyBulletHellFakes()
    {
        float delay = 1.0f;
        float attackLength = 10.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < attackLength; i += 0.5f)
        {
            float rval = Random.value;
            if (rval < .25f)
            {
                StartCoroutine(spawnProjectiles("fakeLaserEasy_spawn", delay * i));
            }
            StartCoroutine(spawnProjectiles("easyBulletHell_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }
    private void hardBulletHell()
    {
        float delay = 0.333f;
        float attackLength = 12.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < 3.333 * attackLength; i++)
        {
            StartCoroutine(spawnProjectiles("hardBulletHell_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }

    private void hardBulletHellFakes()
    {
        float delay = 0.333f;
        float attackLength = 12.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < 3.333 * attackLength; i++)
        {
            float rval = Random.value;
            if (rval < .25f)
            {
                StartCoroutine(spawnProjectiles("fakeLaserHard_spawn", delay * i));
            }
            StartCoroutine(spawnProjectiles("hardBulletHell_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }

    private void zigZagBlaster()
    {
        float delay = 0.1f;
        float attackLength = 3.5f;
        float x1 = ((int)(Random.value * 10) + 20);
        float x2 = ((int)(Random.value * 10) + 20);
        float y1 = ((int)(Random.value * -30) - 10);
        float y2 = ((int)(Random.value * 30) + 10);
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(spawnProjectileAtXY(delay * i, x1, y1));
            x1 -= 1.0f;
            y1 += 1.0f;
            StartCoroutine(spawnProjectileAtXY(delay * i, x2, y2));
            x2 -= 1.0f;
            y2 -= 1.0f;
        }
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(spawnProjectileAtXY(delay * i + 0.5f, x1, y1));
            x1 -= 1.0f;
            y1 -= 1.0f;
            StartCoroutine(spawnProjectileAtXY(delay * i + 0.5f, x2, y2));
            x2 -= 1.0f;
            y2 += 1.0f;
        }
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(spawnProjectileAtXY(delay * i + 1.0f, x1, y1));
            x1 -= 1.0f;
            y1 += 1.0f;
            StartCoroutine(spawnProjectileAtXY(delay * i + 1.0f, x2, y2));
            x2 -= 1.0f;
            y2 -= 1.0f;
        }
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(spawnProjectileAtXY(delay * i + 1.5f, x1, y1));
            x1 -= 1.0f;
            y1 -= 1.0f;
            StartCoroutine(spawnProjectileAtXY(delay * i + 1.5f, x2, y2));
            x2 -= 1.0f;
            y2 += 1.0f;
        }
        float x_avg = (x1 + x2) / 2.0f;
        StartCoroutine(spawnBlaster(x_avg, 0, 2.5f));
        StartCoroutine(spawnBlaster(x_avg + 14, 0, 3.0f));
        StartCoroutine(spawnBlaster(x_avg + 28, 0, 3.5f));
        StartCoroutine(spawnBlaster(x_avg + 42, 0, 4.0f));
        StartCoroutine(waitOnProjectiles(attackLength));
    }

    private IEnumerator spawnBlaster(float x, float y, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        spawner.SpawnProjectileAtAngle(lightningBlaster, x, y, 0.8f);
    }
    private void mostlyFakes()
    {
        float delay = 0.5f;
        float attackLength = 8.0f;
        Invoke("playerTurn", delay * (attackLength / 2.0f));
        for (float i = 0; i < 2 * attackLength; i++)
        {
            float val = Random.value;
            int max = (int)(2.5 + val);
            for (int j = 0; j < max; j++)
            {
                StartCoroutine(spawnProjectiles("fakeLaserHard_spawn", delay * i));
            }
            StartCoroutine(spawnProjectiles("hardBulletHell_spawn", delay * i));
        }
        StartCoroutine(waitOnProjectiles(attackLength));
    }

    private IEnumerator spawnProjectileAtXY(float waitTime, float x, float y)
    {
        yield return new WaitForSeconds(waitTime);
        spawner.SpawnProjectileAtAngle(laser, x, y, 1.0f);
    }
    private IEnumerator spawnProjectiles(string callbackName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(!enemyDead)
        {
            Invoke(callbackName, 0.0f);
        }
    }
    private IEnumerator waitOnProjectiles(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        lib.WaitForProjectiles(attackSelect);
    }

    private void fakeLaserEasy_spawn()
    {
        float y = ((int)(Random.value * 40.0f) - 20);
        float x = ((int)(Random.value * 30.0f) - 10);
        spawner.SpawnProjectileAtAngle(fakeLaser, x, y, 1.0f);
    }
    private void fakeLaserHard_spawn()
    {
        float y = ((int)(Random.value * 60.0f) - 30);
        float x = ((int)(Random.value * 50.0f) - 15);
        spawner.SpawnFakeProjectileAtAngle(fakeLaser, x, y, 1.0f);
    }
    private void easyBulletHell_spawn()
    {
        float y = ((int)(Random.value * 40.0f) - 20);
        float x = ((int)(Random.value * 30.0f) - 10);
        spawner.SpawnProjectileAtAngle(laser, x, y, 1.0f);
    }
    
    private void hardBulletHell_spawn()
    {
        float y = ((int)(Random.value * 60.0f) - 30);
        float x = ((int)(Random.value * 50.0f) - 15);
        spawner.SpawnProjectileAtAngle(laser, x, y, 1.0f);
    }
    public float CrossheirPosition(float time)
    {
        /*if (Time.fixedTime >= lastTimeMoved + 2.0f)
        {
            lastPos = Random.value * 1.5f - 0.75f;
            lastTimeMoved = Time.fixedTime;
        }
        return lastPos;
        */
        return 2.5f - 1.0f * time;
    }
    private void die()
    {
        //playerInfo.MaxHealth = 200; unnecessary atm
        playerInfo.ResetHealth();
        lib.Die();
    }
    private void lethalCheck()
    {
        if (lib.GetAIHealth() == 0.0f)
        {
            enemyDead = true;
            lib.DisallowFire();
            lib.DestroyCrossheir();
            lib.DestroyProjectiles();
            lib.StopMusic();
            lib.AddTextToQueue("...");
            lib.AddTextToQueue("I ...");
            lib.AddTextToQueue("I can't believe it.");
            lib.AddTextToQueue("I thought I was ...");
            lib.AddTextToQueue("the fastest.", die);
        }
        // no support for "mercy" currently
    }
    private void theChoiceAttack()
    {
        lib.PlayerAttack(lethalCheck, true, mercy);
    }
    private void mercy()
    {
        lib.StopMusic();
        lib.AddTextToQueue("Hmph, giving up so soon?", spare);
    }
    private void spare()
    {
        lib.Spare();
    }
    /*public void setLastAttackActualTime(float attackTime)
    {
        lastAttackActualTime = attackTime;
    }*/
    // Update is called once per frame
    void Update()
    {

    }
}
