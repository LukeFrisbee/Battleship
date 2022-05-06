using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private GameObject titleMenu, settingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayButton()
    {
        SceneLoader.Instance.ToAISelectionScreen();
    }

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
}
