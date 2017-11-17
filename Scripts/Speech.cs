using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;   // Not a default "using"

public class Speech : MonoBehaviour {

    private Text text;
	void Start () {
        text = this.gameObject.transform.Find("Canvas").Find("Text").GetComponent<Text>();
        setText("You shouldn't be reading this!");
        Hide();
	}
	
	void Update () {
        
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void SetText(string newText)
    {
        setText(newText);
    }

    private void setText(string newText)
    {
        text.text = newText;
    }
}
