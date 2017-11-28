using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour {

    public float HUD_NudgeAmount = 0.15f;
    public float Max_HUD_Height = 1.5f;
    public float Min_HUD_Height = 0.75f;

    private GameObject player;
    private PlayerInfo playerInfo;
    private OVRPlayerController playerController;
    private GameObject activePlayerPointer;
    private GameObject playerPointer;

    // <Robert>
    private GameObject lightningButton;
    private MenuButtonHighligher lightningButtonHighlighter;
    // </Robert>

    private GameObject papyrusButton;
    private MenuButtonHighligher papyrusButtonHighlighter;
    private GameObject undyneButton;
    private MenuButtonHighligher undyneButtonHighlighter;
    private GameObject sansButton;
    private MenuButtonHighligher sansButtonHighlighter;
    private GameObject quitButton;
    private MenuButtonHighligher quitButtonHighlighter;
    /*private GameObject cheatButton;
    private MenuButtonHighligher cheatButtonHighlighter;
    private GameObject megaCheatButton;
    private MenuButtonHighligher megaCheatButtonHighlighter;*/

    private GameObject upButton;
    private MenuButtonHighligher upButtonHighlighter;
    private GameObject downButton;
    private MenuButtonHighligher downButtonHighlighter;

    private GameObject godButton;
    private MenuButtonHighligher godButtonHighlighter;
    private GameObject papyrusKillButton;
    private MenuButtonHighligher papyrusKillButtonHighlighter;
    private GameObject undyneKillButton;
    private MenuButtonHighligher undyneKillButtonHighlighter;

    private GameObject hud;
    private GameObject mainMenu;
    private GameObject projSpawner;

    private GameObject fightButton;
    private GameObject mercyButton;

    private GameObject undyne;
    private GameObject sans;
    // <Robert>
    private GameObject lightning;
    // </Robert>

    private GameObject[] credits;

    private int enemyFought;
    private Color killedColor;
    private int killedCount;

    private bool domPressedThisFrame;
    private bool domPressed;

    private bool isEnabled;

    private GameObject gameOverHeart;
    private SpriteRenderer gameOverHeartSpriteRen;
    private ParticleSystem gameOverHeartParts;
    public Sprite origHeart;
    public Sprite brokenHeart;
    public GameObject heartCut;
    public GameObject heartBurst;
    private AudioClip gameOverMusic;

    private GameObject mainMenuButton;
    private MenuButtonHighligher mainMenuButtonHighlighter;

    void Start () {
        //player = GameObject.FindGameObjectWithTag("Player");
        player = GameObject.Find("OVRPlayerController");
        playerInfo = player.GetComponent<PlayerInfo>();
        playerController = player.GetComponent<OVRPlayerController>();
        //playerPointer = (GameObject)Resources.Load("PlayerPointer");

        papyrusButton = transform.Find("PapyrusButton").gameObject;
        papyrusButtonHighlighter = papyrusButton.GetComponent<MenuButtonHighligher>();
        undyneButton = transform.Find("UndyneButton").gameObject;
        undyneButtonHighlighter = undyneButton.GetComponent<MenuButtonHighligher>();
        sansButton = transform.Find("BadTimeButton").gameObject;
        sansButtonHighlighter = sansButton.GetComponent<MenuButtonHighligher>();
        quitButton = transform.Find("QuitButton").gameObject;
        quitButtonHighlighter = quitButton.GetComponent<MenuButtonHighligher>();
        /*cheatButton = transform.Find("CheatButton").gameObject;
        cheatButtonHighlighter = cheatButton.GetComponent<MenuButtonHighligher>();
        megaCheatButton = transform.Find("MegaCheatButton").gameObject;
        megaCheatButtonHighlighter = megaCheatButton.GetComponent<MenuButtonHighligher>();*/

        upButton = transform.Find("UpButton").gameObject;
        upButtonHighlighter = upButton.GetComponent<MenuButtonHighligher>();
        downButton = transform.Find("DownButton").gameObject;
        downButtonHighlighter = downButton.GetComponent<MenuButtonHighligher>();

        godButton = GameObject.Find("GodButton");
        godButtonHighlighter = godButton.GetComponent<MenuButtonHighligher>();
        papyrusKillButton = transform.Find("PapyrusKillButton").gameObject;
        papyrusKillButtonHighlighter = papyrusKillButton.GetComponent<MenuButtonHighligher>();
        papyrusKillButton.SetActive(false);
        undyneKillButton = transform.Find("UndyneKillButton").gameObject;
        undyneKillButtonHighlighter = undyneKillButton.GetComponent<MenuButtonHighligher>();
        undyneKillButton.SetActive(false);

        // <Robert>
        lightningButton = transform.Find("LightningButton").gameObject;
        lightningButtonHighlighter = lightningButton.GetComponent<MenuButtonHighligher>();
        // </Robert>

        hud = GameObject.Find("ElasticHUD");
        mainMenu = GameObject.Find("MainMenu");
        projSpawner = GameObject.Find("ProjectileSpawner");

        fightButton = GameObject.Find("AttackButton");
        mercyButton = GameObject.Find("MercyButton");

        credits = new GameObject[3];
        credits[0] = GameObject.Find("Credits - People");
        credits[1] = GameObject.Find("Credits - UIUC");
        credits[2] = GameObject.Find("Credits - Toby");

        gameOverHeart = transform.Find("GameOverHeart").gameObject;
        gameOverHeartSpriteRen = gameOverHeart.GetComponent<SpriteRenderer>();
        gameOverHeartParts = transform.Find("GameOverParts").GetComponent<ParticleSystem>();

        mainMenuButton = GameObject.Find("MainMenuButton");
        mainMenuButtonHighlighter = mainMenuButton.GetComponent<MenuButtonHighligher>();
        mainMenuButton.SetActive(false);
        gameOverHeart.SetActive(false);

        killedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        killedCount = 0;

        gameOverMusic = (AudioClip)Resources.Load("Determination");

        Invoke("initOpenMenu", 0.01f);
    }
	
	void Update () {
        if (!isEnabled)
            return;
        inputManager();
        buttonManager();
	}

    private void initOpenMenu()
    {
        OpenMenu();
    }

    public void OpenMenu(bool enemyKilled = false, bool pointerActive = false)
    {
        playerController.SetMusicPitch(1.0f);
        resetPlayerHealth();
        if (enemyKilled)
        {
            slayEnemy();
        }
        isEnabled = true;
        if(killedCount == 0)
            playMusic((AudioClip)Resources.Load("Menu (Full)"));
        else if(killedCount == 1)
            playMusic((AudioClip)Resources.Load("Start Menu"));
        else if(killedCount == 2)
            playMusic((AudioClip)Resources.Load("Small Shock"));
        else
            playMusic((AudioClip)Resources.Load("Dummy!"));
        /*
        if (pointerActive == false)
        {
            Transform attachShield = GameObject.Find("RightShield").transform;
            activePlayerPointer = Instantiate(playerPointer, attachShield.position, attachShield.rotation, attachShield);
        }*/

        fightButton.SetActive(false);
        mercyButton.SetActive(false);
        papyrusButton.SetActive(true);
        undyneButton.SetActive(true);
        sansButton.SetActive(true);
        quitButton.SetActive(true);
        //cheatButton.SetActive(true);
        //megaCheatButton.SetActive(true);

        // <Robert>
        lightningButton.SetActive(true);
        // </Robert>

        upButton.SetActive(true);
        downButton.SetActive(true);

        godButton.SetActive(true);
        if(killedCount == 0)
            papyrusKillButton.SetActive(true);
        else if(killedCount == 1)
            undyneKillButton.SetActive(true);

        foreach(GameObject obj in credits)
        {
            obj.SetActive(true);
        }
    }

    public void GameOver()
    {
        playerController.StopMusic();
        gameOverHeartSpriteRen.sprite = origHeart;
        gameOverHeart.SetActive(true);
        Invoke("breakHeart", 1.0f);
    }

    private void breakHeart()
    {
        gameOverHeartSpriteRen.sprite = brokenHeart;
        Instantiate(heartCut, player.transform.position, Quaternion.identity);
        Invoke("burstHeart", 1.5f);
    }

    private void burstHeart()
    {
        gameOverHeart.SetActive(false);
        Instantiate(heartBurst, player.transform.position, Quaternion.identity);
        gameOverHeartParts.Play();
        Invoke("showGameOverOptions", 1.0f);
    }

    private void showGameOverOptions()
    {
        playMusic(gameOverMusic);
        playerController.SetMusicPitch(1.0f);
        mainMenuButton.SetActive(true);
        Transform attachShield = GameObject.Find("RightShield").transform;
        //activePlayerPointer = Instantiate(playerPointer, attachShield.position, attachShield.rotation, attachShield);
        isEnabled = true;
    }

    private void inputManager()
    {
        if (GetDominantTriggerDown() && !domPressed)
        {
            domPressedThisFrame = true;
            domPressed = true;
        }
        else
        {
            domPressedThisFrame = false;
        }
        if (!GetDominantTriggerDown())
        {
            domPressed = false;
        }
    }

    private void startUndyne()
    {
        undyne.GetComponent<AILibrary>().RegisterAttackButtons(fightButton, mercyButton);
        undyne.GetComponent<UndyneAI>().RegisterTotalKills(killedCount);
    }

    private void startSans()
    {
        sans.GetComponent<AILibrary>().RegisterAttackButtons(fightButton, mercyButton);
        sans.GetComponent<SansAI>().RegisterTotalKills(killedCount);
    }

    // <Robert>
    private void startLightning()
    {
        lightning.GetComponent<AILibrary>().RegisterAttackButtons(fightButton, mercyButton);
        lightning.GetComponent<LightningAI>().RegisterTotalKills(killedCount);
    }
    // </Robert>

    private void buttonManager()
    {
        if(domPressedThisFrame)
        {
            if(papyrusButtonHighlighter.IsHovered())
            {
                closeMenu();
                enemyFought = 0;
                GameObject p = (GameObject)Instantiate(Resources.Load("Papyrus"));
                p.GetComponent<AILibrary>().RegisterAttackButtons(fightButton, mercyButton);
            }
            else if(undyneButtonHighlighter.IsHovered())
            {
                closeMenu();
                enemyFought = 1;
                undyne = (GameObject)Instantiate(Resources.Load("Undyne"));
                Invoke("startUndyne", 0.01f);
            }
            else if(sansButtonHighlighter.IsHovered())
            {
                closeMenu(true);
                enemyFought = 2;
                sans = (GameObject)Instantiate(Resources.Load("Sans"));
                Invoke("startSans", 0.01f);
            } // <Robert>
            else if(lightningButtonHighlighter.IsHovered())
            {
                closeMenu();
                enemyFought = 3;
                lightning = (GameObject)Instantiate(Resources.Load("Lightning"));
                Invoke("startLightning", 0.01f);
            } // </Robert>
            else if(quitButtonHighlighter.IsHovered())
            {
                Application.Quit();
            }
            else if(upButtonHighlighter.IsHovered())
            {
                nudgeMenu(1.0f);
            }
            else if (downButtonHighlighter.IsHovered())
            {
                nudgeMenu(-1.0f);
            }
            /*else if(cheatButtonHighlighter.IsHovered())
            {
                killedCount = 1;
                playerInfo.MaxHealth = 120;
                playerInfo.ResetHealth();
                playMusic((AudioClip)Resources.Load("Start Menu"));
            }
            else if(megaCheatButtonHighlighter.IsHovered())
            {
                killedCount = 2;
                playerInfo.MaxHealth = 200;
                playerInfo.ResetHealth();
                playMusic((AudioClip)Resources.Load("Small Shock"));
            }*/
            else if(godButtonHighlighter.IsHovered())
            {
                playerInfo.GodModeToggle();
            }
            else if(papyrusKillButtonHighlighter.IsHovered())
            {
                enemyFought = 0;
                slayEnemy();
                playerInfo.MaxHealth = 120;
                playerInfo.ResetHealth();
                playMusic((AudioClip)Resources.Load("Start Menu"));
                undyneKillButton.SetActive(true);
                papyrusKillButton.SetActive(false);
                papyrusKillButtonHighlighter.ForceDeselect();
            }
            else if (undyneKillButtonHighlighter.IsHovered())
            {
                enemyFought = 1;
                slayEnemy();
                playerInfo.MaxHealth = 200;
                playerInfo.ResetHealth();
                playMusic((AudioClip)Resources.Load("Small Shock"));
                undyneKillButton.SetActive(false);
                undyneKillButtonHighlighter.ForceDeselect();
            }
            else if(mainMenuButtonHighlighter.IsHovered())
            {
                mainMenuButton.SetActive(false);
                mainMenuButtonHighlighter.ForceDeselect();
                OpenMenu(false, true);
            }
        }
    }

    private void nudgeMenu(float scale)
    {
        Vector3 nudgeVec = new Vector3(0.0f, HUD_NudgeAmount * scale, 0.0f);
        float newHudHeight = nudgeVec.y + hud.transform.position.y;
        if (newHudHeight > Max_HUD_Height || newHudHeight < Min_HUD_Height)
        {
            return;
        }
        hud.transform.position += nudgeVec;
        mainMenu.transform.position += nudgeVec;
        projSpawner.transform.position += nudgeVec;
    }

    private void closeMenu(bool continueMusic = false)
    {
        if(!continueMusic)
            stopMusic();
        Object.Destroy(activePlayerPointer);
        activePlayerPointer = null;
        papyrusButtonHighlighter.ForceDeselect();
        papyrusButton.SetActive(false);
        undyneButtonHighlighter.ForceDeselect();
        undyneButton.SetActive(false);
        sansButtonHighlighter.ForceDeselect();
        sansButton.SetActive(false);
        quitButtonHighlighter.ForceDeselect();
        quitButton.SetActive(false);

        upButton.SetActive(false);
        upButtonHighlighter.ForceDeselect();
        downButton.SetActive(false);
        downButtonHighlighter.ForceDeselect();

        godButton.SetActive(false);
        godButtonHighlighter.ForceDeselect();
        papyrusKillButton.SetActive(false);
        papyrusKillButtonHighlighter.ForceDeselect();
        undyneKillButton.SetActive(false);
        undyneKillButtonHighlighter.ForceDeselect();
        // <Robert>
        lightningButtonHighlighter.ForceDeselect();
        lightningButton.SetActive(false);
        // </Robert>

        /*cheatButtonHighlighter.ForceDeselect();
        cheatButton.SetActive(false);
        megaCheatButtonHighlighter.ForceDeselect();
        megaCheatButton.SetActive(false);*/
        foreach (GameObject obj in credits)
        {
            obj.SetActive(false);
        }
        isEnabled = false;
    }

    private void slayEnemy()
    {
        killedCount++;
        switch (enemyFought)
        {
            case 0:
                papyrusButtonHighlighter.SetEnabled(false);
                papyrusButton.GetComponent<SpriteRenderer>().color = killedColor;
                break;
            case 1:
                undyneButtonHighlighter.SetEnabled(false);
                undyneButton.GetComponent<SpriteRenderer>().color = killedColor;
                break;
            case 2:
                sansButtonHighlighter.SetEnabled(false);
                sansButton.GetComponent<SpriteRenderer>().color = killedColor;
                break;
            default: // <Robert>
                killedCount--;
                break;
                // </Robert>
        }
    }

    private void playMusic(AudioClip clip)
    {
        playerController.PlayMusic(clip);
    }

    private void stopMusic()
    {
        playerController.StopMusic();
    }

    private void resetPlayerHealth()
    {
        playerInfo.ResetHealth();
    }

    public bool GetDominantTriggerPressed()
    {
        return domPressedThisFrame;
    }

    public static bool GetDominantTriggerDown()
    {
        return OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
    }
}
