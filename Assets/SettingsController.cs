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
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, true, null, null, "DLL Folder", "Load");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
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
