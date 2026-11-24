using System;
using TMPro;
using UnityEngine;

public class scr_debug : MonoBehaviour
{
    public scr_player player;
    private TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (player.DEBUG)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
        else
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Spd - " + Math.Abs(player.rigidbody.linearVelocityX) + "\n" + "Mode - " + player.mode;
    }
}
