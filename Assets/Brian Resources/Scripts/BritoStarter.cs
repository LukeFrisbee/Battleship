using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BritoStarter : MonoBehaviour
{ 
    private Battleship.BattleshipRunner runner;

    // Start is called before the first frame update
    void Start()
    {
        runner = GameObject.FindGameObjectWithTag("Runner").GetComponent<Battleship.BattleshipRunner>(); 
        runner.args = (string[])SceneController.Instance.paths.Clone(); 
        runner.RunBattle();
    } 
}
