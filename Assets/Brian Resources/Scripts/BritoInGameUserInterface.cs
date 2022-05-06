using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BritoInGameUserInterface : MonoBehaviour
{ 

    [SerializeField] private GameObject SettingsContainer;
    [SerializeField] private GameObject WinnerContainer;
    [SerializeField] private TMPro.TMP_Text winnerName;

    public void ButtonSettings() => SettingsContainer.SetActive(!SettingsContainer.activeSelf);  
    public void ButtonAISelect() => SceneController.Instance.LoadScene(2); 
    public void ButtonTitleScreen() => SceneController.Instance.LoadScene(1);

    [SerializeField] private TMPro.TMP_Text[] left, right;

    private void Start()
    { 
        SettingsContainer.SetActive(false);
        WinnerContainer.SetActive(false);
    } 

    public void QueueMessage(int playerID, string msg)
    {
        TMPro.TMP_Text[] text;
        if (playerID == 0) text = left;
        else text = right;

        for (int i = 0; i < text.Length; i++)
        {
            int j = i + 1;
            if (j >= text.Length)
                text[i].text = msg;
            else
                text[i].text = text[j].text; 
        }
    }

    public void TriggerWin(int playerID)
    {
        SettingsContainer.SetActive(false);
        winnerName.text = "PLAYER " + playerID; 
        WinnerContainer.SetActive(true);
    }
}
