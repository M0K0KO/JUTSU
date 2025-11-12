using System;
using UnityEngine;

public class OptionLoader : MonoBehaviour
{
    private void Awake()
    {
        var optionController = FindFirstObjectByType<OptionPanelController>(FindObjectsInactive.Include);
        if (optionController != null)
        {
            optionController.LoadSavedOptions();
        }
    }
}
