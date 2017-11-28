using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndyneAI : MonoBehaviour
{
    public AILibrary lib;
    private Spawner spawner;
    private GameObject basicLaser;
    private GameObject laser;
    private GameObject gasterBlaster;
    private GameObject basicCanon;
    private GameObject canon;
    private int prevPlayerHealth;
    private int turnCount;

    private int totalKillCount;

    private PlayerInfo playerInfo;

    void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        basicLaser = (GameObject)Resources.Load("BasicLaser");
        laser = (GameObject)Resources.Load("UndyneLaser");
        gasterBlaster = (GameObject)Resources.Load("GasterBlaster");
        basicCanon = (GameObject)Resources.Load("CanonBlaster");
        canon = (GameObject)Resources.Load("UndyneCanonBlaster");

        playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

        lib.RegisterCrossheirPositionFunc(CrossheirPosition);
        lib.SetVitals(300.0f);
        //lib.SetVitals(11.0f);
        turnCount = 0;
    }

    public void RegisterTotalKills(int kills)
    {
        totalKillCount = kills;
        Invoke("begin", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float CrossheirPosition(float time)
    {
        return (4.0f - ((time * time) * 1.3f));
    }

    public float FinishCrossheirs(float time)
    {
        return 0.0f;
    }

    private void die()
    {
        playerInfo.MaxHealth = 200;
        playerInfo.ResetHealth();
        lib.Die();
    }

    private void spare()
    {
        lib.Spare();
    }

    private void lethalCheck()
    {
        if (lib.GetAIHealth() == 0.0f)
        {
            lib.StopMusic();
            lib.AddTextToQueue("...");
            lib.AddTextToQueue("Heh.");
            lib.AddTextToQueue("Heheheh.");
            lib.AddTextToQueue("Ahhahahaha.");
            lib.AddTextToQueue("AHHAHAHAHAHAHAHA!");
            lib.AddTextToQueue("...");
            lib.AddTextToQueue("I feel bad for you, kid.");
            lib.AddTextToQueue("...");
            lib.AddTextToQueue("Sorry Papyrus.", die);
        }
        else
            theChoiceAttack();
    }

    private void mercy()
    {
        lib.StopMusic();
        lib.AddTextToQueue("...");
        lib.AddTextToQueue(".....");
        lib.AddTextToQueue(".......", spare);
    }

    private void theChoice()
    {
        lib.PlayMusic((AudioClip)Resources.Load("The Choice"));
        lib.RegisterCrossheirPositionFunc(FinishCrossheirs);
        theChoiceAttack();
    }
    private void theChoiceAttack()
    {
        lib.PlayerAttack(lethalCheck, true, mercy);
    }

    private void finalDialogueDone()
    {
        lib.WaitForMusicFade(theChoice);
    }

    private void finishHer()
    {
        lib.FadeoutMusic(0.0f, 6.0f);
        
        lib.AddTextToQueue("Why...");
        lib.AddTextToQueue("Won't...");
        lib.AddTextToQueue("You...");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("Who am I kidding?");
        lib.AddTextToQueue("I don't know how...");
        lib.AddTextToQueue("But you won.");
        lib.AddTextToQueue("Congrats.");
        lib.AddTextToQueue("You did it.");
        lib.AddTextToQueue("Are you happy?");
        lib.AddTextToQueue("Is this what you wanted?");
        lib.AddTextToQueue("Heh.");
        lib.AddTextToQueue("You won't stop with me, though, will you?");
        lib.AddTextToQueue("You won't be satisfied until there's nobody left.");
        lib.AddTextToQueue("So go on, then.");
        lib.AddTextToQueue("You know how. What's the holdup?", finalDialogueDone);
    }

    private AILibrary.OnSpeechFinishedEvent getRandomAttack()
    {
        int choice = (int)(Random.value * 5);
        switch(choice)
        {
            case 0:
                return theTwist2;
            case 1:
                return tripleSpray2;
            case 2:
                return doubleTapBlast2;
            case 3:
                return bulletHell2;
            default:
                return circleShock2;
        }
    }

    private void getRandomChat()
    {
        int choice = (int)(Random.value * 6);
        switch (choice)
        {
            case 0:
                lib.AddTextToQueue("GET OVER HERE!", getRandomAttack());
                break;
            case 1:
                lib.AddTextToQueue("I can see you weakening.", getRandomAttack());
                break;
            case 2:
                lib.AddTextToQueue("You think you can triumph?");
                lib.AddTextToQueue("Fool! Goodness always wins over evil!", getRandomAttack());
                break;
            case 3:
                lib.AddTextToQueue("GOD you're an annoying little brat!", getRandomAttack());
                break;
            case 4:
                lib.AddTextToQueue("Hehehe...", getRandomAttack());
                break;
            default:
                lib.AddTextToQueue("JUST DIE ALREADY!", getRandomAttack());
                break;
        }
    }

    private void circleShock2()
    {
        lib.fadeEnemy();
        float delay = 0.65f;
        circleShock2_spawnRandomCircle();
        for (int i = 1; i < 8; i++)
        {
            Invoke("circleShock2_spawnRandomCircle", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }
    private void circleShock2_spawnRandomCircle()
    {
        float x = (int)(Random.value * 50) - 0;
        float y = (int)(Random.value * 80) - 40;
        circleShock2_spawnCircle(new Vector2(x, y), 1.0f, 15.0f, 8, 0.5f);
    }
    private void circleShock2_spawnCircle(Vector2 center, float dScale, float radius, int laserCount, float totalTime)
    {
        const float TWOPI = 2 * Mathf.PI;
        float delay = ((float)totalTime) / ((float)laserCount);
        for (int i = 0; i < laserCount; i++)
        {
            float rads = (((float)i) / ((float)laserCount)) * TWOPI;
            float pitchOff = Mathf.Sin(rads) * radius;
            float yawOff = Mathf.Cos(rads) * radius;
            StartCoroutine(circleShock2_spawnLaser(new Vector2(center.x + pitchOff, center.y + yawOff), dScale, delay * (float)i));
        }
    }
    private IEnumerator circleShock2_spawnLaser(Vector2 pos, float dScale, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        spawner.SpawnProjectileAtAngle(laser, pos.x, pos.y, dScale);
    }

    private void doubleTapBlast2()
    {
        lib.fadeEnemy();
        float delay = 0.35f;
        float offset = -0.9f;
        float gast1 = 8.0f * delay - 0.3f;
        float gast2 = gast1 + 3.0f + 7.0f * delay;
        doubleTapBlast2_Phase1();
        Invoke("doubleTapBlast2_Phase2", 1.0f * delay);
        Invoke("doubleTapBlast2_Phase1", 2.0f * delay);
        Invoke("doubleTapBlast2_Phase2", 3.0f * delay);
        Invoke("doubleTapBlast2_Phase1", 4.0f * delay);
        Invoke("doubleTapBlast2_Phase2", 5.0f * delay);
        Invoke("doubleTapBlast2_Phase1", 6.0f * delay);
        Invoke("doubleTapBlast2_Phase2", 7.0f * delay);
        Invoke("doubleTapBlast2_LGaster", gast1);
        Invoke("doubleTapBlast2_Phase1", gast1 + 8.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase2", gast1 + 9.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase1", gast1 + 10.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase2", gast1 + 11.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase1", gast1 + 12.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase2", gast1 + 13.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase1", gast1 + 14.0f * delay + offset);
        Invoke("doubleTapBlast2_Phase2", gast1 + 15.0f * delay + offset);
        Invoke("doubleTapBlast2_RGaster", gast2 + offset);
        lib.WaitForProjectiles(playerTurn);
    }
    private void doubleTapBlast2_Phase1()
    {
        doubleTapBlast2_LL();
        doubleTapBlast2_RD();
    }
    private void doubleTapBlast2_Phase2()
    {
        doubleTapBlast2_LR();
        doubleTapBlast2_RU();
    }
    private void doubleTapBlast2_LL()
    {
        spawner.SpawnProjectileAtAngle(laser, 0, -40, 0.8f);
    }
    private void doubleTapBlast2_LR()
    {
        spawner.SpawnProjectileAtAngle(laser, 0, -10, 0.8f);
    }
    private void doubleTapBlast2_RD()
    {
        spawner.SpawnProjectileAtAngle(laser, -10, 25, 0.8f);
    }
    private void doubleTapBlast2_RU()
    {
        spawner.SpawnProjectileAtAngle(laser, 30, 25, 0.8f);
    }
    private void doubleTapBlast2_LGaster()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 25, -25, 0.8f);
    }
    private void doubleTapBlast2_RGaster()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 25, 25, 0.8f);
    }

    private void tripleSpray2()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, -5, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 60, 0.8f, 2.5f, 0.30f);
        c = spawner.SpawnProjectileAtAngle(canon, 15, 30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, -60, 0.8f, 2.5f, 0.30f);
        c = spawner.SpawnProjectileAtAngle(canon, 35, 30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, -60, 0.8f, 2.5f, 0.30f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void bulletHell2()
    {
        lib.fadeEnemy();
        float delay = 0.35f;
        bulletHell2_spawn();
        for (int i = 1; i < 40; i++)
        {
            Invoke("bulletHell2_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }
    private void bulletHell2_spawn()
    {
        float y = ((int)(Random.value * 70.0f) - 35);
        float x = ((int)(Random.value * 50.0f) - 10);
        spawner.SpawnProjectileAtAngle(laser, x, y, 1.0f);
    }

    private void theTwist2()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 10, 0, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 0, 1, 6, 0.5f);
        c = spawner.SpawnProjectileAtAngle(canon, -10, -25, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 0, 1, 1.5f, 0.49f);
        Invoke("theTwist2_2", 2.0f);
    }
    private void theTwist2_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 30, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(-20, 35, 1, 1.5f, 0.49f);
        Invoke("theTwist2_3", 2.0f);
    }
    private void theTwist2_3()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 40, 0, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 0, 1, 1.5f, 0.49f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void circleShock1()
    {
        lib.fadeEnemy();
        float delay = 0.85f;
        circleShock1_spawnRandomCircle();
        for (int i = 1; i < 5; i++)
        {
            Invoke("circleShock1_spawnRandomCircle", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }
    private void circleShock1_spawnRandomCircle()
    {
        float x = (int)(Random.value * 50) - 0;
        float y = (int)(Random.value * 80) - 40;
        circleShock1_spawnCircle(new Vector2(x, y), 1.0f, 10.0f, 8, 0.5f);
    }
    private void circleShock1_spawnCircle(Vector2 center, float dScale, float radius, int laserCount, float totalTime)
    {
        const float TWOPI = 2 * Mathf.PI;
        float delay = ((float)totalTime) / ((float)laserCount);
        for(int i = 0; i < laserCount; i++)
        {
            float rads = (((float)i) / ((float)laserCount)) * TWOPI;
            float pitchOff = Mathf.Sin(rads) * radius;
            float yawOff = Mathf.Cos(rads) * radius;
            StartCoroutine(circleShock1_spawnLaser(new Vector2(center.x + pitchOff, center.y + yawOff), dScale, delay * (float)i));
        }
    }
    private IEnumerator circleShock1_spawnLaser(Vector2 pos, float dScale, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        spawner.SpawnProjectileAtAngle(basicLaser, pos.x, pos.y, dScale);
    }

    private void bulletHell1()
    {
        lib.fadeEnemy();
        float delay = 0.35f;
        bulletHell1_spawn();
        for(int i = 1; i < 40; i++)
        {
            Invoke("bulletHell1_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }
    private void bulletHell1_spawn()
    {
        float y = ((int)(Random.value * 60.0f) - 30);
        float x = ((int)(Random.value * 40.0f) - 10);
        spawner.SpawnProjectileAtAngle(basicLaser, x, y, 1.0f);
    }

    private void tripleSpray1()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(basicCanon, -5, -30, 0.8f);
        c.GetComponent<CanonSeries>().Init(0, 60, 0.8f, 3.0f, 0.35f);
        c = spawner.SpawnProjectileAtAngle(basicCanon, 15, 30, 0.8f);
        c.GetComponent<CanonSeries>().Init(0, -60, 0.8f, 3.0f, 0.35f);
        c = spawner.SpawnProjectileAtAngle(basicCanon, 35, 30, 0.8f);
        c.GetComponent<CanonSeries>().Init(0, -60, 0.8f, 3.0f, 0.35f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void doubleTapBlast1()
    {
        lib.fadeEnemy();
        float delay = 0.45f;
        float gast1 = 3.0f * delay + 1.0f;
        float gast2 = gast1 + 3.0f + 3.0f * delay;
        doubleTapBlast1_Phase1();
        Invoke("doubleTapBlast1_Phase2", 1.0f * delay);
        Invoke("doubleTapBlast1_Phase1", 2.0f * delay);
        Invoke("doubleTapBlast1_Phase2", 3.0f * delay);
        Invoke("doubleTapBlast1_LGaster", gast1);
        Invoke("doubleTapBlast1_Phase1", gast1 + 1.5f + 1.0f * delay);
        Invoke("doubleTapBlast1_Phase2", gast1 + 1.5f + 2.0f * delay);
        Invoke("doubleTapBlast1_Phase1", gast1 + 1.5f + 3.0f * delay);
        Invoke("doubleTapBlast1_Phase2", gast1 + 1.5f + 4.0f * delay);
        Invoke("doubleTapBlast1_RGaster",gast2);
        lib.WaitForProjectiles(playerTurn);
    }
    private void doubleTapBlast1_Phase1()
    {
        doubleTapBlast1_LL();
        doubleTapBlast1_RD();
    }
    private void doubleTapBlast1_Phase2()
    {
        doubleTapBlast1_LR();
        doubleTapBlast1_RU();
    }
    private void doubleTapBlast1_LL()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, 0, -40, 0.8f);
    }
    private void doubleTapBlast1_LR()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, 0, -10, 0.8f);
    }
    private void doubleTapBlast1_RD()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, -10, 25, 0.8f);
    }
    private void doubleTapBlast1_RU()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, 30, 25, 0.8f);
    }
    private void doubleTapBlast1_LGaster()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 25, -25, 0.8f);
    }
    private void doubleTapBlast1_RGaster()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 25, 25, 0.8f);
    }

    private void theTwist1()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(basicCanon, 10, 0, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 0, 1, 6, 0.5f);
        c = spawner.SpawnProjectileAtAngle(basicCanon, -10, -25, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 0, 1, 1.5f, 0.49f);
        Invoke("theTwist1_2", 2.0f);
    }
    private void theTwist1_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(basicCanon, 30, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(-20, 35, 1, 1.5f, 0.49f);
        Invoke("theTwist1_3", 2.0f);
    }
    private void theTwist1_3()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(basicCanon, 40, 0, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 0, 1, 1.5f, 0.49f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void isThisHardMode()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(basicCanon, 40, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 60, 1, 4, 0.75f);
        c = spawner.SpawnProjectileAtAngle(basicCanon, 0, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 60, 1, 4, 0.75f);
        lib.WaitForProjectiles(isThisHardMode_suprise);
    }
    private void isThisHardMode_suprise()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 40, -40, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 80, 1, 3, 0.2f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, 40, 1.0f);
        c.GetComponent<CanonSeries>().Init(10, -80, 1, 3, 0.2f);
        lib.WaitForProjectiles(isThisHardMode_gaster);
    }
    private void isThisHardMode_gaster()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 20, -20, 1.0f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void returnToPlayerTurn()
    {
        lib.WaitForProjectiles(playerTurn);
    }

    private void attackSelect()
    {
        if (lib.GetAIHealth() <= 10.0f)
        {
            finishHer();
            return;
        }
        switch (turnCount)
        {
            case 1:
                lib.AddTextToQueue("You can't possibly understand what Papyrus means to us.", isThisHardMode);
                break;
            case 2:
                lib.AddTextToQueue("He's not much of a warrior... but he has a kind heart.", theTwist1);
                break;
            case 3:
                lib.AddTextToQueue("Heh, he'd rather hug his enemies than fight them.");
                lib.AddTextToQueue("...");
                lib.AddTextToQueue("And then he met you.", doubleTapBlast1);
                break;
            case 4:
                lib.AddTextToQueue("He's really gone, isn't he?", tripleSpray1);
                break;
            case 5:
                lib.AddTextToQueue("Papyrus...");
                lib.AddTextToQueue("I'm so sorry...");
                lib.AddTextToQueue("I wasn't there when it mattered.", bulletHell1);
                break;
            case 6:
                lib.AddTextToQueue("I'll make you pay dearly!", circleShock1);
                break;
            case 7:
                lib.AddTextToQueue("You won't ever hurt anyone ever again!", theTwist2);
                break;
            case 8:
                lib.AddTextToQueue("YOU WILL PAY FOR EVERYTHING!");
                lib.AddTextToQueue("EVERY");
                lib.AddTextToQueue("SINGLE");
                lib.AddTextToQueue("THING", bulletHell2);
                break;
            case 9:
                lib.AddTextToQueue("GAAAAH!");
                lib.AddTextToQueue("...");
                lib.AddTextToQueue("Well, you're tough... I'll give you that.", tripleSpray2);
                break;
            case 10:
                lib.AddTextToQueue("But you may as well give up now!");
                lib.AddTextToQueue("You are no match for a true heroine!", doubleTapBlast2);
                break;
            case 11:
                lib.AddTextToQueue("DIE, DAMN YOU!", circleShock2);
                break;
            default:
                getRandomChat();
                break;
        }
    }

    private void playerTurn()
    {
        lib.solidifyEnemy();
        turnCount++;
        lib.PlayerAttack(attackSelect);
    }

    private void beginTrueFight()
    {
        lib.PlayMusic((AudioClip)Resources.Load("Battle Against a True Hero"));
        playerTurn();
    }

    private void fightIntro()
    {
        lib.AddTextToQueue("...");
        lib.AddTextToQueue(".....");
        lib.AddTextToQueue("Papyrus...");
        lib.AddTextToQueue("He was supposed to meet me for training hours ago.");
        lib.AddTextToQueue("He NEVER misses a session!");
        lib.AddTextToQueue("How is it that you show up and suddenly he vanishes? HUH?");
        lib.AddTextToQueue("What did you do?");
        lib.AddTextToQueue("WHAT DID YOU DO?");
        lib.AddTextToQueue("WHAT");
        lib.AddTextToQueue("DID");
        lib.AddTextToQueue("YOU");
        lib.AddTextToQueue("DO?", beginTrueFight);
    }

    private void refuse()
    {
        lib.AddTextToQueue("Huh? Who are you? How did you get down here?");
        lib.AddTextToQueue("You should turn back.");
        lib.AddTextToQueue("There are people around here that might hurt you.");
        lib.AddTextToQueue("Hurry! Shoo!", spare);
    }

    private void begin()
    {
        if (totalKillCount == 0)
            refuse();
        else
            fightIntro();
    }
}
