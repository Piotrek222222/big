using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
public class Settings : MonoBehaviour
{
    public AudioMixer Mixer;
    public Slider Music_Slider;
    public Slider SFX_Slider;
    public Toggle fullscreenToggle;
    public GameObject Player;

    private void Start()
    {
    Player = GameObject.FindWithTag("Player");

    if (Mixer.GetFloat("Music", out float value_Music))
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            value_Music = PlayerPrefs.GetFloat("MusicVolume");
            Mixer.SetFloat("Music", value_Music);
            Music_Slider.value = value_Music;
            }
        else
            Music_Slider.value = value_Music;
    }
    if (Mixer.GetFloat("SFX", out float value_SFX))
    {
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            value_SFX = PlayerPrefs.GetFloat("SFXVolume");
            Mixer.SetFloat("SFX", value_SFX);
            SFX_Slider.value = value_SFX;
        }
        else
            SFX_Slider.value = value_SFX;
    }

    QualitySettings.vSyncCount = 0; 
    fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    fullscreenToggle.onValueChanged.AddListener(SetFullScreen);
    SetFullScreen(fullscreenToggle.isOn);

    }

    public void SetMusicVolume(){
        float volume = Music_Slider.value;
        Mixer.SetFloat("Music",volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume(){
        float volume = SFX_Slider.value;
        Mixer.SetFloat("SFX",volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

   
    public void SetFullScreen(bool isFullscreen )
    {
    Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    Screen.fullScreen = isFullscreen;
    PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    PlayerPrefs.Save();
    Debug.Log("nigga");
    }
}