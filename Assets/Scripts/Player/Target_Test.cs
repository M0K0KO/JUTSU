using System.Collections;
using UnityEngine;

interface IDamageable_TEST
{
    public void OnDamage(float damage);
}

public class Target_Test : MonoBehaviour, IDamageable_TEST
{
    private Material material;
    private Coroutine onDamageCoroutine;
    
    private Color originalColor;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
    }

    public void OnDamage(float damage)
    {
        if (onDamageCoroutine != null) StopCoroutine(onDamageCoroutine);
        StartCoroutine(ChangeColor());
    }

    private IEnumerator ChangeColor()
    {
        material.SetColor("_BaseColor", Color.red);
        yield return new WaitForSeconds(0.2f);
        material.SetColor("_BaseColor", originalColor);
    }
}
