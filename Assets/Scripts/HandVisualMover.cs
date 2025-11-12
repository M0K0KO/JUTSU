using System;
using UnityEngine;

public class HandVisualMover : MonoBehaviour
{
    [SerializeField] private PlayerManager player;
    [SerializeField] private Vector3 positionOffset;

    private void Update()
    {
        
    }


    private void MoveVisual()
    {
        player.transform.position += positionOffset;
    }
}
