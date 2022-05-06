using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    public Animator anim;
    public float transitionTime = 1f;

    public string[] paths;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        paths = new string[2]; 
        DontDestroyOnLoad(gameObject); 
        StartCoroutine(LoadLevel(1));
    }

    public void ToAISelectionScreen() => StartCoroutine(LoadLevel(2));
    public void LoadScene(int level) => StartCoroutine(LoadLevel(level));
    public void ToBattleScreen() => StartCoroutine(LoadLevel(3)); 

    private IEnumerator LoadLevel(int level)
    {
        anim.SetTrigger("In");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);

        anim.SetTrigger("Out");
    } 
}
