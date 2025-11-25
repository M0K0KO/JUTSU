using UnityEngine;

public class RockSphereArranger : MonoBehaviour
{
    public float sphereRadius = 5f;
    
    [ContextMenu("Arrange Rocks")]
    public void ArrangeRocks()
    {
       
        foreach (Transform child in transform)
        {
            var randomPos = Random.insideUnitSphere * sphereRadius;

            child.localPosition = randomPos;

            child.localRotation = Random.rotation;

        }

        Debug.Log($"{transform.childCount} rocks arranged.");
    }
}