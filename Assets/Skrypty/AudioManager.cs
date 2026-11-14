using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    private static AudioManager instance;
    public List<AudioClip> Music = new List<AudioClip>();
    public List<AudioClip> SFX = new List<AudioClip>();

    public int Music_number=0;
    public bool isPlaying=false;
    public int savedIndex =1;
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
            MusicSource.clip= Music[index];
        }
        if (MusicSource.isPlaying==false)
        {
            isPlaying=true;
            MusicSource.loop = true;  
            MusicSource.Play();
        }
    }


    public void PlaySFX(AudioClip clip, float volume=1f)
    {
        SFXSource.PlayOneShot(clip, volume);
    }

}


