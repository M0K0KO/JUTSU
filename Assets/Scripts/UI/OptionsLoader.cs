using System;
using UnityEngine;

public class OptionsLoader : MonoBehaviour
{
    private void Start()
    {
        var optionController = FindFirstObjectByType<OptionsPanelController>(FindObjectsInactive.Include);
        if (optionController != null)
        {
            optionController.LoadSavedOptions();
        }
    }
}
