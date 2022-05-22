using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LookAtCamera))]
public class StatusBar : MonoBehaviour
{
    public TextMeshProUGUI statusBarUI;

    public void SetText(string text)
    {
        statusBarUI.text = text;
    }
}
