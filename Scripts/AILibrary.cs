using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class AILibrary : MonoBehaviour {

    public delegate void OnSpeechFinishedEvent();
    public delegate void OnProjectilesFinishedEvent();
    public delegate void OnAttackEndEvent();
    public delegate float CrossheirPositionCallback(float time);
    public delegate void OnMusicFadeFinishedEvent();

    public struct SpeechEntry
    {
        public string text;
        public OnSpeechFinishedEvent callback;
        public SpeechEntry(string t, OnSpeechFinishedEvent s) { text = t; callback = s; }
    }
    private OnSpeechFinishedEvent latestSpeechCallback;
    private OnProjectilesFinishedEvent waitProjectilesCallback;
    private CrossheirPositionCallback crossheirPosCallback;
    private OnMusicFadeFinishedEvent musicFadeCallback;
    private bool waitingForMusicFade;
    private bool waitingForProjectiles;
    private Speech speechObj;
    private Queue<SpeechEntry> speechQueue;
    private AudioSource musicSource;
    private bool isSpeaking;

    private GameObject enemy;
    private SpriteRenderer enemySprite;
    private bool isEnemyDodging;
    private float enemyDodgeStartTime;
    private float enemyDodgeDistance;
    private float enemyDodgeTime;
    private Vector3 enemyInitPos;
    private GameObject healthBar;
    private GameObject healthBarHealth;
    private GameObject damageText;
    private bool isLerpingHealthBar;
    private float startLerpHealth;
    private float lerpStartTime;

    private float playerAttackStartTime;
    private bool isAttackMenuLive;
    private bool isPlayerAttacking;
    // <Robert>
    private bool crossheirActive;
    private bool allowFire;
    private bool fightDone;
    private float dingTime;
    public bool lightning;
    // </Robert>
    private bool isAttackTimeFrozen;
    private float frozenAttackTime;
    private bool isPlayerShotLive;
    private GameObject playerShot;
    private GameObject leftCrossheir;
    private GameObject rightCrossheir;
    private SpriteRenderer leftCrossheirRenderer;
    private SpriteRenderer rightCrossheirRenderer;
    private bool forcePlayerMiss;
    private GameObject enemyBody;
    private GameObject activePlayerPointer;
    private GameObject activePlayerSword;

    private GameObject fightButton;
    private MenuButtonHighligher fightButtonHighlighter;
    private GameObject mercyButton;
    private MenuButtonHighligher mercyButtonHighlighter;
    private bool lerpingMenuButtons;
    private Vector3 initAttackButtonPos;
    private Quaternion initAttackButtonRot;
    private Vector3 finalAttackButtonPos;
    private Quaternion finalAttackButtonRot;
    private Vector3 initMercyButtonPos;
    private Quaternion initMercyButtonRot;
    private Vector3 finalMercyButtonPos;
    private Quaternion finalMercyButtonRot;
    private bool startedButtonLerp;
    private bool isFinishing;
    private float menuButtonLerpStartTime;

    private OnAttackEndEvent immediateAttackCallback;

    private OnAttackEndEvent attackCallback;
    private OnAttackEndEvent mercyCallback;

    private bool domPressedThisFrame;
    private bool domPressed;

    private GameObject player;
    private OVRPlayerController playerController;

    private float health;
    private float maxHealth;

    private float musicFadeStartTime;
    private bool isMusicFading;
    private float musicFadeTime;
    private float musicFadeStartVolume;
    private float musicFadeEndVolume;

    private float pitchFadeStartTime;
    private bool isPitchFading;
    private float pitchFadeTime;
    private float pitchFadeStartPitch;
    private float pitchFadeEndPitch;

    private bool isFadingCrossheirs;
    private float crossheirFadeStartTime;
    private float crossheirFadeTime;

    private bool isDying;
    private bool isDead;
    private bool isSparing;
    private float dieStartTime;
    private float spareStartTime;
    private Color liveColor;
    private Color deadColor;
    private Vector3 spareStartPos;
    private Vector3 spareEndPos;

    // Variables to facilitate Sans' fight functionality
    private bool preventPlayerShot;
    private bool forceLethalHit;

    public GameObject ding;

    public void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        enemySprite = enemy.transform.Find("BodySprite").GetComponent<SpriteRenderer>();
        waitingForProjectiles = false;
        waitingForMusicFade = false;
        isAttackMenuLive = false;
        isPlayerAttacking = false;
        crossheirActive = false;
        allowFire = false;
        fightDone = false;
        lightning = false;
        speechObj = transform.Find("BodySprite").Find("SpeechBubbleSprite").GetComponent<Speech>();
        enemyBody = transform.Find("BodySprite").gameObject;
        healthBar = GameObject.Find("HealthBar");
        healthBarHealth = GameObject.Find("Health");
        healthBar.SetActive(false);
        damageText = GameObject.Find("DamageText");
        if(damageText == null)
        {
            print("damageText null");
        } 
        else
        {
            print(damageText);
        }

        damageText.gameObject.GetComponent<TextMesh>().text = "";
        isSpeaking = false;
        domPressedThisFrame = false;
        domPressed = false;
        speechQueue = new Queue<SpeechEntry>();
        player = GameObject.Find("OVRPlayerController");
        playerController = player.GetComponent<OVRPlayerController>();
        if(playerController == null)
        {
            print("error");
        }
        else
        {
            print("fine");
        }
        isPlayerShotLive = false;
        isLerpingHealthBar = false;
        forcePlayerMiss = false;
        crossheirFadeTime = 1.5f;
        enemyDodgeTime = 2.0f;
        isEnemyDodging = false;
        enemyDodgeDistance = 2.0f;
        dingTime = Time.fixedTime + 1.0f;

        enemyInitPos = enemySprite.transform.position;


        preventPlayerShot = forceLethalHit = false;

        /*fightButton = GameObject.Find("AttackButton");
        fightButtonHighlighter = fightButton.GetComponent<MenuButtonHighligher>();
        mercyButton = GameObject.Find("MercyButton");
        mercyButtonHighlighter = mercyButton.GetComponent<MenuButtonHighligher>();
        Invoke("hideButtons", 0.01f);*/

        isPitchFading = isMusicFading = false;
        lerpingMenuButtons = startedButtonLerp = isFinishing = false;

        isDead = isDying = isSparing = false;
        liveColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        deadColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);


        //initAttackButtonPos = initAttackButtonRot = finalAttackButtonPos = finalAttackButtonRot = null;
        //initMercyButtonPos = initMercyButtonRot = finalMercyButtonPos = finalMercyButtonRot = null;

        immediateAttackCallback = null;
        //createSword();
        activePlayerSword = GameObject.Find("PlayerSword");
    }

    public void RegisterAttackButtons(GameObject a, GameObject m)
    {
        fightButton = a;
        fightButtonHighlighter = fightButton.GetComponent<MenuButtonHighligher>();
        mercyButton = m;
        mercyButtonHighlighter = mercyButton.GetComponent<MenuButtonHighligher>();
    }

    private void hideButtons()
    {
        //fightButton.SetActive(false);
        //mercyButton.SetActive(false);
    }

    public void SetVitals(float mhp)
    {
        health = maxHealth = mhp;
    }

    public void Update()
    {
        inputManager();
        speechManager();
        projectileCallbackManager();
        playerAttackManager();
        musicFadeManager();
        endFightManager();
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

    private void speechManager()
    {
        if (isSpeaking)
        {
            if (GetDominantTriggerPressed())
            {
                if (speechQueue.Count != 0)
                {
                    sayNext();
                }
                else
                {
                    stopSpeaking();
                }
            }
        }
    }

    private void projectileCallbackManager()
    {
        if (waitingForProjectiles)
        {
            if (GameObject.FindGameObjectWithTag("Projectile") == null)
            {
                waitingForProjectiles = false;
                waitProjectilesCallback();
            }
        }
    }

    private void playerAttackManager()
    {
        if (lightning)
        {
            if (isPlayerShotLive)
            {
                /*
                if(isFadingCrossheirs)
                {
                    float lerpFactor = Mathf.Min((Time.fixedTime - crossheirFadeStartTime) / crossheirFadeTime, 1.0f);
                    leftCrossheirRenderer.color = rightCrossheirRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - lerpFactor);
                    if(lerpFactor == 1.0f)
                    {
                        isFadingCrossheirs = false;
                    }
                }
                */
                if (isEnemyDodging)
                {
                    float lerpFactor = Mathf.Min((Time.fixedTime - enemyDodgeStartTime) / enemyDodgeTime, 1.0f);
                    enemyBody.transform.position = enemyInitPos + new Vector3(Mathf.Sin(lerpFactor * Mathf.PI) * enemyDodgeDistance, 0, 0);
                    if (lerpFactor == 1.0f)
                    {
                        isEnemyDodging = false;
                    }
                }
                if (playerShot == null)
                {
                    dingTime = Time.fixedTime + 1.0f;
                    isPlayerAttacking = false;
                    isPlayerShotLive = false;
                    isAttackTimeFrozen = false;
                    //Object.Destroy(leftCrossheir);
                    //Object.Destroy(rightCrossheir);
                    Invoke("clearDamageText", 3.0f);
                    if (attackCallback != null)
                        attackCallback();
                }
            }
            else if (!isSpeaking && allowFire && domPressedThisFrame)
            {
                lightningPlayerFire();
            }
            else if (!fightDone && !allowFire && Time.fixedTime > dingTime)
            {
                allowFire = true;
                if (ding != null)
                {
                    Instantiate(ding, transform.position, transform.rotation);
                }
            }
        }
        else if (!lightning)
        {
            if (isLerpingHealthBar)
            {
                Vector3 startScale = healthBarHealth.transform.localScale;
                float lerpFactor = Mathf.Min((Time.fixedTime - lerpStartTime) / 2.0f, 1.0f);
                healthBarHealth.transform.localScale = new Vector3(Mathf.Lerp((startLerpHealth / maxHealth), health / maxHealth, lerpFactor), startScale.y, startScale.z);
                if (lerpFactor == 1.0f)
                {
                    isLerpingHealthBar = false;
                    Invoke("hideHealthBar", 2.0f);
                }
            }
            if (lerpingMenuButtons)
            {
                float lerpFactor = Mathf.Min(Mathf.Sin(((Time.fixedTime - menuButtonLerpStartTime) / 6.0f) * (Mathf.PI / 2.0f)), 1.0f);
                fightButton.transform.localPosition = Vector3.Lerp(initAttackButtonPos, finalAttackButtonPos, lerpFactor);
                fightButton.transform.rotation = Quaternion.Lerp(initAttackButtonRot, finalAttackButtonRot, lerpFactor);

                mercyButton.transform.localPosition = Vector3.Lerp(initMercyButtonPos, finalMercyButtonPos, lerpFactor);
                mercyButton.transform.rotation = Quaternion.Lerp(initMercyButtonRot, finalMercyButtonRot, lerpFactor);

                if (lerpFactor == 1.0f)
                {
                    lerpingMenuButtons = false;
                }
            }
            if (isAttackMenuLive)
            {
                if (domPressedThisFrame)
                {
                    if (fightButtonHighlighter.IsHovered())
                    {
                        closeAttackMenu();
                        onAttackButtonPress();
                        return;
                    }
                    else if (mercyButtonHighlighter.IsHovered())
                    {
                        closeAttackMenu();
                        onMercyButtonPress();
                        return;
                    }
                }
            }
            if (!isPlayerAttacking)
                return;

            if (isPlayerShotLive)
            {
                /*
                if(isFadingCrossheirs)
                {
                    float lerpFactor = Mathf.Min((Time.fixedTime - crossheirFadeStartTime) / crossheirFadeTime, 1.0f);
                    leftCrossheirRenderer.color = rightCrossheirRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - lerpFactor);
                    if(lerpFactor == 1.0f)
                    {
                        isFadingCrossheirs = false;
                    }
                }
                */
                if (isEnemyDodging)
                {
                    float lerpFactor = Mathf.Min((Time.fixedTime - enemyDodgeStartTime) / enemyDodgeTime, 1.0f);
                    enemyBody.transform.position = enemyInitPos + new Vector3(Mathf.Sin(lerpFactor * Mathf.PI) * enemyDodgeDistance, 0, 0);
                    if (lerpFactor == 1.0f)
                    {
                        isEnemyDodging = false;
                    }
                }
                if (playerShot == null)
                {
                    dingTime = Time.fixedTime + 1.0f;
                    isPlayerAttacking = false;
                    isPlayerShotLive = false;
                    isAttackTimeFrozen = false;
                    Object.Destroy(leftCrossheir);
                    Object.Destroy(rightCrossheir);
                    Invoke("clearDamageText", 3.0f);
                    if (attackCallback != null)
                        attackCallback();
                }
            }
            else if (domPressedThisFrame)
            {
                playerFire();
            }
        }
    }

    private void musicFadeManager()
    {
        if (isPitchFading)
        {
            float lerpF = Mathf.Min((Time.fixedTime - pitchFadeStartTime) / pitchFadeTime, 1.0f);
            playerController.SetMusicPitch(Mathf.Lerp(pitchFadeStartPitch, pitchFadeEndPitch, lerpF));
            if (lerpF == 1.0f)
            {
                isPitchFading = false;
            }
        }
        if (waitingForMusicFade && !isMusicFading)
        {
            musicFadeCallback();
            waitingForMusicFade = false;
            return;
        }
        if (!isMusicFading)
            return;
        float lerpFactor = Mathf.Min((Time.fixedTime - musicFadeStartTime) / musicFadeTime, 1.0f);
        playerController.SetMusicVolume(Mathf.Lerp(musicFadeStartVolume, musicFadeEndVolume, lerpFactor));
        if (lerpFactor == 1.0f)
        {
            isMusicFading = false;
            if (musicFadeEndVolume == 0.0f)
            {
                playerController.StopMusic();
                playerController.SetMusicVolume(1.0f);
            }
        }
    }

    private void endFightManager()
    {
        if (isDying)
        {
            float lerpFactor = Mathf.Min((Time.fixedTime - dieStartTime) / 1.0f, 1.0f);
            enemySprite.color = Color.Lerp(liveColor, deadColor, lerpFactor);
            if (lerpFactor == 1.0f)
            {
                Invoke("cleanUp", 6.0f);
                isDying = false;
                isDead = true;
            }
        }
        else if (isSparing)
        {
            float lerpFactor = Mathf.Min((Time.fixedTime - spareStartTime) / 3.0f, 1.0f);
            enemySprite.color = Color.Lerp(liveColor, deadColor, lerpFactor);
            enemy.transform.position = Vector3.Lerp(spareStartPos, spareEndPos, lerpFactor);
            if (lerpFactor == 1.0f)
            {
                Invoke("cleanUp", 4.0f);
                isSparing = false;
            }
        }
    }

    private void resetAttackButtons()
    {
        fightButton.transform.localPosition = initAttackButtonPos;
        fightButton.transform.rotation = initAttackButtonRot;
        mercyButton.transform.localPosition = initMercyButtonPos;
        mercyButton.transform.rotation = initMercyButtonRot;
    }

    private void cleanUp()
    {
        if (initAttackButtonPos != null)
        {
            resetAttackButtons();
        }
        GameObject.Find("MainMenu").GetComponent<MainMenuScript>().OpenMenu(isDead);
        Object.Destroy(this.gameObject);
    }

    public void RegisterImmediateAttackCallback(OnAttackEndEvent callback)
    {
        immediateAttackCallback = callback;
    }

    public void ForcePlayerToMiss(bool miss)
    {
        forcePlayerMiss = miss;
    }

    public void DisallowFire()
    {
        allowFire = false;
        fightDone = true;
    }

    public void DestroyCrossheir()
    {
        Object.Destroy(leftCrossheir);
    }

    public void DestroyProjectiles()
    {
        GameObject[] projs = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject p in projs)
        {
            GameObject.Destroy(p);
        }
    }

    public void TranslateEnemy(Vector3 offset)
    {
        enemyBody.transform.position += offset;
    }

    public void ResetEnemyPos()
    {
        enemyBody.transform.position = enemyInitPos;
    }

    public void ForceLethalHit(bool isLethal)
    {
        forceLethalHit = isLethal;
    }

    public void createPointer()
    {
        if (activePlayerPointer == null)
        {
            Transform attachShield = GameObject.Find("RightShield").transform;
            activePlayerPointer = (GameObject)Instantiate(Resources.Load("PlayerPointer"), attachShield.position, attachShield.rotation, attachShield);
            destroySword();
        }
    }

    public void destroyPointer()
    {
        if (activePlayerPointer != null)
        {
            Destroy(activePlayerPointer);
        }
    }
    public void createSword()
    {
        activePlayerSword.SetActive(true);
        destroyPointer();
    }

    public void destroySword()
    {
        activePlayerSword.SetActive(false);
    }

    public void Die(bool overrideSpriteFade = false, float timeToCleanup = 4.0f)
    {
        Instantiate(Resources.Load("Die"), player.transform.position, player.transform.rotation);   // Sound effect
        if (!overrideSpriteFade)
        {
            dieStartTime = Time.fixedTime;
            isDying = true;
        }
        else
        {
            Invoke("cleanUp", timeToCleanup);
            isDead = true;
        }
        createSword();
    }

    public void Spare()
    {
        spareStartPos = enemy.transform.position;
        spareEndPos = spareStartPos - new Vector3(4.0f, 0.0f, 0.0f);
        spareStartTime = Time.fixedTime;
        isSparing = true;
    }

    public void fadeEnemy()
    {
        enemySprite.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    }

    public void solidifyEnemy()
    {
        enemySprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void registerInitAttackPos()
    {
        initAttackButtonPos = fightButton.transform.localPosition;
        initAttackButtonRot = fightButton.transform.rotation;
        initMercyButtonPos = mercyButton.transform.localPosition;
        initMercyButtonRot = mercyButton.transform.rotation;
    }

    public void clearDamageText()
    {
        damageText.GetComponent<TextMesh>().text = "";
    }

    private void hideHealthBar()
    {
        healthBar.SetActive(false);
    }

    private void playerFire()
    {
        allowFire = false;
        frozenAttackTime = Time.fixedTime - playerAttackStartTime;
        playerShot = (GameObject)Instantiate(Resources.Load("PlayerShot"), activePlayerSword.transform.position, activePlayerSword.transform.rotation);
        //Object.Destroy(activePlayerPointer);
        //activePlayerPointer = null;
        isPlayerShotLive = true;
        if (forcePlayerMiss)
        {
            isFadingCrossheirs = true;
            isEnemyDodging = true;
            if (Random.value > 0.5f) // Randomize the dodge direction
            {
                enemyDodgeDistance *= -1;
            }
            enemyDodgeStartTime = crossheirFadeStartTime = Time.fixedTime;
        }
        else
        isAttackTimeFrozen = true;
        if (immediateAttackCallback != null)
        {
            immediateAttackCallback();
            immediateAttackCallback = null;
        }
    }

    private void lightningPlayerFire()
    {
        allowFire = false;
        frozenAttackTime = Time.fixedTime - playerAttackStartTime;
        playerShot = (GameObject)Instantiate(Resources.Load("PlayerShot"), activePlayerPointer.transform.position, activePlayerPointer.transform.rotation);
        // Object.Destroy(activePlayerPointer);
        // activePlayerPointer = null;
        isPlayerShotLive = true;
        if (forcePlayerMiss)
        {
            isFadingCrossheirs = true;
            isEnemyDodging = true;
            if (Random.value > 0.5f) // Randomize the dodge direction
            {
                enemyDodgeDistance *= -1;
            }
            enemyDodgeStartTime = crossheirFadeStartTime = Time.fixedTime;
        }
        else
        //isAttackTimeFrozen = true;
        if (immediateAttackCallback != null)
        {
            immediateAttackCallback();
            immediateAttackCallback = null;
        }
    }

    public void PlayerAttack(OnAttackEndEvent callback, bool finisher = false, OnAttackEndEvent mercyEvent = null, bool dontAllowShot = false, bool instantMercyPos = false)
    {
        attackCallback = callback;
        mercyCallback = mercyEvent;
        preventPlayerShot = dontAllowShot;
        registerInitAttackPos();
        /*if(finisher)
        {
            registerInitAttackPos();
        }*/
        if(instantMercyPos)
        {
            mercyButtonHighlighter.SetEnabled(true);
            setupFinisherLerpEnds();
            fightButton.transform.localPosition = finalAttackButtonPos;
            fightButton.transform.rotation = finalAttackButtonRot;
            mercyButton.transform.localPosition = finalMercyButtonPos;
            mercyButton.transform.rotation = finalMercyButtonRot;
            openAttackMenu(finisher);
        }
        else
            openAttackMenu(finisher, true);
    }

    private void openAttackMenu(bool finisher, bool ignoreMercyHandle = false)
    {
        isAttackMenuLive = true;
        Transform attachShield = GameObject.Find("RightShield").transform;
        //activePlayerPointer = (GameObject)Instantiate(Resources.Load("PlayerPointer"), attachShield.position, attachShield.rotation, attachShield);
        fightButton.SetActive(true);
        if(ignoreMercyHandle)
            mercyButtonHighlighter.SetEnabled(finisher);
        mercyButton.SetActive(true);
        isFinishing = finisher;
        //if(finisher && !startedButtonLerp)    // Move the buttons to where they need to go
        if (finisher)
        {
            setFinisherButtonLerp();
        }
    }

    void setupFinisherLerpEnds()
    {
        float xOffset = 0.4f;
        //registerInitAttackPos();
        //startedButtonLerp = true;
        //initAttackButtonPos = fightButton.transform.localPosition;
        //initAttackButtonRot = fightButton.transform.rotation;
        finalAttackButtonPos = new Vector3(initAttackButtonPos.x - xOffset, initAttackButtonPos.y, initAttackButtonPos.z);

        //initMercyButtonPos = mercyButton.transform.localPosition;
        //initMercyButtonRot = mercyButton.transform.rotation;
        finalMercyButtonPos = finalAttackButtonPos + new Vector3(2.0f * xOffset, 0.0f, 0.0f);
        finalMercyButtonRot = Quaternion.Euler(finalAttackButtonRot.eulerAngles.x, initMercyButtonRot.eulerAngles.y, finalAttackButtonRot.eulerAngles.z);

        finalAttackButtonRot = finalMercyButtonRot * Quaternion.Euler(Vector3.up * (-2 * finalMercyButtonRot.eulerAngles.y));
    }

    private void setFinisherButtonLerp()
    {
        setupFinisherLerpEnds();
        //startedButtonLerp = true;
        menuButtonLerpStartTime = Time.fixedTime;
        lerpingMenuButtons = true;
    }

    private void closeAttackMenu()
    {
        lerpingMenuButtons = false;
        resetAttackButtons();
        isAttackMenuLive = false;
        fightButton.SetActive(false);
        fightButtonHighlighter.ForceDeselect();
        mercyButton.SetActive(false);
        mercyButtonHighlighter.ForceDeselect();
    }

    private void onMercyButtonPress()
    {
        if(isFinishing)
        {
            //resetAttackButtons();
            isFinishing = false;
        }
        if(preventPlayerShot)
        {
            //resetAttackButtons();
        }
        //Object.Destroy(activePlayerPointer);
        //activePlayerPointer = null;
        mercyCallback();
    }

    private void onAttackButtonPress()
    {
        if(isFinishing)
        {
            //resetAttackButtons();
            isFinishing = false;
        }
        if (preventPlayerShot)
        {
            preventPlayerShot = false;
            //Object.Destroy(activePlayerPointer);
            //activePlayerPointer = null;
            //resetAttackButtons();
            attackCallback();
            return;
        }
        playerAttackStartTime = Time.fixedTime;
        float parentScaleX = enemyBody.transform.localScale.x;
        float parentScaleY = enemyBody.transform.localScale.y;
        float baseScale = 0.1f;
        leftCrossheir = GameObject.Instantiate((GameObject)Resources.Load("Crossheir"), enemyBody.transform.position, enemyBody.transform.rotation, enemyBody.transform);
        leftCrossheirRenderer = leftCrossheir.GetComponent<SpriteRenderer>();
        leftCrossheir.transform.localScale = new Vector3(baseScale / parentScaleX, baseScale / parentScaleY, 1.0f);
        rightCrossheir = GameObject.Instantiate((GameObject)Resources.Load("Crossheir"), enemyBody.transform.position, enemyBody.transform.rotation, enemyBody.transform);
        rightCrossheirRenderer = rightCrossheir.GetComponent<SpriteRenderer>();
        rightCrossheir.transform.localScale = new Vector3(baseScale / parentScaleX, baseScale / parentScaleY, 1.0f);
        leftCrossheir.GetComponent<CrossheirScript>().RegisterLibrary(this, false);
        rightCrossheir.GetComponent<CrossheirScript>().RegisterLibrary(this, true);
        if (forcePlayerMiss)
        {
            leftCrossheir.GetComponent<BoxCollider>().enabled = rightCrossheir.GetComponent<BoxCollider>().enabled = false;
        }

        domPressedThisFrame = false;

        isPlayerAttacking = true;
    }

    
    public void lightningAttack(OnAttackEndEvent callback)
    {
        crossheirActive = true;
        attackCallback = callback;
        playerAttackStartTime = Time.fixedTime;
        domPressedThisFrame = false;
        isPlayerAttacking = true;
    }
    

    public void GiveRawDamage(float damage)
    {
        if (!lightning)
        {
            damage -= Mathf.Abs(leftCrossheir.transform.position.x - rightCrossheir.transform.position.x) * 0.4f;
        }
        Damage(damage);
    }

    public void Damage(float amount)
    {
        startLerpHealth = health;
        int displayDmg;
        if (isFinishing || forceLethalHit)
        {
            health = 0;
            displayDmg = 999999999;
        }
        else
        {
            health -= amount;
            health = Mathf.Max(health, 0.0f);
            displayDmg = ((int)(amount * 10));
        }
        healthBar.SetActive(true);
        lerpStartTime = Time.fixedTime;
        isLerpingHealthBar = true;
        damageText.GetComponent<TextMesh>().text = displayDmg.ToString();
    }

    public void AddTextToQueue(string text, OnSpeechFinishedEvent callback = null)
    {
        speechQueue.Enqueue(new SpeechEntry(text, callback));
        if(!isSpeaking && speechQueue.Count == 1)
        {
            beginSpeaking();
        }
    }

    private void beginSpeaking()
    {
        speechObj.Show();
        sayNext();
        isSpeaking = true;
    }

    private void stopSpeaking()
    {
        isSpeaking = false;
        speechObj.Hide();
        if(latestSpeechCallback != null)
        {
            latestSpeechCallback();
        }
    }

    private void sayNext()
    {
        SpeechEntry ent = speechQueue.Peek();
        speechObj.SetText(ent.text);
        latestSpeechCallback = ent.callback;
        speechQueue.Dequeue();
    }

    public bool IsSpeaking()
    {
        return isSpeaking;
    }

    public void WaitForMusicFade(OnMusicFadeFinishedEvent callback)
    {
        musicFadeCallback = callback;
        waitingForMusicFade = true;
    }

    public void WaitForProjectiles(OnProjectilesFinishedEvent callback)
    {
        waitProjectilesCallback = callback;
        waitingForProjectiles = true;
    }

    public void RegisterCrossheirPositionFunc(CrossheirPositionCallback callback)
    {
        crossheirPosCallback = callback;
    }

    public float GetCrossheirPosition()
    {
        if(!isAttackTimeFrozen)
            return crossheirPosCallback(Time.fixedTime - playerAttackStartTime);
        return crossheirPosCallback(frozenAttackTime);
    }

    public void FadeoutMusic(float toVolume, float time)
    {
        musicFadeStartVolume = playerController.GetMusicVolume();
        musicFadeEndVolume = toVolume;
        musicFadeTime = time;
        musicFadeStartTime = Time.fixedTime;
        isMusicFading = true;
    }

    public void FadePitch(float toPitch, float time)
    {
        pitchFadeStartPitch = playerController.GetMusicPitch();
        pitchFadeEndPitch = toPitch;
        pitchFadeTime = time;
        pitchFadeStartTime = Time.fixedTime;
        isPitchFading = true;
    }

    public void SetPitch(float toPitch)
    {
        playerController.SetMusicPitch(toPitch);
    }

    public void PlayMusic(AudioClip clip)
    {
        playerController.PlayMusic(clip);
    }

    public void PauseMusic()
    {
        playerController.PauseMusic();
    }

    public void ResumeMusic()
    {
        playerController.ResumeMusic();
    }

    public void StopMusic()
    {
        playerController.StopMusic();
    }

    public Vector3 GetPlayerPos()
    {
        return playerController.GetPos();
    }

    public void BlindPlayer(bool activate)
    {
        playerController.Blind(activate);
    }

    public int GetAIHealth()
    {
        return (int)health;
    }

    public int GetAIMaxHealth()
    {
        return (int)maxHealth;
    }

    public int GetPlayerHealth()
    {
        return player.GetComponent<PlayerInfo>().GetHealth();
    }

    public int GetPlayerMaxHealth()
    {
        return player.GetComponent<PlayerInfo>().MaxHealth;
    }

    // Returns true if pressed this frame
    public bool GetDominantTriggerPressed()
    {
        return domPressedThisFrame;
    }

    public static bool GetDominantTriggerDown()
    {
        return OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
    }

    public static bool GetDominantGripDown()
    {
        return OVRInput.Get(OVRInput.RawButton.RHandTrigger);
    }

    public void SetLightning()
    {
        lightning = true;

        float parentScaleX = enemyBody.transform.localScale.x;
        float parentScaleY = enemyBody.transform.localScale.y;
        float baseScale = 0.1f;
        leftCrossheir = GameObject.Instantiate((GameObject)Resources.Load("Crossheir"), enemyBody.transform.position, enemyBody.transform.rotation, enemyBody.transform);
        leftCrossheirRenderer = leftCrossheir.GetComponent<SpriteRenderer>();
        leftCrossheir.transform.localScale = new Vector3(baseScale / parentScaleX, baseScale / parentScaleY, 1.0f);
        leftCrossheir.GetComponent<CrossheirScript>().RegisterLightningLibrary(this, false);

        createPointer();
        activePlayerPointer.layer = 9;
        activePlayerPointer.tag = "Untagged";
    }
}
