using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AISelectionCreation : MonoBehaviour
{
    public string path;

    private SortedDictionary<string, string> dlls;

    [SerializeField] private GameObject EmptyDLL;

    private void NoDLL()
    {
        EmptyDLL.SetActive(true);
        var aisel = GetComponents<AISelection>();

        foreach (var box in aisel)
            box.enabled = false;  
    }

    // Start is called before the first frame update
    void Start()
    {
        path = BritoUtil.ReadSettings();

        dlls = new SortedDictionary<string, string>();

        if (path.Length < 2)
        {
            NoDLL();
            print("Returning due to bad url");
            return;
        }

        var files = Directory.GetFiles(path, "*.dll");


        if (files.Length == 0)
        {
            NoDLL();
            print("Returning due no dlls in folder");
            return;
        }

        foreach (string dll in files)
        {
            var key = Path.GetFileName(dll).Replace(".dll", "");
            dlls.Add(key, dll);
        }


        //Called last
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    { 
        //for (int i = 0; i < 10; i++)
        //    dlls.Add("OPTION " + i, "path");


        var aisel = GetComponents<AISelection>();

        foreach(var box in aisel)
            box.Initialize(dlls); 
    }

    public void ReturnToMainMenu()
    {
        SceneLoader.Instance.LoadScene(1);
    }

    public void ButtonBattleField()
    {
        SceneLoader.Instance.LoadScene(3);
    }
}
