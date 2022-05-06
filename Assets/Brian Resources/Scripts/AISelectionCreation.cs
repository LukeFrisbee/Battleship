using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class AISelectionCreation : MonoBehaviour
{
    public string path;

    private List<string> dlls; 

    [SerializeField] private GameObject EmptyDLL; 
    [SerializeField] private AISelection[] selections;

    private void NoDLL()
    {
        EmptyDLL.SetActive(true);  
        foreach (var box in selections)
            box.enabled = false;  
    }

    // Start is called before the first frame update
    void Start()
    {
        path = BritoUtil.ReadSettings(); 
        dlls = new List<string>(); 

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
            if (dll.Contains("BattleshipAgent.dll")) continue;  
            dlls.Add(dll);
        } 

        //Called last
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        ClickHandler();
    }

    private Vector2 startPos;

    private void ClickHandler()
    {
        if (Input.GetMouseButtonDown(0)) 
            startPos = Input.mousePosition;
        if (Input.GetMouseButtonUp(0))
        {
            var endPos = Input.mousePosition;
            var dist = Vector2.Distance(startPos, endPos); 
            if (dist > .1f)
                return;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, result);

            foreach (var item in result)
            {
                if (item.gameObject.layer != 20)
                    continue;

                string[] split = item.gameObject.transform.parent.name.Split(':'); 
                int id = int.Parse(split[0]);
                int index = Mathf.Abs(int.Parse(split[1]) - dlls.Count) - 1;
                string name = split[2];

                selections[id].SetContent(index);
                return;
            } 
        }
    }

    public void Initialize()
    {   
        var aisel = GetComponents<AISelection>(); 
        foreach(var box in aisel)
            box.Initialize(dlls); 
    }

    public void ReturnToMainMenu()
    {
        SceneController.Instance.LoadScene(1);
    }

    public void ButtonBattleField()
    {
        var scene = SceneController.Instance; 
        for (int i = 0; i < 2; i++) 
            scene.paths[i] = selections[i].selectedDLL; 
        scene.ToBattleScreen();
    }
}
