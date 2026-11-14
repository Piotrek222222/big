using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetMusic(0);
    }

    public void SetMusic(int index)
    {
        if (index >= 0 && index < Music.Count)
        {
            MusicSource.clip = Music[index];
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