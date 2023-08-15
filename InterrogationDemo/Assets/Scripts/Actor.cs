using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public string screenName;
    public bool isActing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    public IEnumerator Slide(float toValue, float time)
    {
        float fromValue = transform.position.x;
        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            Vector3 position = new Vector3(Mathf.Lerp(fromValue, toValue, t), transform.position.y, transform.position.z);
            transform.position = position;
            yield return null;
        }

        transform.position = new Vector3(toValue, transform.position.y, transform.position.z);
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

        spriteRenderer.color = new Color(1, 1, 1, toValue);

        isActing = false;
    }

    public void StepBack()
    {
        StartCoroutine(ChangeColor(0.2f, 0.2f));
    }

    public void StepIn()
    {
        StartCoroutine(ChangeColor(1f, 0.2f));
    }

    private IEnumerator ChangeColor(float toValue, float time)
    {
        float fromValue = spriteRenderer.color.b;

        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            float timeNumber = Mathf.Lerp(fromValue, toValue, t);
            Color newColor = new Color(timeNumber, timeNumber, timeNumber, 1f);
            spriteRenderer.color = newColor;
            yield return null;
        }

        spriteRenderer.color = new Color(toValue, toValue, toValue, 1f);
    }
}
