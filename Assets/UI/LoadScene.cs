using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    public AudioManager audioManager;
    public Image fadeImage;
    public float fadeDuration = 1f;
    void Start()
    {
        if (fadeImage != null){
        StartCoroutine(FadeIn());
        }
    }

    public void LoadLocalScene(string scene)
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.Button_pressed,1.0f);
        else{
            Debug.Log("ddd");
        }
        if (GetComponent<PauseMenu>() != null && GetComponent<PauseMenu>().isPaused)
        {
            Time.timeScale = 1f;
        }
        if (fadeImage != null){
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeOut());
        }
        SceneManager.LoadScene(scene);
    }
    public void Quit(){
        Application.Quit();
    }
    
    public IEnumerator FadeIn()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }
        SetAlpha(0f);
        yield return new WaitForSeconds(0.5f);
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }
        SetAlpha(1f);
    }

    void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }  
   
}
