using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Inspired from
// https://www.youtube.com/watch?v=iD1_JczQcFY&ab_channel=CodeMonkey
public class PopupText : MonoBehaviour
{
    TextMeshPro textMesh;
    float disappearTimer;
    Color textColor;
    private Vector3 moveVector;

    private const float DISAPPER_TIMER_MAX = 1f;

    public static PopupText Create(GameObject prefab, Vector3 location, float amount)
    {
        var textTransform = Instantiate(prefab, location, Quaternion.identity);
        var popup = textTransform.GetComponent<PopupText>();
        popup.Setup(amount);
        return popup;
    }

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();    
    }

    public void Setup(float amount)
    {
        int positiveInt = Mathf.RoundToInt(Mathf.Abs(amount));
        textMesh.text = positiveInt.ToString();
        if (amount < 0)
        {
            textMesh.fontSize = 36;
            textColor = Color.red;
        } else
        {
            textMesh.fontSize = 24;
            textColor = Color.green;
        }
        textMesh.color = textColor;
        disappearTimer = DISAPPER_TIMER_MAX;
        moveVector = new Vector3(1, 1) * 30f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPER_TIMER_MAX * 0.5f)
        {
            // First half of animation.
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        } else
        {
            // Second half of animation.
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
