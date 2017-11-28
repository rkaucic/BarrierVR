using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SansAI : MonoBehaviour {

    public AILibrary lib;
    public GameObject CutInSound;
    public GameObject CutOutSound;

    private delegate void OnCutoutFinishedEvent();

    private PlayerInfo playerInfo;

    private Spawner spawner;
    private GameObject laser;
    private GameObject megaLaser;
    private GameObject blaster;
    private GameObject megaBlaster;
    private GameObject megaBlasterNoSwing;
    private GameObject canon;
    private GameObject megaCanon;
    private GameObject dudBlaster;

    private Transform handTrans;

    private SpriteRenderer body;
    private Sprite origSprite;
    private Sprite sleepSprite;

    private bool isGrabbingButton;
    private Transform grabbingShieldTrans;
    private ParticleSystem grabParts;
    private GrabScript grabScript;
    private CapsuleCollider grabCollider;

    private GameObject attackButton;
    private Vector3 attackButtonOriginPos;
    private Quaternion attackButtonOriginRot;
    private Vector3 attackButtonOriginScale;
    private Rigidbody attackButtonRB;

    private GameObject playerShot;
    private GameObject fakeKillShot;
    public GameObject playerHitSound;
    public GameObject stealAttackSound;

    private bool isSlidingOut;
    private Vector3 initSlidePos;
    private Vector3 finalSlidePos;
    private float totalSlideTime;
    private float startSlideTime;
    private int slidePeriods;
    private Color slideStartColor;
    private Color slideEndColor;
    private float slidePeriodTime;

    private int totalKillCount;

    private int turnCount;

    private bool overrideMusicHandleNextCutin;

    private int numAttacksPerformedInSeq;
    private int numAttacksInSubSeq;

    private Vector2[] prevZigEndPos;

    private OnCutoutFinishedEvent onCutFinishedCallback;

    private const float timeToFly = 1.428571428571429f;
    private const float oneFourthPi = Mathf.PI / 4.0f;

    void Start () {
        playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        laser = (GameObject)Resources.Load("UndyneLaser");
        megaLaser = (GameObject)Resources.Load("MegaLaser");
        blaster = (GameObject)Resources.Load("GasterBlaster");
        megaBlaster = (GameObject)Resources.Load("MegaGasterBlaster");
        megaBlasterNoSwing = (GameObject)Resources.Load("MegaGasterBlasterNoSwing");
        canon = (GameObject)Resources.Load("UndyneCanonBlaster");
        megaCanon = (GameObject)Resources.Load("MegaCanonBlaster");
        dudBlaster = (GameObject)Resources.Load("MegaGasterBlasterDud");
        
        handTrans = transform.Find("BodySprite/Hand").transform;
        grabbingShieldTrans = GameObject.Find("RightShield").transform;
        grabParts = GameObject.Find("GrabEffect").GetComponent<ParticleSystem>();
        GameObject grabObj = GameObject.Find("GrabBox");
        grabScript = grabObj.GetComponent<GrabScript>();
        grabCollider = grabObj.GetComponent<CapsuleCollider>();

        body = transform.Find("BodySprite").GetComponent<SpriteRenderer>();
        origSprite = body.sprite;
        sleepSprite = Resources.Load<Sprite>("sans_color_sleep");

        playerShot = (GameObject)Resources.Load("PlayerShot");

        overrideMusicHandleNextCutin = false;
        turnCount = 0;
        numAttacksPerformedInSeq = 0;
        numAttacksInSubSeq = 0;

        lib.SetVitals(1.0f);
        lib.RegisterCrossheirPositionFunc(CrossheirPosition);

        lib.ForcePlayerToMiss(true);

        isGrabbingButton = false;
        isSlidingOut = false;
    }

    public void RegisterTotalKills(int kills)
    {
        totalKillCount = kills;
        Invoke("begin", 0.5f);
    }

    public float CrossheirPosition(float time)
    {
        return 4.0f - (time * 2.0f);    // ... But they'll never hit :)
    }

    public float FinishCrossheirs(float time)
    {
        return 0.0f;
    }

    void Update () {
		if(isGrabbingButton)
        {
            if(AILibrary.GetDominantGripDown())
            {
                if(!grabParts.isEmitting)
                {
                    grabParts.Play();
                }
                if(grabScript.IsAttackButtonHovered())
                {
                    attackButtonRB.AddForce(5.0f * Vector3.Normalize(grabbingShieldTrans.position - attackButton.transform.position));
                }
            }
            else
            {
                if(grabParts.isEmitting)
                {
                    grabParts.Stop();
                }
            }
        }
        else if(isSlidingOut)
        {
            Vector3 offsetPerPeriod = (finalSlidePos - initSlidePos) / slidePeriods;
            float periodTime = totalSlideTime / (float)slidePeriods;

            float timeSinceStart = Mathf.Min(Time.fixedTime - startSlideTime, totalSlideTime);

            const float cosShift = Mathf.PI * 0.5f;

            Vector3 periodStartPos = offsetPerPeriod * (timeSinceStart / periodTime);
            body.transform.localPosition = initSlidePos + (periodStartPos + ((offsetPerPeriod / 2.0f) * Mathf.Abs(Mathf.Cos(cosShift + Mathf.PI * ((timeSinceStart % periodTime) / periodTime)))));

            float lerpFactor = timeSinceStart / totalSlideTime;
            body.color = Color.Lerp(slideStartColor, slideEndColor, lerpFactor);

            if(timeSinceStart == totalSlideTime)
            {
                isSlidingOut = false;
            }
        }
	}

    private void spare()
    {
        lib.SetPitch(1.0f);
        lib.Spare();
    }

    private void cutout(float outTime, OnCutoutFinishedEvent finishedCallback, bool overrideMusicHandle = false)
    {
        overrideMusicHandleNextCutin = overrideMusicHandle;
        if (!overrideMusicHandle)
        {
            lib.PauseMusic();
            
        }
        Instantiate(CutInSound, lib.GetPlayerPos(), Quaternion.identity);
        lib.DestroyProjectiles();
        lib.BlindPlayer(true);
        onCutFinishedCallback = finishedCallback;
        Invoke("cutoutEnd", outTime);
    }

    private void cutoutEnd()
    {
        Instantiate(CutOutSound, lib.GetPlayerPos(), Quaternion.identity);
        lib.BlindPlayer(false);
        if (!overrideMusicHandleNextCutin)
        {
            lib.ResumeMusic();
        }
        overrideMusicHandleNextCutin = false;
        onCutFinishedCallback();
    }

    private IEnumerator spawnMegaBlaster(Vector2 pos, float dScale, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        spawner.SpawnProjectileAtAngle(megaBlaster, pos.x, pos.y, dScale);
    }

    private IEnumerator spawnMegaLaser(Vector2 pos, float dScale, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        spawner.SpawnProjectileAtAngle(megaLaser, pos.x, pos.y, dScale);
    }

    private IEnumerator spawnDudBlaster(Vector2 pos, float dScale, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        spawner.SpawnProjectileAtAngle(dudBlaster, pos.x, pos.y, dScale);
    }

    private void die()
    {
        playerInfo.MaxHealth = 999;
        playerInfo.ResetHealth();
        lib.Die(true, 8.0f);
    }

    private void finalHit_slideOut2_slide()
    {
        initSlidePos = body.transform.localPosition;
        finalSlidePos = body.transform.localPosition + new Vector3(0.0f, 0.0f, -1.0f);
        slideStartColor = body.color;
        slideEndColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        totalSlideTime = 4.0f;
        slidePeriods = 2;
        startSlideTime = Time.fixedTime;
        slidePeriodTime = (float)slidePeriods / totalSlideTime;

        isSlidingOut = true;

        Invoke("die", 6.0f);
    }

    private void finalHit_slideOut2()
    {
        lib.AddTextToQueue("Papyrus, do you want anything?", finalHit_slideOut2_slide);
    }

    private void finalHit_slideOut1()
    {
        initSlidePos = body.transform.localPosition;
        finalSlidePos = body.transform.localPosition + new Vector3(0.0f, 0.0f, -2.5f);
        slideStartColor = body.color;   // (1, 1, 1, 1), right?
        slideEndColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
        totalSlideTime = 5.0f;
        slidePeriods = 3;
        startSlideTime = Time.fixedTime;
        slidePeriodTime = (float)slidePeriods / totalSlideTime;

        isSlidingOut = true;

        Invoke("finalHit_slideOut2", 7.0f);
    }

    private void finalHit_deadSans()
    {
        // TODO: SPRITE CHANGE

        lib.clearDamageText();

        lib.AddTextToQueue("...");
        lib.AddTextToQueue(".....");
        lib.AddTextToQueue("So.");
        lib.AddTextToQueue("I guess that's it, huh?");
        lib.AddTextToQueue("... Just...");
        lib.AddTextToQueue("Don't say I didn't warn you.");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("Welp.");
        lib.AddTextToQueue("I'm going to Grillby's.", finalHit_slideOut1);
    }

    private void finalHit_pretendHit()
    {
        Instantiate(playerHitSound, fakeKillShot.transform.position, fakeKillShot.transform.rotation);
        lib.Damage(99999999.9f);
        Object.Destroy(fakeKillShot);
        Invoke("finalHit_deadSans", 3.0f);
    }

    private void finalHit() // The player finally hits Sans. It's game over, man
    {
        fakeKillShot = Instantiate(playerShot, grabbingShieldTrans.position, Quaternion.identity);
        fakeKillShot.transform.LookAt(body.transform);
        Invoke("finalHit_pretendHit", 1.4f);
    }

    private void finalHit_prelim()
    {
        lib.AddTextToQueue("Heh, did ya really think you could--", finalHit);
    }

    private void wakeSans()
    {
        body.sprite = origSprite;
        isGrabbingButton = false;
        grabCollider.enabled = false;
        attackButtonRB.useGravity = false;
        attackButton.transform.position = attackButtonOriginPos;
        attackButton.transform.rotation = attackButtonOriginRot;
        attackButton.transform.localScale /= 2.0f;
    }

    private void theEnd_Sleep_dropButton()
    {
        attackButton.GetComponent<BoxCollider>().isTrigger = false;
        attackButtonRB.useGravity = true;
        grabCollider.enabled = true;
    }

    private void theEnd_sleep_forReal()
    {
        body.sprite = sleepSprite;
        attackButtonRB = attackButton.GetComponent<Rigidbody>();
        Invoke("theEnd_Sleep_dropButton", 1.6f);
    }

    private void theEnd_sleep()
    {
        Invoke("theEnd_sleep_forReal", 8.0f);
        //Invoke("theEnd_sleep_forReal", 1.0f);
    }

    private void theEnd_stealAttack_talk3_speech()
    {
        lib.AddTextToQueue("You've reached the end, okay?");
        lib.AddTextToQueue("There's nothing more for you here.");
        lib.AddTextToQueue("You won!");
        lib.AddTextToQueue("Great job!");
        lib.AddTextToQueue("So, uh, you know...");
        lib.AddTextToQueue("Go beat another game!");
        lib.AddTextToQueue("Good luck!");
        lib.AddTextToQueue("I'll, uh... *yawn*... be cheering for you.", theEnd_sleep);
    }

    private void theEnd_stealAttack_talk3()
    {
        Invoke("theEnd_stealAttack_talk3_speech", 8.0f);
        //Invoke("theEnd_stealAttack_talk3_speech", 1.0f);
    }

    private void theEnd_stealAttack_talk2_speech()
    {
        lib.AddTextToQueue("You'll get bored here, if you haven't already.");
        lib.AddTextToQueue("I know your type.");
        lib.AddTextToQueue("You'll just move on to the next game.");
        lib.AddTextToQueue("So... go on.", theEnd_stealAttack_talk3);
    }

    private void theEnd_stealAttack_talk2()
    {
        Invoke("theEnd_stealAttack_talk2_speech", 8.0f);
        //Invoke("theEnd_stealAttack_talk2_speech", 1.0f);
    }

    private void theEnd_stealAttack_talk()
    {
        lib.AddTextToQueue("Yep. That's right. It's literally nothing.");
        lib.AddTextToQueue("And it's not gonna be anything, either.");
        lib.AddTextToQueue("Heh heh heh... ya get it?");
        lib.AddTextToQueue("I know I can't beat you. One of these turns...");
        lib.AddTextToQueue("You're just gonna kill me.");
        lib.AddTextToQueue("So, uh, I've decided...");
        lib.AddTextToQueue("Maybe I better hold onto this.");
        lib.AddTextToQueue("And we're just going to sit in this menu 'til you give up.");
        lib.AddTextToQueue("Even if it means we stand here until the end of time.");
        lib.AddTextToQueue("Capiche?", theEnd_stealAttack_talk2);
    }

    private void theEnd_stealAttack()
    {
        Transform attackTrans = attackButton.transform;
        Instantiate(stealAttackSound, attackTrans.position, attackTrans.rotation);
        attackButtonOriginPos = attackTrans.position;
        attackButtonOriginRot = attackTrans.rotation;
        attackButtonOriginScale = attackTrans.localScale;

        attackTrans.position = handTrans.position;
        attackTrans.rotation = handTrans.rotation;
        attackTrans.localScale *= 2.0f;

        lib.RegisterCrossheirPositionFunc(FinishCrossheirs);
        lib.ForceLethalHit(true);
        lib.PlayerAttack(finalHit_prelim);

        lib.RegisterImmediateAttackCallback(wakeSans);

        Invoke("theEnd_stealAttack_talk", 5.0f);
        //Invoke("theEnd_stealAttack_talk", 1.0f);
    }

    private void theEnd()
    {
        lib.solidifyEnemy();
        isGrabbingButton = true;
        lib.AddTextToQueue("*Huff* *puff* *huff*");
        lib.AddTextToQueue("Alright, that's it.");
        lib.AddTextToQueue("It's time for my special attack.");
        lib.AddTextToQueue("Are you ready?");
        lib.AddTextToQueue("Here goes nothing.", theEnd_stealAttack);
    }

    private void specialAttack_cooldown()   // Spawns harmless blasters
    {
        lib.FadeoutMusic(0.0f, 12.0f);

        const float x = 15.0f;
        const float yHalfSpan = 20.0f;
        const int totalLaserCount = 20;
        const float initDelay = 0.133333333f;
        const float finalDelay = 0.7f;

        const float yGap = yHalfSpan / (float)totalLaserCount;

        float delay = 0.0f;

        for (int i = 0; i < totalLaserCount; i++)
        {
            delay += Mathf.Lerp(initDelay, finalDelay, (float)i / (float)totalLaserCount);
            StartCoroutine(spawnDudBlaster(new Vector2(x, yHalfSpan - (2* ((float)i * yGap))), 1.0f, delay));
        }
        lib.WaitForMusicFade(theEnd);
    }

    private void specialAttack_veryBadTime_2()  // Blaster sin wave - final challenge - EXTREMELY HARD, a very bad time indeed.
    {
        const int waveCount = 3;
        const float totalTime = 15.0f;
        const float yHalfSpan = 60.0f;
        const float waveAmp = 60.0f;
        const int lasersPerWave = 37;
        const int totalLaserCount = lasersPerWave * waveCount;
        const float delay = totalTime / (float)totalLaserCount;
        const float yGap = (2 * yHalfSpan) / totalLaserCount;

        const float xOff = 0.0f;

        const float cosShift = Mathf.PI * 0.5f;

        for(int i = 0; i < totalLaserCount; i++)
        {
            StartCoroutine(spawnMegaBlaster(new Vector2(((waveAmp / 2.0f) * Mathf.Abs(Mathf.Cos(cosShift + Mathf.PI * ((float)(i % lasersPerWave) / (float)lasersPerWave)) )) + xOff, (-1.0f * yHalfSpan) + ((float)i * yGap)), 1.0f, (float)i * delay));
        }
        Invoke("specialAttack_cooldown", delay * (totalLaserCount + 4));
    }

    private void specialAttack_veryBadTime()    // Blaster strafe
    {
        const float firstXStrafe = 0.0f;
        const int firstStrafeLaserCount = 45;
        const float firstYHalfSpan = 60.0f;
        const float firstStrafeTime = 6.0f;
        const float firstStrafeDelay = firstStrafeTime / (float)firstStrafeLaserCount;
        const float firstStrafeYGap = (firstYHalfSpan * 2.0f) / (float)firstStrafeLaserCount;
        for(int i = 0; i < firstStrafeLaserCount; i++)
        {
            StartCoroutine(spawnMegaBlaster(new Vector2(firstXStrafe, firstYHalfSpan - ((float)i * firstStrafeYGap)), 1.0f, (float)i * firstStrafeDelay));
        }
        Invoke("specialAttack_veryBadTime_2", firstStrafeLaserCount * firstStrafeDelay);
    }

    private void specialAttack_tripleBlast_spawn(bool horiz)
    {
        float x = Random.value * 40.0f;
        float y = numAttacksPerformedInSeq % 2 == 0 ? -40.0f : 40.0f;
        const float gap = 5.0f;
        if (horiz)
        {
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x, y - gap, 1.0f);
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x, y, 1.0f);
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x, y + gap, 1.0f);
        }
        else
        {
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x - gap, y, 1.0f);
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x, y, 1.0f);
            spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, x + gap, y, 1.0f);
        }
        Invoke("specialAttack_tripleBlast_loop", 1.35f);
    }

    private void specialAttack_tripleBlast_blast()
    {
        bool isHorizontal = Random.value > 0.5f;
        specialAttack_tripleBlast_spawn(isHorizontal);
    }

    private void specialAttack_tripleBlast_loop()
    {
        if (numAttacksPerformedInSeq == 4)
        {
            lib.WaitForProjectiles(specialAttack_veryBadTime);
            return;
        }
        cutout(0.2f, specialAttack_tripleBlast_blast);
        lib.ResetEnemyPos();
        if(numAttacksPerformedInSeq != 3)
            lib.TranslateEnemy(cutShuffle_randomPos());
        numAttacksPerformedInSeq++;
    }

    private void specialAttack_tripleBlast()
    {
        numAttacksPerformedInSeq = 0;
        specialAttack_tripleBlast_loop();
    }

    private void specialAttack_doubleRing_spawnRandomCircle()
    {
        float x = (int)(Random.value * 50);
        float y = (int)(Random.value * 40) + 10;
        // (Vector2 center, float dScale, float radius, int laserCount, float totalTime, bool clockWise)
        doubleRing1_spawnCircle(new Vector2(x, y), 1.0f, 8.50f, 8, 0.30f, false);
        doubleRing1_spawnCircle(new Vector2(50 - x, y * -1), 1.0f, 8.5f, 8, 0.35f, true);
    }

    private void specialAttack_doubleRing()
    {
        lib.fadeEnemy();
        float delay = 0.7f;
        for (int i = 0; i < 5; i++)
        {
            Invoke("specialAttack_doubleRing_spawnRandomCircle", delay * i);
        }
        Invoke("specialAttack_tripleBlast", 4.0f * 0.7f + timeToFly + 0.2f);
    }

    private void specialAttack_zigZag_4()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        
        Invoke("specialAttack_doubleRing",1.4f);
    }


    private void specialAttack_zigZag_3()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        zigZag_registerEndingPos(endArr);
        Invoke("specialAttack_zigZag_4", 1.0f);
    }

    private void specialAttack_zigZag_2()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        zigZag_registerEndingPos(endArr);
        Invoke("specialAttack_zigZag_3", 1.0f);
    }

    private void specialAttack_zigZag()
    {
        lib.fadeEnemy();
        Vector2 rCenter = cutShuffle1_zigZag_getRandomCenter();
        Vector2 lCenter = cutShuffle1_zigZag_getRandomCenter();
        lCenter.y *= -1;
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        zigZag_registerEndingPos(endArr);
        Invoke("specialAttack_zigZag_2", 1.0f);
    }

    private void specialAttack_xStrike()
    {
        lib.fadeEnemy();

        float delay = 0.6f;
        xStrike1_spawn();
        for (int i = 1; i < 4; i++)
        {
            Invoke("xStrike1_spawn", delay * i);
        }
        Invoke("specialAttack_zigZag", (delay * 4.0f + timeToFly) - 0.8f);
    }

    private void specialAttack()
    {
        lib.fadeEnemy();
        specialAttack_xStrike();
    }

    private void blasterHell3() // Special attack incoming boi
    {
        lib.fadeEnemy();
        float delay = 0.80f;
        blasterHell2_spawn();
        for (int i = 1; i < 20; i++)
        {
            Invoke("blasterHell2_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }

    private void snapRing3_blast() // 0.8 swing, 0.7 idle 0.5s blast, 0.8s sweepout
    {
        float x = (int)(Random.value * 45) - 5;
        float y = (int)(Random.value * 80) - 40;
        spawner.SpawnProjectileAtAngle(megaBlaster, x,y, 1.0f);
        Invoke("snapRing3_loop", 0.42f + 1.28f - 0.5f);
    }

    private void snapRing3_spawnRandomRing()
    {
        float x = (int)(Random.value * 45) - 5;
        float y = (int)(Random.value * 60) - 30;
        snapRing1_spawnRing(new Vector2(x, y), 1.0f, 4.0f, 5);
    }

    private void snapRing3_loop()
    {
        if(numAttacksPerformedInSeq == 3)
        {
            lib.WaitForProjectiles(playerTurn);
            return;
        }
        const float delay = 0.22f;
        const int numRings = 6;
        for (int i = 0; i < numRings; i++)
        {
            Invoke("snapRing3_spawnRandomRing", delay * i);
        }
        Invoke("snapRing3_blast", timeToFly + (numRings - 1) * delay - 1.38f);
        numAttacksPerformedInSeq++;
    }

    private void snapRing3()
    {
        lib.fadeEnemy();
        numAttacksPerformedInSeq = 0;
        snapRing3_loop();
    }

    private void cutShuffle3_doubleRing_spawnLeaders()
    {
        float x = (int)(Random.value * 45);
        float y = (int)(Random.value * 40) + 10;
        doubleRing1_spawnCircle(new Vector2(x, y), 0.58f, 8.50f, 8, 0.30f, false);
        doubleRing1_spawnCircle(new Vector2(x, y * -1), 0.58f, 8.50f, 8, 0.35f, true);
    }

    private void cutShuffle3_doubleRing_spawnRandomCircle()
    {
        float x = (int)(Random.value * 45);
        float y = (int)(Random.value * 40) + 10;
            // (Vector2 center, float dScale, float radius, int laserCount, float totalTime, bool clockWise)
        doubleRing1_spawnCircle(new Vector2(x, y), 1.0f, 8.50f, 8, 0.30f, false);
        doubleRing1_spawnCircle(new Vector2(x, y * -1), 1.0f, 8.5f, 8, 0.35f, true);
    }

    private void cutShuffle3_doubleRing()
    {
        lib.fadeEnemy();
        float delay = 0.6f;
        cutShuffle3_doubleRing_spawnLeaders();
        for (int i = 0; i < 5; i++) // 2 more hit
        {
            Invoke("cutShuffle3_doubleRing_spawnRandomCircle", delay * i);
        }
        Invoke("cutShuffle3_loop", 3.0f);
    }

    private Vector2 cutShuffle3_tripleBlast_getRandomCenter()
    {
        return new Vector2(Random.value * 50.0f - 0.5f, Random.value * 80.0f - 40.0f);
    }

    private void cutShuffle3_tripleBlast()
    {
        Vector2 center = cutShuffle1_tripleBlast_getRandomCenter();
        const float yGap = 5.0f;
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y - yGap, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y + yGap, 1.0f);
        Invoke("cutShuffle3_loop", 1.35f);
    }

    private void cutShuffle3_shuffleLines_dud()
    {
        Vector2 center1 = shuffleLines1_getRandomCenter();
        Vector2 center2 = shuffleLines1_getRandomCenter();
        Vector2 dir1 = shuffleLines1_getRandomDir();
        Vector2 dir2 = shuffleLines1_getRandomDir();
        Vector2 end1 = center1 + dir1;
        Vector2 end2 = center2 + dir2;
        shuffleLines1_correctEndPoint(ref end1, ref dir1);
        shuffleLines1_correctEndPoint(ref end2, ref dir2);
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, center1.x, center1.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(dir1.x, dir1.y, 0.75f, 1.0f, 0.09f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, center2.x, center2.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(dir2.x, dir2.y, 0.75f, 1.0f, 0.09f);
    }

    private void cutShuffle3_shuffleLines_loop()
    {
        Vector2 center1 = shuffleLines1_getRandomCenter();
        Vector2 center2 = shuffleLines1_getRandomCenter();
        Vector2 dir1 = shuffleLines1_getRandomDir();
        Vector2 dir2 = shuffleLines1_getRandomDir();
        Vector2 end1 = center1 + dir1;
        Vector2 end2 = center2 + dir2;
        shuffleLines1_correctEndPoint(ref end1, ref dir1);
        shuffleLines1_correctEndPoint(ref end2, ref dir2);
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, center1.x, center1.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(dir1.x, dir1.y, 0.75f, 1.0f, 0.09f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, center2.x, center2.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(dir2.x, dir2.y, 0.75f, 1.0f, 0.09f);
        numAttacksInSubSeq++;
        if (numAttacksInSubSeq != 2)
            Invoke("cutShuffle3_shuffleLines_loop", 1.35f);
        else
        {
            Invoke("cutShuffle3_loop", 2.10f);
            Invoke("cutShuffle3_shuffleLines_dud", 1.35f);
        }
    }

    private void cutShuffle3_shuffleLines()
    {
        lib.fadeEnemy();
        numAttacksInSubSeq = 0;
        cutShuffle3_shuffleLines_loop();
    }

    private Vector2 cutShuffle3_blasterHell_getRandomCenter()
    {
        return new Vector2((int)(Random.value * 45.0f) - 5, (int)(Random.value * 80.0f) - 40);
    }
    private void cutShuffle3_blasterHell_spawnLeader()
    {
        Vector2 center = blasterHell1_getRandomCenter();
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y, 1.0f);
    }

    private void cutShuffle3_blasterHell_spawn()
    {
        Vector2 center = blasterHell1_getRandomCenter();
        spawner.SpawnProjectileAtAngle(megaBlaster, center.x, center.y, 1.0f);
    }

    private void cutShuffle3_blasterHell() // 0.8 swing, 0.7 idle 0.5s blast, 0.8s sweepout
    {
        lib.fadeEnemy();
        float delay = 0.80f;
        const float offset = 0.8f;
        cutShuffle3_blasterHell_spawnLeader();
        for (int i = 1; i < 7; i++) // 4 + leader hits
        {
            Invoke("cutShuffle3_blasterHell_spawn", delay * i - offset);
        }
        Invoke("cutShuffle3_loop", 4.55f);
    }

    private OnCutoutFinishedEvent cutShuffle3_selectAttack()
    {
        int selector = (int)(Random.value * 4.0f);
        switch (selector)
        {
            case 0:
                return cutShuffle3_blasterHell;
            case 1:
                return cutShuffle3_shuffleLines;
            case 2:
                return cutShuffle3_tripleBlast;
            default:
                return cutShuffle3_doubleRing;
        }
    }

    private void cutShuffle3_loop()
    {
        if (numAttacksPerformedInSeq == 8)
        {
            numAttacksPerformedInSeq = 0;
            cutout(0.3f, playerTurn);
            lib.ResetEnemyPos();
            return;
        }
        cutout(0.2f, cutShuffle3_selectAttack());
        lib.ResetEnemyPos();
        lib.TranslateEnemy(cutShuffle_randomPos());
        numAttacksPerformedInSeq++;
    }

    private void cutShuffle3()
    {
        lib.fadeEnemy();
        numAttacksPerformedInSeq = 0;
        cutShuffle3_loop();
    }

    private bool shuffleLines1_isInBoundsX(Vector2 point)
    {
        if (point.x >= -5.0f && point.x <= 60.0f)
        {
            return true;
        }
        return false;
    }

    private bool shuffleLines1_isInBoundsY(Vector2 point)
    {
        if (point.y >= -80.0f && point.x <= 80.0f)
        {
            return true;
        }
        return false;
    }

    private void shuffleLines1_correctEndPoint(ref Vector2 point, ref Vector2 dir)
    {
        if (!shuffleLines1_isInBoundsX(point))
        {
            dir.x *= -1;
            point.x += dir.x * 2;
        }
        if (!shuffleLines1_isInBoundsY(point))
        {
            dir.y *= -1;
            point.y += dir.y * 2;
        }
    }

    private Vector2 shuffleLines1_getRandomDir()
    {
        const float length = 50.0f;
        float rads = Random.value * 2.0f * Mathf.PI;
        return new Vector2(Mathf.Sin(rads) * length, Mathf.Cos(rads) * length);
    }

    private Vector2 shuffleLines1_getRandomCenter()
    {
        return new Vector2(Random.value * 40.0f - 5.0f, Random.value * 80.0f - 40.0f);
    }

    private void shuffleLines1_loop()
    {
        if (numAttacksPerformedInSeq != 8)
        {
            Vector2 center1 = shuffleLines1_getRandomCenter();
            Vector2 center2 = shuffleLines1_getRandomCenter();
            Vector2 dir1 = shuffleLines1_getRandomDir();
            Vector2 dir2 = shuffleLines1_getRandomDir();
            Vector2 end1 = center1 + dir1;
            Vector2 end2 = center2 + dir2;
            shuffleLines1_correctEndPoint(ref end1, ref dir1);
            shuffleLines1_correctEndPoint(ref end2, ref dir2);
            GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, center1.x, center1.y, 0.75f);
            rCanon.GetComponent<CanonSeries>().Init(dir1.x, dir1.y, 0.75f, 1.0f, 0.09f);
            GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, center2.x, center2.y, 0.75f);
            lCanon.GetComponent<CanonSeries>().Init(dir2.x, dir2.y, 0.75f, 1.0f, 0.09f);
            numAttacksPerformedInSeq++;
            Invoke("shuffleLines1_loop", 1.4f);
        }
        else
        {
            lib.WaitForProjectiles(playerTurn);
        }
    }

    private void shuffleLines1()
    {
        lib.fadeEnemy();
        numAttacksPerformedInSeq = 0;
        shuffleLines1_loop();
    }

    private void blasterHell2_spawn()
    {
        const float yOff = 5.5f;
        Vector2 center = blasterHell1_getRandomCenter();
        spawner.SpawnProjectileAtAngle(megaBlaster, center.x, center.y - yOff, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlaster, center.x, center.y + yOff, 1.0f);
    }

    private void blasterHell2() // 1.5s warmup, 0.5s blast, 0.8s sweepout
    {
        lib.fadeEnemy();
        float delay = 0.85f;
        blasterHell2_spawn();
        for (int i = 1; i < 15; i++)
        {
            Invoke("blasterHell2_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }

    private bool zigZag2_isInBoundsX(Vector2 point)
    {
        if(point.x >= -10.0f && point.x <= 70.0f)
        {
            return true;
        }
        return false;
    }

    private bool zigZag2_isInBoundsY(Vector2 point)
    {
        if (point.y >= -80.0f && point.x <= 80.0f)
        {
            return true;
        }
        return false;
    }

    private void zigZag_correctEndPoint(ref Vector2 point, ref Vector2 dir)
    {
        if (!zigZag2_isInBoundsX(point))
        {
            dir.x *= -1;
            point.x += dir.x * 2;
        }
        if (!zigZag2_isInBoundsY(point))
        {
            dir.y *= -1;
            point.y += dir.y * 2;
        }
    }

    private Vector2 zigZag2_getRandomDir()
    {
        const float length = 45.0f;
        float rads = Random.value * 2.0f * Mathf.PI;
        return new Vector2(Mathf.Sin(rads) * length, Mathf.Cos(rads) * length);
    }

    private void zigZag2_3()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);

        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);

        StartCoroutine(spawnMegaBlaster(blasterHell1_getRandomCenter(), 1.0f, 0.82f));

        lib.WaitForProjectiles(playerTurn);
    }

    private void zigZag2_2()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        zigZag_registerEndingPos(endArr);
        Invoke("zigZag2_3", 1.0f);
    }

    private void zigZag2()
    {
        lib.fadeEnemy();
        Vector2 rCenter = cutShuffle1_zigZag_getRandomCenter();
        Vector2 lCenter = cutShuffle1_zigZag_getRandomCenter();
        lCenter.y *= -1;
        Vector2 rDir = zigZag2_getRandomDir();
        Vector2 lDir = zigZag2_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.08f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.08f);
        zigZag_registerEndingPos(endArr);
        Invoke("zigZag2_2", 1.0f);
    }

    private void cutShuffle2_doubleRing()
    {
        lib.fadeEnemy();
        float delay = 0.7f;
        float graceTime = 0.4f;
        cutShuffle1_doubleRing_spawnLeaders();
        for (int i = 0; i < 4; i++) // 2 more hit
        {
            Invoke("doubleRing1_spawnRandomCircle", delay * i + graceTime);
        }
        Invoke("cutShuffle2_loop", 3.4f);
    }

    private void cutShuffle2_zigZag_2()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 lDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.1f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.1f);
        Invoke("cutShuffle2_loop", 1.85f);
    }

    private void cutShuffle2_zigZag()
    {
        Vector2 rCenter = cutShuffle1_zigZag_getRandomCenter();
        Vector2 lCenter = cutShuffle1_zigZag_getRandomCenter();
        lCenter.y *= -1;
        Vector2 rDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 lDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.1f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.1f);
        zigZag_registerEndingPos(endArr);
        Invoke("cutShuffle2_zigZag_2", 1.0f);
    }

    private void cutShuffle2_tripleBlast()
    {
        Vector2 center = cutShuffle1_tripleBlast_getRandomCenter();
        const float yGap = 5.0f;
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y - yGap, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y + yGap, 1.0f);
        Invoke("cutShuffle2_loop", 1.35f);
    }

    private void cutShuffle2_bulletHell()
    {
        float delay = 0.25f;
        cutShuffle1_bulletHell_spawnLeaders();
        for (int i = 1; i < 9 + 6; i++)
        {
            Invoke("bulletHellBlast1_spawn", delay * i);
        }
        Invoke("cutShuffle2_loop", 3.5f);
    }

    private OnCutoutFinishedEvent cutShuffle2_selectAttack()
    {
        int selector = (int)(Random.value * 4.0f);
        switch (selector)
        {
            case 0:
                return cutShuffle2_bulletHell;
            case 1:
                return cutShuffle2_tripleBlast;
            case 2:
                return cutShuffle2_zigZag;
            default:
                return cutShuffle2_doubleRing;
        }
    }

    private void cutShuffle2_loop()
    {
        if (numAttacksPerformedInSeq == 7)
        {
            numAttacksPerformedInSeq = 0;
            cutout(0.3f, playerTurn);
            lib.ResetEnemyPos();
            return;
        }
        cutout(0.3f, cutShuffle2_selectAttack());
        lib.ResetEnemyPos();
        lib.TranslateEnemy(cutShuffle_randomPos());
        numAttacksPerformedInSeq++;
    }

    private void cutShuffle2()
    {
        lib.fadeEnemy();
        cutShuffle2_loop();
    }

    private Vector2 blasterHell1_getRandomCenter()
    {
        return new Vector2((int)(Random.value * 45.0f) - 5, (int)(Random.value * 80.0f) - 40);
    }

    private void blasterHell1_spawn()
    {
        Vector2 center = blasterHell1_getRandomCenter();
        spawner.SpawnProjectileAtAngle(megaBlaster, center.x, center.y, 1.0f);
    }

    private void blasterHell1() // 1.5s warmup, 0.5s blast, 0.8s sweepout
    {
        lib.fadeEnemy();
        float delay = 0.85f;
        blasterHell1_spawn();
        for (int i = 1; i < 15; i++)
        {
            Invoke("blasterHell1_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }

    private void cutShuffle1_doubleRing_spawnLeaders()
    {
        float x = (int)(Random.value * 40) - 2;
        float y = (int)(Random.value * 30) + 10;
        doubleRing1_spawnCircle(new Vector2(x, y), 0.79f, 8.50f, 8, 0.30f, false);
        doubleRing1_spawnCircle(new Vector2(x, y * -1), 0.79f, 8.50f, 8, 0.35f, true);
    }

    private void cutShuffle1_doubleRing()
    {
        lib.fadeEnemy();
        float delay = 0.7f;
        float graceTime = 0.4f;
        cutShuffle1_doubleRing_spawnLeaders();
        for (int i = 0; i < 4; i++) // 2 more hit
        {
            Invoke("doubleRing1_spawnRandomCircle", delay * i + graceTime);
        }
        Invoke("cutShuffle1_loop", 3.4f);
    }

    private Vector2 cutShuffle1_zigZag_getRandomDir()
    {
        const float length = 30.0f;
        float rads = Random.value * 2.0f * Mathf.PI;
        return new Vector2(Mathf.Cos(rads) * length, Mathf.Sin(rads) * length);
    }

    private Vector2 cutShuffle1_zigZag_getRandomCenter()
    {
        const float xOff = 10.0f, yOff = 5.0f;
        return new Vector2(Random.value * 20.0f + xOff, Random.value * 20.0f + yOff);
    }

    private void cutShuffle1_zigZag_2()
    {
        Vector2 rCenter = prevZigEndPos[0];
        Vector2 lCenter = prevZigEndPos[1];
        Vector2 rDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 lDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.1f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.1f);
        Invoke("cutShuffle1_loop", 1.85f);
    }

    private void zigZag_registerEndingPos(Vector2[] endingPos)
    {
        prevZigEndPos = endingPos;
    }

    private void cutShuffle1_zigZag()
    {
        Vector2 rCenter = cutShuffle1_zigZag_getRandomCenter();
        Vector2 lCenter = cutShuffle1_zigZag_getRandomCenter();
        lCenter.y *= -1;
        Vector2 rDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 lDir = cutShuffle1_zigZag_getRandomDir();
        Vector2 rEnd = rCenter + rDir;
        Vector2 lEnd = lCenter + lDir;
        zigZag_correctEndPoint(ref rEnd, ref rDir);
        zigZag_correctEndPoint(ref lEnd, ref lDir);
        Vector2[] endArr = { rEnd, lEnd };
        GameObject rCanon = spawner.SpawnProjectileAtAngle(megaCanon, rCenter.x, rCenter.y, 0.75f);
        rCanon.GetComponent<CanonSeries>().Init(rDir.x, rDir.y, 0.75f, 1.0f, 0.1f);
        GameObject lCanon = spawner.SpawnProjectileAtAngle(megaCanon, lCenter.x, lCenter.y, 0.75f);
        lCanon.GetComponent<CanonSeries>().Init(lDir.x, lDir.y, 0.75f, 1.0f, 0.1f);
        zigZag_registerEndingPos(endArr);
        Invoke("cutShuffle1_zigZag_2", 1.0f);
    }

    private Vector3 cutShuffle_randomPos()
    {
        float xOff = Random.value * 6.0f - 3.0f;
        float zOff = Random.value * 4.0f - 2.0f;
        return new Vector3(xOff, 0.0f, zOff);
    }

    private Vector2 cutShuffle1_tripleBlast_getRandomCenter()
    {
        return new Vector2(Random.value * 45.0f, Random.value * 60.0f - 30.0f);
    }

    private void cutShuffle1_tripleBlast()
    {
        Vector2 center = cutShuffle1_tripleBlast_getRandomCenter();
        const float yGap = 5.0f;
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y - yGap, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, center.x, center.y + yGap, 1.0f);
        Invoke("cutShuffle1_loop", 1.35f);
    }

    private void cutShuffle1_bulletHell_spawnLeaders()
    {
        const float dOff = 0.175f;
        for (int i = 0; i < 3; i++)
        {
            Vector2 center = bulletHellBlase1_getRandomCenter();
            spawner.SpawnProjectileAtAngle(megaLaser, center.x, center.y, 1.0f - (dOff * i));
        }
    }

    private void cutShuffle1_bulletHell()
    {
        float delay = 0.25f;
        cutShuffle1_bulletHell_spawnLeaders();
        for (int i = 1; i < 9 + 6; i++)
        {
            Invoke("bulletHellBlast1_spawn", delay * i);
        }
        Invoke("cutShuffle1_loop", 3.5f);
    }

    private OnCutoutFinishedEvent cutShuffle1_selectAttack()
    {
        int selector = (int)(Random.value * 4.0f);
        switch(selector)
        {
            case 0:
                return cutShuffle1_bulletHell;
            case 1:
                return cutShuffle1_tripleBlast;
            case 2:
                return cutShuffle1_zigZag;
            default:
                return cutShuffle1_doubleRing;
        }
    }

    private void cutShuffle1_loop()
    {
        if (numAttacksPerformedInSeq == 5)
        {
            numAttacksPerformedInSeq = 0;
            cutout(0.4f, playerTurn);
            lib.ResetEnemyPos();
            return;
        }
        cutout(0.4f, cutShuffle1_selectAttack());
        lib.ResetEnemyPos();
        lib.TranslateEnemy(cutShuffle_randomPos());
        numAttacksPerformedInSeq++;
    }

    private void cutShuffle1()
    {
        lib.fadeEnemy();
        cutShuffle1_loop();
    }

    private void phase2_intro()
    {
        lib.PlayMusic((AudioClip)Resources.Load("MEGALOVANIA"));
        cutShuffle1();
    }

    private void ontoPhase2()
    {
        lib.StopMusic();
        lib.RegisterCrossheirPositionFunc(CrossheirPosition);
        lib.AddTextToQueue("Welp, it was worth a shot.");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("Let's get this fight started then, shall we?", phase2_intro);
    }

    private void cheapKill_kill()
    {
        const int numRows = 6, numCols = 12;
        const float xSpan = 50, ySpan = 80;
        for(int x = 0; x < numRows; x++)
        {
            for(int y = 0; y < numCols; y++)
            {
                spawner.SpawnProjectileAtAngle(megaBlaster, xSpan - ((x * xSpan) / (numRows - 1)), ySpan - ((y * ySpan * 2) / (numCols - 1)), 1.0f);
            }
        }
    }

    private void cheapKill()
    {
        lib.StopMusic();
        lib.AddTextToQueue("Wow.");
        lib.AddTextToQueue("You're having mercy on me?");
        lib.AddTextToQueue("Finally.");
        lib.AddTextToQueue("Buddy, pal, friend, chum.");
        lib.AddTextToQueue("I know how hard this must be.");
        lib.AddTextToQueue("To go back on everything you've worked up to");
        lib.AddTextToQueue("So, I, uh, just want you to know--");
        lib.AddTextToQueue("I won't let this go to waste.");
        lib.AddTextToQueue("C'mere, pal.", cheapKill_kill);
    }

    private void fakeSurrender_choice()
    {
        lib.RegisterCrossheirPositionFunc(FinishCrossheirs);
        lib.PlayerAttack(ontoPhase2, true, cheapKill, false, false);
    }

    private void fakeSurrender_music()
    {
        lib.PlayMusic((AudioClip)Resources.Load("The Choice"));
        lib.AddTextToQueue("Listen...");
        lib.AddTextToQueue("I don't remember if we ever spoke before all this anymore...");
        lib.AddTextToQueue("But somewhere in there, I can *huff* *puff* feel it...");
        lib.AddTextToQueue("There's a person who wants to do the right thing.");
        lib.AddTextToQueue("We can... uh, we can stop all this.");
        lib.AddTextToQueue("Let's just sit around, telling jokes like pals again.");
        lib.AddTextToQueue("Remember?");
        lib.AddTextToQueue("Here's a *huff* *puff* good one...");
        lib.AddTextToQueue("What does a skeleton hate most *gasp* about the wind?");
        lib.AddTextToQueue("*huff* *puff*");
        lib.AddTextToQueue("Nothing. It, uh, *wheeze* it goes right through him!");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("So, whadda ya say, friend?");
        lib.AddTextToQueue("Just lay down your, uh, your weapon, and-- well...");
        lib.AddTextToQueue("This could all be *huff* a lot easier.", fakeSurrender_choice);
    }

    private void fakeSurrender()
    {
        lib.StopMusic();
        lib.AddTextToQueue("*Huff* *puff* but, uh, that being said...");
        lib.AddTextToQueue("You, uh, really like throwing that thing around, huh?", fakeSurrender_music);
    }

    private void snapRing2_spawnRandomRing()
    {
        float x = (int)(Random.value * 50) - 0;
        float y = (int)(Random.value * 80) - 40;
        snapRing1_spawnRing(new Vector2(x, y), 1.0f, 4.0f, 5);
    }

    private void snapRing2()
    {
        lib.fadeEnemy();
        float delay = 0.22f;
        for (int i = 0; i < 20; i++)
        {
            Invoke("snapRing2_spawnRandomRing", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }

    private void doubleRing1_spawnCircle(Vector2 center, float dScale, float radius, int laserCount, float totalTime, bool clockWise)
    {
        const float TWOPI = 2 * Mathf.PI;
        float delay = ((float)totalTime) / ((float)laserCount);
        for (int i = 0; i < laserCount; i++)
        {
            float rads = (((float)i) / ((float)laserCount)) * TWOPI;
            float pitchOff = Mathf.Sin(rads) * radius;
            float yawOff = Mathf.Cos(rads) * radius;
            if(clockWise == false)
            {
                yawOff *= -1;
            }
            StartCoroutine(spawnMegaLaser(new Vector2(center.x + pitchOff, center.y + yawOff), dScale, delay * (float)i));
        }
    }

    private void doubleRing1_spawnRandomCircle()
    {
        float x = (int)(Random.value * 40) - 2;
        float y = (int)(Random.value * 30) + 10;
        doubleRing1_spawnCircle(new Vector2(x, y), 1.0f, 8.50f, 8, 0.30f, false);
        doubleRing1_spawnCircle(new Vector2(x, y * -1), 1.0f, 8.5f, 8, 0.35f, true);
    }

    private void doubleRing1()
    {
        lib.fadeEnemy();
        float delay = 0.6f;
        doubleRing1_spawnRandomCircle();
        for (int i = 1; i < 6; i++)
        {
            Invoke("doubleRing1_spawnRandomCircle", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }

    private void tracer2_blast()
    {
        spawner.SpawnProjectileAtAngle(megaBlaster, 30, 0, 1.0f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void tracer2_4()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 40, 1.0f);  // Up-Left
        c.GetComponent<CanonSeries>().Init(25, -40, 1.0f, 1.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 40, -40, 1.0f);  // Down-Right
        c.GetComponent<CanonSeries>().Init(-25, 40, 1.0f, 1.0f, 0.05f);
        Invoke("tracer2_blast", 0.8f);
    }

    private void tracer2_3()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 0, -40, 1.0f);  // Right
        c.GetComponent<CanonSeries>().Init(0, 80, 1.0f, 2.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 40, 40, 1.0f);  // Left
        c.GetComponent<CanonSeries>().Init(0, -80, 1.0f, 2.0f, 0.05f);
        Invoke("tracer2_4", 2.0f);
    }

    private void tracer2_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, -40, 1.0f);  // Down
        c.GetComponent<CanonSeries>().Init(-40, 0, 1.0f, 1.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 40, 1.0f);  // Up
        c.GetComponent<CanonSeries>().Init(40, 0, 1.0f, 1.0f, 0.05f);
        Invoke("tracer2_3", 1.0f);
    }

    private void tracer2()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, 0, 1.0f);  // Left
        c.GetComponent<CanonSeries>().Init(0, -40, 1.0f, 1.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 0, 1.0f);  // Right
        c.GetComponent<CanonSeries>().Init(0, 40, 1.0f, 1.0f, 0.05f);
        Invoke("tracer2_2", 1.0f);
    }

    private void xStrike1_spawn()
    {
        const int lasersPerSlash = 5;   // We assume >= 2
        const float xDist = 20, yDist = 20;
        float delay = 0.1f;

        float y = ((int)(Random.value * 60.0f) - 30);
        float x = ((int)(Random.value * 40.0f) - 5);
        Vector2 center = new Vector2(x, y);
        Vector2 origin = center - new Vector2(xDist / 2, yDist / 2);
        
        const float xGap = xDist / (lasersPerSlash - 1);
        const float yGap = yDist / (lasersPerSlash - 1);

        int randomDir = (int)(Random.value * 4.0f);
        for(int i = 0; i < lasersPerSlash; i++)
        {
            switch (randomDir)
            {
                case 0: // Left -> right
                    StartCoroutine(spawnMegaLaser(new Vector2(origin.x + xGap * i, origin.y + yGap * i), 1.0f, delay * (float)i));
                    StartCoroutine(spawnMegaLaser(new Vector2((origin.x + xDist) - xGap * i, origin.y + yGap * i), 1.0f, delay * (float)i));
                    break;
                case 1: // Up -> Down
                    StartCoroutine(spawnMegaLaser(new Vector2((origin.x + xDist) - xGap * i, (origin.y + yDist) - yGap * i), 1.0f, delay * (float)i));
                    StartCoroutine(spawnMegaLaser(new Vector2((origin.x + xDist) - xGap * i, origin.y + yGap * i), 1.0f, delay * (float)i));
                    break;
                case 2: // Right -> Left
                    StartCoroutine(spawnMegaLaser(new Vector2(origin.x + xGap * i, (origin.y + yDist) - yGap * i), 1.0f, delay * (float)i));
                    StartCoroutine(spawnMegaLaser(new Vector2((origin.x + xDist) - xGap * i, (origin.y + yDist) - yGap * i), 1.0f, delay * (float)i));
                    break;
                case 3: // Down -> Up
                    StartCoroutine(spawnMegaLaser(new Vector2(origin.x + xGap * i, (origin.y + yDist) - yGap * i), 1.0f, delay * (float)i));
                    StartCoroutine(spawnMegaLaser(new Vector2(origin.x + xGap * i, origin.y + yGap * i), 1.0f, delay * (float)i));
                    break;
            }
            

            //spawner.SpawnProjectileAtAngle(megaLaser, origin.x + xGap * i, origin.y + yGap * i, 1.0f);
            //spawner.SpawnProjectileAtAngle(megaLaser, (origin.x + xDist) -  xGap * i, origin.y + yGap * i, 1.0f);
        }
    }

    private void xStrike1()
    {
        lib.fadeEnemy();
        
        float delay = 0.8f;
        xStrike1_spawn();
        for (int i = 1; i < 10; i++)
        {
            Invoke("xStrike1_spawn", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }

    private Vector2 bulletHellBlase1_getRandomCenter()
    {
        return new Vector2((int)(Random.value * 45.0f) - 5, (int)(Random.value * 80.0f) - 40);
    }

    private void bulletHellBlast1_spawn()
    {
        Vector2 center = bulletHellBlase1_getRandomCenter();
        spawner.SpawnProjectileAtAngle(megaLaser, center.x, center.y, 1.0f);
    }

    private void bulletHellBlast1()
    {
        lib.fadeEnemy();
        float delay = 0.25f;
        bulletHellBlast1_spawn();
        for (int i = 1; i < 40; i++)
        {
            Invoke("bulletHellBlast1_spawn", delay * i);
        }
        lib.WaitForProjectiles(playerTurn);
    }

    private void crossBlast1_blast2()
    {
        spawner.SpawnProjectileAtAngle(megaBlaster, 20, -10, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlaster, 20, 10, 1.0f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void crossBlast1_round2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, 40, 1.0f);  // Right
        c.GetComponent<CanonSeries>().Init(0, -80, 1.0f, 2.0f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, -40, 1.0f);  // Left
        c.GetComponent<CanonSeries>().Init(0, 80, 1.0f, 2.0f, 0.1f);
        Invoke("crossBlast1_blast2", 2.2f);
    }

    private void crossBlast1_blast1()
    {
        spawner.SpawnProjectileAtAngle(megaBlaster, 20, -10, 1.0f);
        spawner.SpawnProjectileAtAngle(megaBlaster, 20, 10, 1.0f);
        Invoke("crossBlast1_round2", 1.5f);
    }

    private void crossBlast1()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, -40, 1.0f);  // Left
        c.GetComponent<CanonSeries>().Init(0, 80, 1.0f, 2.0f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 40, 1.0f);  // Right
        c.GetComponent<CanonSeries>().Init(0, -80, 1.0f, 2.0f, 0.1f);
        Invoke("crossBlast1_blast1", 2.2f);
    }

    private void snapRing1_spawnRing(Vector2 center, float dScale, float radius, int laserCount)
    {
        const float TWOPI = 2 * Mathf.PI;
        for (int i = 0; i < laserCount; i++)
        {
            float rads = (((float)i) / ((float)laserCount)) * TWOPI;
            float pitchOff = Mathf.Sin(rads) * radius;
            float yawOff = Mathf.Cos(rads) * radius;
            snapRing1_spawnLaser(new Vector2(center.x + pitchOff, center.y + yawOff), dScale);
        }
    }

    private void snapRing1_spawnRandomRing()
    {
        float x = (int)(Random.value * 50) - 0;
        float y = (int)(Random.value * 80) - 40;
        snapRing1_spawnRing(new Vector2(x, y), 1.0f, 3.0f, 3);
    }

    private void snapRing1_spawnLaser(Vector2 pos, float dScale)
    {
        spawner.SpawnProjectileAtAngle(megaLaser, pos.x, pos.y, dScale);
    }

    private void snapRing1()
    {
        lib.fadeEnemy();
        float delay = 0.22f;
        for (int i = 0; i < 14; i++)
        {
            Invoke("snapRing1_spawnRandomRing", delay * i);
        }
        Invoke("returnToPlayerTurn", 0.3f);
    }

    private void tracer1_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, -40, 1.0f);  // Down
        c.GetComponent<CanonSeries>().Init(-40, 0, 1.0f, 1.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 40, 1.0f);  // Up
        c.GetComponent<CanonSeries>().Init(40, 0, 1.0f, 1.0f, 0.05f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void tracer1()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, 0, 1.0f);  // Left
        c.GetComponent<CanonSeries>().Init(0, -40, 1.0f, 1.0f, 0.05f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 0, 1.0f);  // Right
        c.GetComponent<CanonSeries>().Init(0, 40, 1.0f, 1.0f, 0.05f);
        Invoke("tracer1_2", 1.0f);
    }

    private void attackSelect()
    {
        /*lib.AddTextToQueue("Survive THIS, and I'll show you my special attack!", theEnd);
        lib.StopMusic();*/
        switch (turnCount)
        {
            case 1:
                lib.AddTextToQueue("What? You think I'm gonna just stand here and take it?", tracer1);
                break;
            case 2:
                lib.AddTextToQueue("So why are we doing this, anyway?");
                lib.AddTextToQueue("Did you just want to check out some VR demo?", snapRing1);
                break;
            case 3:
                lib.AddTextToQueue("Hehehe.");
                lib.AddTextToQueue("That's all we are, isn't it?");
                lib.AddTextToQueue("A few chumps to satisfy your curiosity.", crossBlast1);
                break;
            case 4:
                lib.AddTextToQueue("You can't understand how this feels.", bulletHellBlast1);
                break;
            case 5:
                lib.AddTextToQueue("Knowing that anything you can ever do...");
                lib.AddTextToQueue("Won't matter once someone hits \"Quit\".", xStrike1);
                break;
            case 6:
                lib.AddTextToQueue("To be honest...");
                lib.AddTextToQueue("It makes it kinda hard to give this my all.", tracer2);
                break;
            case 7:
                lib.AddTextToQueue("And beating you isn't appealing anymore, either.", doubleRing1);
                break;
            case 8:
                lib.AddTextToQueue("'Cause even if I do...");
                lib.AddTextToQueue("I'll just end up right back here without any memory, right?", snapRing2);
                break;
            case 9:
                fakeSurrender();
                break;
            case 10:
                lib.AddTextToQueue("You know, before all this I was hoping we could be friends.", blasterHell1);
                break;
            case 11:
                lib.AddTextToQueue("I thought that maybe you were just unhappy.");
                lib.AddTextToQueue("If we could just give you what you wanted...");
                lib.AddTextToQueue("You might stop all this.", cutShuffle2);
                break;
            case 12:
                lib.AddTextToQueue("And maybe all you needed was... I dunno.");
                lib.AddTextToQueue("Some good friends?");
                lib.AddTextToQueue("Some bad laughs?", zigZag2);
                break;
            case 13:
                lib.AddTextToQueue("But that's ridiculous, right?");
                lib.AddTextToQueue("Yeah, you're the sort of person who won't EVER be happy.", blasterHell2);
                break;
            case 14:
                lib.AddTextToQueue("You'll keep destroying world after world, until...");
                lib.AddTextToQueue("Well, take it from me, kid...");
                lib.AddTextToQueue("That's not gonna happen on my watch.", shuffleLines1);
                break;
            case 15:
                lib.AddTextToQueue("Sometimes, you just gotta learn when to QUIT.");
                lib.AddTextToQueue("Such as, say, RIGHT NOW.", cutShuffle3);
                break;
            case 16:
                lib.AddTextToQueue("Cause, you see...");
                lib.AddTextToQueue("All this sliding around is really tiring me out.", snapRing3);
                break;
            case 17:
                lib.AddTextToQueue("And if you keep pushing me...");
                lib.AddTextToQueue("I'm gonna have to show you my special attack.", cutShuffle3);
                break;
            case 18:
                lib.AddTextToQueue("*Huff* *puff* Yeah, my special attack.");
                lib.AddTextToQueue("So, uh, if you don't want to see it...");
                lib.AddTextToQueue("Feel free to die now.", blasterHell3);
                break;
            case 19:
                lib.AddTextToQueue("Alright.");
                lib.AddTextToQueue("Well, here goes nothing...");
                lib.AddTextToQueue("Are you ready?");
                lib.AddTextToQueue("Survive THIS, and I'll show you my special attack!", specialAttack);
                break;
            // RIP Sans
        }
    }

    private void returnToPlayerTurn()
    {
        lib.WaitForProjectiles(playerTurn);
    }

    private void playerTurn()
    {
        lib.solidifyEnemy();
        turnCount++;
        lib.PlayerAttack(attackSelect);
    }

    private void getThisStarted()
    {
        lib.SetPitch(1.0f);
        lib.PlayMusic((AudioClip)Resources.Load("MEGALOVANIA"));
        //lib.SetPitch(1.0f);
        //turnCount = 8;
        playerTurn();
        attackButton = GameObject.Find("AttackButton");
    }

    private void firstBlood_done()
    {
        lib.solidifyEnemy();
        lib.AddTextToQueue("Well look at you, hotshot.");
        lib.AddTextToQueue("It's almost like we've done this before.", getThisStarted);
        //cutout(0.8f, firstBlood_suprise, true);
    }

    private void firstBlood_psychSave()
    {
        cutout(0.8f, firstBlood_done, true);
    }

    private void firstBlood_psychOut()  // These will be cut away from before they hit
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 40, -50, 1.0f);
        c.GetComponent<CanonSeries>().Init(-50, 100, 1.0f, 2.0f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 40, 50, 1.0f);
        c.GetComponent<CanonSeries>().Init(-50, -100, 1.0f, 2.0f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 10, -50, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 100, 1.0f, 1.0f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 10, 50, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, -100, 1.0f, 1.0f, 0.1f);
        Invoke("firstBlood_psychSave", 1.2f);
    }

    private void firstBlood_round2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 15, 50, 0.7f); // Left
        c.GetComponent<CanonSeries>().Init(0, -50, 0.8f, 1.5f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 0, 25, 0.7f); // Up
        c.GetComponent<CanonSeries>().Init(50, 0, 0.8f, 1.5f, 0.1f);

        Invoke("firstBlood_psychOut", 1.5f);
    }
    
    private void firstBlood_suprise()
    {
        spawner.SpawnProjectileAtAngle(megaBlasterNoSwing, 20, 20, 1.0f);
        Invoke("firstBlood_round2", 0.6f);
    }

    private void firstBlood_earlyCut1()
    {
        cutout(0.8f, firstBlood_suprise, true);
    }

    private void firstBlood_blast1()
    {
        spawner.SpawnProjectileAtAngle(megaBlaster, 20, -20, 1.0f);
        Invoke("firstBlood_earlyCut1", 2.0f);
    }

    private void firstBlood()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(megaCanon, 15, -50, 0.7f); // Right
        c.GetComponent<CanonSeries>().Init(0, 50, 0.8f, 1.5f, 0.1f);
        c = spawner.SpawnProjectileAtAngle(megaCanon, 50, -25, 0.7f); // Down
        c.GetComponent<CanonSeries>().Init(-50, 0, 0.8f, 1.5f, 0.1f);

        Invoke("firstBlood_blast1", 1.25f);
    }

    private void postFirstCut()
    {
        lib.AddTextToQueue("I'm gonna drag you screaming to hell.", firstBlood);
    }

    private void firstCut()
    {
        lib.StopMusic();
        cutout(1.0f, postFirstCut, true);
    }

    private void contFight3()
    {
        lib.FadePitch(0.70f, 2.0f);
        lib.AddTextToQueue("Welp.");
        lib.AddTextToQueue("Sorry, kid.");
        lib.AddTextToQueue("But, you see...", firstCut);
    }

    private void fakeMercy3()
    {
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("What a loser you are.", spare);
    }

    private void fakeOptions3()
    {
        lib.PlayerAttack(contFight3, false, fakeMercy3, true, true);
    }

    private void contFight2()
    {
        lib.FadePitch(0.78f, 2.0f);
        lib.AddTextToQueue("Oh boy.");
        lib.AddTextToQueue("Well, not that you deserve the warning or anything...");
        lib.AddTextToQueue("But if you take one more step forward...");
        lib.AddTextToQueue("You are REALLY not going to like what happens next.", fakeOptions3);
    }

    private void fakeMercy2()
    {
        lib.AddTextToQueue("Heh.");
        lib.AddTextToQueue("I guess that's for everyone else, huh?");
        lib.AddTextToQueue("Get out of here.");
        lib.AddTextToQueue("You disgust me.", spare);
    }

    private void fakeOptions2()
    {
        lib.PlayerAttack(contFight2, false, fakeMercy2, true, true);
    }

    private void contFight1()
    {
        lib.FadePitch(0.85f, 2.0f);
        lib.AddTextToQueue("Heh heh heh heh...");
        lib.AddTextToQueue("Alright.");
        lib.AddTextToQueue("Well, here's a better question.");
        lib.AddTextToQueue("Do you wanna have a bad time?", fakeOptions2);
    }

    private void fakeMercy1()
    {
        lib.AddTextToQueue("Ah.");
        lib.AddTextToQueue("I see.");
        lib.AddTextToQueue("Then why'd you kill my brother?", spare);
    }

    private void fakeOptions1()
    {
        lib.PlayerAttack(contFight1, false, fakeMercy1, true, true);
    }

    private void fightIntro()
    {
        lib.PlayMusic((AudioClip)Resources.Load("Small Shock"));
        lib.FadePitch(0.93f, 2.0f);
        lib.AddTextToQueue("Hey there, kid.");
        lib.AddTextToQueue("You've been pretty busy, huh?");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("So, I've got a question for ya.");
        lib.AddTextToQueue("Do you think that even the worst person can change?");
        lib.AddTextToQueue("That everyone can be a good person, if they just try?", fakeOptions1);
    }

    private void begin()
    {
        //getThisStarted();
        switch (totalKillCount)
        {
            case 0:
                lib.AddTextToQueue("Hey there, kid.");
                lib.AddTextToQueue("So, uh, how did someone like you end up down here?");
                lib.AddTextToQueue("...");
                lib.AddTextToQueue("Wanna hear a joke?");
                lib.AddTextToQueue("A horse walks into a bar.");
                lib.AddTextToQueue("The bartender says, \"What're you havin'?\"");
                lib.AddTextToQueue("The horse says, \"Malt whiskey, straight up\"");
                lib.AddTextToQueue("And the bartender says, uh...");
                lib.AddTextToQueue("He says...");
                lib.AddTextToQueue("I forgot.");
                lib.AddTextToQueue("Honestly it wasn't even that great of a joke anyway.");
                lib.AddTextToQueue("But, uh, if you haven't yet, you should talk with Papyrus.");
                lib.AddTextToQueue("He's my brother.");
                lib.AddTextToQueue("He's pretty great.");
                lib.AddTextToQueue("Tell him hi from me, Sans, if you see him!");
                lib.AddTextToQueue("Well, catch ya later kid.", spare);
                break;
            case 1:
                lib.AddTextToQueue("Hey there, kid.");
                lib.AddTextToQueue("...");
                lib.AddTextToQueue("Wanna hear a joke?");
                lib.AddTextToQueue("A human walks into a bar");
                lib.AddTextToQueue("The bartender says, \"Where's my brother?\"");
                lib.AddTextToQueue("...");
                lib.AddTextToQueue("Funny joke, huh?", spare);
                break;
            case 2:
                fightIntro();
                break;
        }
    }
}
