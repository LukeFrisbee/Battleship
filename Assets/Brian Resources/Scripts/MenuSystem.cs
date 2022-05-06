using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private GameObject titleMenu, settingsMenu; 
    public void PlayButton() => SceneController.Instance.ToAISelectionScreen(); 

    public void SettingsButton()
    {
        settingsMenu.SetActive(true);
        titleMenu.SetActive(false);
    }

    public void BackButton()
    {
        settingsMenu.SetActive(false);
        titleMenu.SetActive(true);
    }

    public void ExitButton()
    {
        print("stopping!");
        Application.Quit();
    }
}
