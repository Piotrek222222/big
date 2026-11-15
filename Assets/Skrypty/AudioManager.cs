using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public static AudioManager instance;
    public List<AudioClip> Music = new List<AudioClip>();
    public List<AudioClip> SFX = new List<AudioClip>();

    public int Music_number = 0;
    public bool isPlaying = false;
    public int savedIndex = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        SprawdzMuzykeDlaSceny(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SprawdzMuzykeDlaSceny(scene.name);
    }

    private void SprawdzMuzykeDlaSceny(string nazwaSceny)
    {
        if (nazwaSceny == "BossBattle" || nazwaSceny == "Dices")
        {
            SetMusic(1);
        }
        else
        {
            SetMusic(0);
        }
    }

    public void SetMusic(int index)
    {
        if (index >= 0 && index < Music.Count)
        {
            if (MusicSource.clip != Music[index])
            {
                MusicSource.clip = Music[index];
                MusicSource.loop = true;
                MusicSource.Play();
                isPlaying = true;
                Music_number = index;
                return;
            }
        }
        if (MusicSource.isPlaying == false)
        {
            isPlaying = true;
            MusicSource.loop = true;
            MusicSource.Play();
        }
    }

    // Działa, gdy podasz konkretny plik AudioClip
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && SFXSource != null)
        {
            SFXSource.PlayOneShot(clip, volume);
        }
    }

    // Działa, gdy podasz numer z listy SFX (np. AudioManager.instance.PlaySFX(2))
    public void PlaySFX(int index, float volume = 1f)
    {
        if (index >= 0 && index < SFX.Count && SFXSource != null)
        {
            SFXSource.PlayOneShot(SFX[index], volume);
        }
    }
}