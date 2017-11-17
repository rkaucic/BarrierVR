using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PapyrusAI : MonoBehaviour {
    public AILibrary lib;
    private Spawner spawner;
    private GameObject basicLaser;
    private GameObject gasterBlaster;
    private GameObject canon;
    private PlayerInfo playerInfo;
    private int laserFailCount;
    private int prevPlayerHealth;
    private int turnCount;
    private float shuffleAttackDelay;

	void Start () {
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        basicLaser = (GameObject)Resources.Load("BasicLaser");
        gasterBlaster = (GameObject)Resources.Load("GasterBlaster");
        canon = (GameObject)Resources.Load("CanonBlaster");
        playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();
        laserFailCount = 0;
        lib.RegisterCrossheirPositionFunc(CrossheirPosition);
        lib.SetVitals(180.0f);
        //lib.SetVitals(11.0f);
        turnCount = 0;
        Invoke("begin", 0.1f);
    }
	
	// Update is called once per frame
	void Update () {

	}

    public float CrossheirPosition(float time)
    {
        return 4.0f - (time * 1.5f);
    }

    public float FinishCrossheirs(float time)
    {
        return 0.0f;
    }

    private void die()
    {
        playerInfo.MaxHealth = 120;
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
            lib.AddTextToQueue(".....");
            lib.AddTextToQueue(".......");
            lib.AddTextToQueue(".........");
            lib.AddTextToQueue("Well I guess I had that coming!");
            lib.AddTextToQueue("Still...");
            lib.AddTextToQueue(".....");
            lib.AddTextToQueue("Good luck, human!", die);
        }
        else
            theChoiceAttack();
    }

    private void mercy()
    {
        lib.StopMusic();
        lib.AddTextToQueue("What a pal!");
        lib.AddTextToQueue("Hahaha, you know, it's kinda funny");
        lib.AddTextToQueue("Because for a second there you had me a little worried!");
        lib.AddTextToQueue("I'll catch you later buddy!");
        lib.AddTextToQueue("No hard feelings about the whole murder thing, right?", spare);
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

    private void finishHim()
    {
        lib.FadeoutMusic(0.0f, 6.0f);
        lib.AddTextToQueue("*huff* *puff* *huff*");
        lib.AddTextToQueue("Well, it's uh *puff* clear that you can't defeat me!");
        lib.AddTextToQueue("So... uh... tell you what");
        lib.AddTextToQueue("I, the great Papyrus, choose to spare you!");
        lib.AddTextToQueue("So, uh, how about it? Friends?", finalDialogueDone);
    }

    private void fastSplitSingle()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 40, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 60, 1, 2, 0.39f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 60, 1, 2, 0.39f);
        lib.WaitForProjectiles(fastSplitSingle_2);
    }
    private void fastSplitSingle_2()
    {
        spawner.SpawnProjectileAtAngle(gasterBlaster, 20, 0, 1.0f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void fastDownXDouble()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 40, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 60, 1, 2, 0.39f);
        c = spawner.SpawnProjectileAtAngle(canon, 40, 30, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, -60, 1, 2, 0.39f);
        lib.WaitForProjectiles(fastDownXDouble_2);
    }
    private void fastDownXDouble_2()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 60, 1, 2, 0.39f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, 30, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, -60, 1, 2, 0.39f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void downUpDouble()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 0, 1, 4, 0.8f);
        c = spawner.SpawnProjectileAtAngle(canon, 40, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 0, 1, 4, 0.8f);
        lib.WaitForProjectiles(downUpDouble_2);
    }
    private void downUpDouble_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 40, -20, 1.0f);
        c.GetComponent<CanonSeries>().Init(-40, 0, 1, 4, 0.8f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 0, 1, 4, 0.8f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void returnToPlayerTurn()
    {
        lib.WaitForProjectiles(playerTurn);
    }

    private void shuffleAttack()
    {
        lib.fadeEnemy();
        shuffleSpawnLL();
        Invoke("shuffleSpawnUR", shuffleAttackDelay* 1.0f);
        Invoke("shuffleSpawnUL", shuffleAttackDelay * 2.0f);
        Invoke("shuffleSpawnLR", shuffleAttackDelay * 3.0f);
        Invoke("shuffleSpawnLL", shuffleAttackDelay * 4.0f);
        Invoke("shuffleSpawnUR", shuffleAttackDelay * 5.0f);
        Invoke("shuffleSpawnUL", shuffleAttackDelay * 6.0f);
        Invoke("shuffleSpawnLR", shuffleAttackDelay * 7.0f);
        Invoke("shuffleSpawnLL", shuffleAttackDelay * 8.0f);
        Invoke("shuffleSpawnUR", shuffleAttackDelay * 9.0f);
        Invoke("shuffleSpawnUL", shuffleAttackDelay * 10.0f);
        Invoke("shuffleSpawnLR", shuffleAttackDelay * 11.0f);
        Invoke("shuffleSpawnLL", shuffleAttackDelay * 12.0f);
        Invoke("returnToPlayerTurn", shuffleAttackDelay * 2.0f);
    }
    private void shuffleSpawnLL()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, -10, -20, 1.0f);
    }
    private void shuffleSpawnLR()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, -10, 20, 1.0f);
    }
    private void shuffleSpawnUL()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, 35, -20, 1.0f);
    }
    private void shuffleSpawnUR()
    {
        spawner.SpawnProjectileAtAngle(basicLaser, 35, 20, 1.0f);
    }

    private void slowXDouble()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 40, 1, 4, 0.8f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, -40, 1, 4, 0.8f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void horizontalDouble()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 60, 1, 5, 0.8f);
        c = spawner.SpawnProjectileAtAngle(canon, 0, 30, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, -60, 1, 5, 0.8f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void slowPlusSingle()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, 0, 1.0f);
        c.GetComponent<CanonSeries>().Init(50, 0, 1, 4, 0.8f);
        lib.WaitForProjectiles(slowPlusSingle_2);
    }
    private void slowPlusSingle_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -25, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 50, 1, 4, 0.8f);
        lib.WaitForProjectiles(playerTurn);
    }

    private void slowXSingle()
    {
        lib.fadeEnemy();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, 40, 1, 4, 0.8f);
        lib.WaitForProjectiles(slowXSingle_2);
    }
    private void slowXSingle_2()
    {
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, 20, 1.0f);
        c.GetComponent<CanonSeries>().Init(40, -40, 1, 4, 0.8f);
        lib.WaitForProjectiles(playerTurn);
    }

    private AILibrary.OnSpeechFinishedEvent getRandomAttack()
    {
        int num = (int)(Random.value * 3.0f);
        if (num == 0)
            return shuffleAttack;
        else if (num == 1)
            return fastSplitSingle;
        return fastDownXDouble;
    }

    private void randomSpeech()
    {
        int num = (int)(Random.value * 3.0f);
        if (num == 0)
            lib.AddTextToQueue("Is my cape getting dirty?", getRandomAttack());
        else if (num == 1)
            lib.AddTextToQueue("I respect your stamina, human!", getRandomAttack());
        else if (num == 2)
            lib.AddTextToQueue("Think you can outlast me? Hah!", getRandomAttack());
        else
            lib.AddTextToQueue("Getting tired yet?", getRandomAttack());
    }

    private void attackSelect()
    {
        if(lib.GetAIHealth() <= 10.0f)
        {
            finishHim();
            return;
        }
        switch(turnCount)
        {
            case 1:
                lib.AddTextToQueue("Don't let your guard down!", slowXSingle);
                break;
            case 2:
                lib.AddTextToQueue("Because I'm giving this battle my all!", slowPlusSingle);
                break;
            case 3:
                lib.AddTextToQueue("For you see...");
                lib.AddTextToQueue("It's always been my dream to join the Elite Guard!", horizontalDouble);
                break;
            case 4:
                lib.AddTextToQueue("But Undyne won't let me in until I kill a human!");
                lib.AddTextToQueue("Talk about barriers to entry, am I right?");
                lib.AddTextToQueue("Nyeh heh heh!", slowXDouble);
                break;
            case 5:
                lib.AddTextToQueue("Seeing her in that sweet armor, she looks...");
                lib.AddTextToQueue("Well...");
                shuffleAttackDelay = 0.5f;
                lib.AddTextToQueue("Almost as cool as me!", shuffleAttack);
                break;
            case 6:
                lib.AddTextToQueue("So... uh...");
                lib.AddTextToQueue("What were we talking about?");
                lib.AddTextToQueue("Oh, yeah!");
                lib.AddTextToQueue("I need to kill you!", downUpDouble);
                break;
            case 7:
                lib.AddTextToQueue("C'mon, don't look at me like that!");
                lib.AddTextToQueue("Don't think of it as pointlessly dying!");
                lib.AddTextToQueue("Think of it as dying to make me look cool!");
                lib.AddTextToQueue("Cooler, I mean!", fastDownXDouble);
                break;
            case 8:
                shuffleAttackDelay = 0.3f;
                lib.AddTextToQueue("I can see it now... Papyrus, the great Guardsman!", shuffleAttack);
                break;
            case 9:
                lib.AddTextToQueue("Undyne's so proud of me...", fastSplitSingle);
                break;
            case 10:
                lib.AddTextToQueue("It's all I ever wanted, but...");
                lib.AddTextToQueue("Somehow it seems all... mean.", getRandomAttack());
                break;
            case 11:
                lib.AddTextToQueue("Do I really want to kill you, human?", getRandomAttack());
                break;
            case 12:
                lib.AddTextToQueue("I'm not...");
                lib.AddTextToQueue("I'm not sure anymore.", getRandomAttack());
                break;
            case 13:
                lib.AddTextToQueue("I'm sure you're giving me a real workout, though!");
                lib.AddTextToQueue("You might say you're...");
                lib.AddTextToQueue("Working me to the bone!");
                lib.AddTextToQueue("Nyeh heh heh!", getRandomAttack());
                break;
            default:
                randomSpeech();
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
        lib.PlayMusic((AudioClip)Resources.Load("Bonetrousle"));
        playerTurn();
    }

    private void superSeriousBattleStarting()
    {
        lib.StopMusic();
        lib.AddTextToQueue("Our games are over!");
        lib.AddTextToQueue("I must now give you the proper greeting as custom dictates!");
        lib.AddTextToQueue("I shall not hold back!");
        lib.AddTextToQueue("Prepare yourself, human!", beginTrueFight);
    }

    private void blasterSuccessCheck() {
        lib.solidifyEnemy();
        if (lib.GetPlayerHealth() != prevPlayerHealth)
        {
            playerInfo.ResetHealth();
            switch (laserFailCount)
            {
                case 0:
                    lib.AddTextToQueue("Not quite!");
                    lib.AddTextToQueue("Remember, you need to use BOTH BARRIERS to block the beam!", blasterIntro);
                    break;
                case 1:
                    lib.AddTextToQueue("Okay, I'm not sure how else to put this.");
                    lib.AddTextToQueue("Put your shields together!");
                    lib.AddTextToQueue("Real close!");
                    lib.AddTextToQueue("Nice and snug!");
                    lib.AddTextToQueue("Capiche?", blasterIntro);
                    break;
                case 2:
                    lib.AddTextToQueue("Oh my God.");
                    lib.AddTextToQueue("I'm actually dying over here.");
                    lib.AddTextToQueue("Which is pretty impressive seeing as I'm a skeleton.");
                    lib.AddTextToQueue("Just... like...");
                    lib.AddTextToQueue("Put your shields together! They'll change color.");
                    lib.AddTextToQueue(".....");
                    lib.AddTextToQueue("Do you know what colors are?");
                    lib.AddTextToQueue("Seven billion humans and I get this one...", blasterIntro);
                    break;
                default:
                    lib.AddTextToQueue("...");
                    lib.AddTextToQueue(".....");
                    lib.AddTextToQueue(".......");
                    lib.AddTextToQueue("Just try again.", blasterIntro);
                    break;
            }
            laserFailCount++;
        }
        else
        {
            lib.AddTextToQueue("Fantastic!");
            lib.AddTextToQueue("Phenominal!");
            if(laserFailCount >= 3)
            {
                lib.AddTextToQueue("Kinda embarassing it took you this long, but hey.");
                lib.AddTextToQueue("We can't all not be terrible!");
            }
            lib.AddTextToQueue("But the time has come!", superSeriousBattleStarting);
        }
    }

    private void blasterIntro()
    {
        lib.fadeEnemy();
        spawner.SpawnProjectileAtAngle(gasterBlaster, 10, 0, 1.0f);
        lib.WaitForProjectiles(blasterSuccessCheck);
        prevPlayerHealth = lib.GetPlayerHealth();
    }

    private void blasterTutorial()
    {
        lib.AddTextToQueue("Now then, it's time for something harder!");
        lib.AddTextToQueue("I present to you-- a blaster!");
        lib.AddTextToQueue("Its might is unsurpassed! Its power is undeniable!");
        lib.AddTextToQueue("You must use BOTH BARRIERS to block its attack!");
        lib.AddTextToQueue("Ready? Here it comes!", blasterIntro);
        laserFailCount = 0;
    }

    private void canonSuccessCheck()
    {
        lib.solidifyEnemy();
        if(prevPlayerHealth == lib.GetPlayerHealth())
        {
            lib.AddTextToQueue("Congrats!");
            lib.AddTextToQueue("You didn't die!", blasterTutorial);
        }
        else
        {
            playerInfo.ResetHealth();
            switch (laserFailCount)
            {
                case 0:
                    lib.AddTextToQueue("Almost!");
                    lib.AddTextToQueue("Try again!", canonIntro);
                    break;
                case 1:
                    lib.AddTextToQueue("You're so close!");
                    lib.AddTextToQueue("I can feel it in my bones!");
                    lib.AddTextToQueue("Nyeh heh heh!", canonIntro);
                    break;
                case 2:
                    lib.AddTextToQueue("Alright!");
                    lib.AddTextToQueue("Maybe you actually weren't close!");
                    lib.AddTextToQueue("Honestly I was just saying that to make you feel better.");
                    lib.AddTextToQueue("You're actually pretty bad.");
                    lib.AddTextToQueue("Er... Try it again, I guess?", canonIntro);
                    break;
                default:
                    lib.AddTextToQueue("AAAAARGH");
                    lib.AddTextToQueue("You're killing me!", canonIntro);
                    break;
            }
            laserFailCount++;
        }
    }

    private void canonIntro()
    {
        lib.fadeEnemy();
        prevPlayerHealth = lib.GetPlayerHealth();
        GameObject c = spawner.SpawnProjectileAtAngle(canon, 0, -20, 1.0f);
        c.GetComponent<CanonSeries>().Init(0, 40, 1, 4, 0.8f);
        lib.WaitForProjectiles(canonSuccessCheck);
    }

    private void firstAttackSuccess()
    {
        lib.AddTextToQueue("Now let's see how you handle a canon!");
        lib.AddTextToQueue("It's a moving skull that shoots a bunch of lasers!");
        lib.AddTextToQueue("Not as attractive as my skull, but hey, what is?");
        laserFailCount = 0;
        lib.AddTextToQueue("Here it comes!", canonIntro);
    }

    private void afterFirstAttack()
    {
        if(lib.GetAIHealth() == lib.GetAIMaxHealth())
        {
            lib.AddTextToQueue("That was great!");
            lib.AddTextToQueue("For a loser!");
            lib.AddTextToQueue("Seriously, try that again...", firstAttack);
        }
        else
        {
            lib.AddTextToQueue("Ouch!");
            lib.AddTextToQueue("Hahaha, don't worry, it'll take waaaaaay more than that to take me down!", firstAttackSuccess);
        }
    }

    private void firstAttack()
    {
        lib.PlayerAttack(afterFirstAttack);
    }

    private void swingTutorial()
    {
        lib.AddTextToQueue("You should probably learn how to attack yourself, too!");
        lib.AddTextToQueue("It's super easy!");
        lib.AddTextToQueue("It's like I always say:");
        lib.AddTextToQueue("\"Violence--");
        lib.AddTextToQueue("It's... uh...");
        lib.AddTextToQueue("Pretty great!");
        lib.AddTextToQueue("Just like me!\"");
        lib.AddTextToQueue("Don't I have the best quotes?");
        lib.AddTextToQueue("They're pretty great.");
        lib.AddTextToQueue("So, uh, to attack, just point your barrier at your foe's weak points");
        lib.AddTextToQueue("And unleash the power within you!");
        lib.AddTextToQueue("Folks are most vulnerable to attacks when their weak points overlap!");
        lib.AddTextToQueue("So fire just as they're on top of each other!");
        lib.AddTextToQueue("That's the trick.");
        lib.AddTextToQueue("Give it a shot on me.");
        lib.AddTextToQueue("Get it? \"Shot\"?");
        lib.AddTextToQueue("Nyeh heh heh!", firstAttack);
    }

    private void done()
    {
        lib.solidifyEnemy();
        if (lib.GetPlayerHealth() == prevPlayerHealth)
        {
            lib.AddTextToQueue("Nice job! Lasers hurt, let me tell you!", swingTutorial);
        }
        else
        {
            playerInfo.ResetHealth();
            switch (laserFailCount)
            {
                case 0:
                    lib.AddTextToQueue("Wow! You're really bad!");
                    break;
                case 1:
                    lib.AddTextToQueue("Um.");
                    lib.AddTextToQueue("Perhaps we're not communicating here!");
                    lib.AddTextToQueue("You're supposed to BLOCK the LASERS with a BARRIER!");
                    break;
                case 2:
                    lib.AddTextToQueue("Wow!");
                    lib.AddTextToQueue("I don't know what you're doing!");
                    lib.AddTextToQueue("You're actually just the worst!");
                    break;
                default:
                    lib.AddTextToQueue("...");
                    break;
            }
            laserFailCount++;
            lib.AddTextToQueue("Let's try that again!", pewpew);
        }
    }

    private void pewpew()
    {
        lib.fadeEnemy();
        spawner.SpawnProjectileAtAngle(basicLaser, 10, 0, 1.0f);
        prevPlayerHealth = lib.GetPlayerHealth();
        lib.WaitForProjectiles(done);
    }

    private void begin()
    {
        lib.PlayMusic((AudioClip)Resources.Load("Nyeh Heh Heh!"));
        lib.AddTextToQueue("Welcome, human!");
        lib.AddTextToQueue("Although you are new to this world, fear not!");
        lib.AddTextToQueue("For I, the great Papyrus, am here to guide you on your path!");
        lib.AddTextToQueue("Not because I must-- but because I am great.");
        lib.AddTextToQueue("And handsome.");
        lib.AddTextToQueue("And also humble.");
        lib.AddTextToQueue("I'm pretty great.");
        lib.AddTextToQueue("Yup.");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue("Anyway, life down here is fraught with peril!");
        lib.AddTextToQueue("Everyone you meet will surely try to kill you!");
        lib.AddTextToQueue("Not because we're mean, honestly it's just how we say \"hello\".");
        lib.AddTextToQueue("...");
        lib.AddTextToQueue(".....");
        lib.AddTextToQueue("Hey, don't judge my culture you ethnocentric ass.");
        lib.AddTextToQueue("Anyway, the great Papyrus shall now prepare you for the journey ahead!");
        lib.AddTextToQueue("See those glowey things on your hands?");
        lib.AddTextToQueue("Those are your BARRIERS!");
        lib.AddTextToQueue("Or possibly inoperable TUMORS!");
        lib.AddTextToQueue("But hopefully BARRIERS!");
        lib.AddTextToQueue("Down here, we tend to throw LASERS at our guests.");
        lib.AddTextToQueue("They're super easy to draw, you see.");
        lib.AddTextToQueue("Hold a BARRIER in front of this LASER!", pewpew);
    }
}
