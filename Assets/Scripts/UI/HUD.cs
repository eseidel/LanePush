using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public MOB player;

    public TextMeshProUGUI gold;
    public Image deathScreen;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // If player is dead show grey?
        gold.text = "Gold: " + player.Gold;
        deathScreen.gameObject.SetActive(!player.isAlive);
    }
}
