using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal.Internal;


public class AutoSceneLoader : MonoBehaviour
{
    public float delay = 3f;         // czas czekania w sekundach
    public string nextSceneName ="Main Menu";     // nazwa następnej sceny

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    public IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }
}
