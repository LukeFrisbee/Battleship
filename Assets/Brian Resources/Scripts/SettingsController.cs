using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{

	[SerializeField] private TMPro.TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
		text.text = BritoUtil.ReadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void BrowsePath()
	{
		StartCoroutine(ShowLoadDialogCoroutine());
	}
	

	IEnumerator ShowLoadDialogCoroutine()
	{ 
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, true, null, null, "DLL Folder", "Load"); 
		Debug.Log(FileBrowser.Success); 
		if (FileBrowser.Success)
		{
			// Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
			for (int i = 0; i < FileBrowser.Result.Length; i++)
				Debug.Log(FileBrowser.Result[i]);

			SaveToDisk(FileBrowser.Result[0]);
		}
	}

	private void SaveToDisk(string path)
    { 
		text.text = path;
		BritoUtil.WriteSettings(path);
    }
}
