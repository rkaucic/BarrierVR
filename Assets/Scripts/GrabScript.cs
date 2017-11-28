using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabScript : MonoBehaviour {

    private bool isAttackButtonHovered;

    public void Start()
    {
        isAttackButtonHovered = false;
    }

    public bool IsAttackButtonHovered()
    {
        return isAttackButtonHovered;
    }

    void OnTriggerEnter(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.name == "AttackButton")
        {
            isAttackButtonHovered = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject other = collider.gameObject;
        if (other.name == "AttackButton")
        {
            isAttackButtonHovered = false;
        }
    }
}
