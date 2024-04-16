using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    /*[HideInInspector]*/ public float damage;
    [SerializeField] float minSize;
    [SerializeField] float maxSize;
    float size;

    void Start()
    {
        transform.localScale = Vector3.zero;
        GetComponent<TextMeshPro>().text = damage.ToString();
        if (damage < 250) size = Mathf.Lerp(minSize, maxSize, damage / 250.0f);
        else size = Mathf.Lerp(maxSize, 0.3f, damage / 1000f);
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float currentSize = 0.0f;
        while (currentSize < size)
        {
            currentSize += Time.deltaTime * size * 10;
            transform.localScale = Vector3.one * currentSize;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        while (currentSize > 0.0f)
        {
            currentSize -= Time.deltaTime * size * 10;
            transform.localScale = Vector3.one * currentSize;
            yield return null;
        }
        Destroy(transform.parent.gameObject);
    }
}
