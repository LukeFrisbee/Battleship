using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Battleship;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        print("test");
        string dllName = "C:\\Users\\lukeb\\OneDrive\\Documents\\Unity Creations\\Unity Creations\\Battleship\\Assets\\Agents\\BozoTheClown.dll";

        string battleshipAgentBaseClassName = "Battleship.BattleshipAgent";
        Assembly agentAssembly = Assembly.LoadFile(dllName);

        foreach (Type type in agentAssembly.ExportedTypes)
        {
            if (type.BaseType.FullName == battleshipAgentBaseClassName)
            {
                var agent = Activator.CreateInstance(type, null);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
