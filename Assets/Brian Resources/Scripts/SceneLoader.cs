using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public Animator anim;
    public float transitionTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject); 
        StartCoroutine(LoadLevel(1));
    }

    public void ToAISelectionScreen()
    {
        StartCoroutine(LoadLevel(2));
    }

    public void LoadScene(int level)
    {
        StartCoroutine(LoadLevel(level));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator LoadLevel(int level)
    {
        anim.SetTrigger("In");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);

        anim.SetTrigger("Out");
    }
}
