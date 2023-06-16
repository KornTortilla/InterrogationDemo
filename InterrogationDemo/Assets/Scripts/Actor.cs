using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public bool isActing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    public IEnumerator Fade(float toValue, float time)
    {
        isActing = true;

        float fromValue = spriteRenderer.color.a;

        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(fromValue, toValue, t));
            spriteRenderer.color = newColor;
            yield return null;
        }

        spriteRenderer.color = new Color(1, 1, 1, 1);

        isActing = false;
    }
}
