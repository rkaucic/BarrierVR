using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonHighligher : MonoBehaviour
{

    public Sprite UnselectedSprite;
    public Sprite SelectedSprite;
    public bool EnabledOnStart = true;

    private SpriteRenderer spriteRenderer;
    private Color disabledColor;
    private Color enabledColor;
    private bool isHovered;
    private bool isEnabled;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isHovered = false;
        disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        enabledColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        isEnabled = EnabledOnStart;
        if (!enabled)
            spriteRenderer.color = disabledColor;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetEnabled(bool e)
    {
        isEnabled = e;
        if (e)
            spriteRenderer.color = enabledColor;
        else
            spriteRenderer.color = disabledColor;
    }

    public bool IsHovered()
    {
        return isHovered;
    }

    public void ForceDeselect()
    {
        spriteRenderer.sprite = UnselectedSprite;
        isHovered = false;
    }

    void OnTriggerEnter(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.gameObject.CompareTag("Pointer") && isEnabled)
        {
            isHovered = true;
            spriteRenderer.sprite = SelectedSprite;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.gameObject.CompareTag("Pointer") && isEnabled)
        {
            isHovered = false;
            spriteRenderer.sprite = UnselectedSprite;
        }
    }
}
