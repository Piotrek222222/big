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
    public TMP_Dropdown ColorDropdown;
    public Toggle fullscreenToggle;
    public Color ropeAColor = Color.red;
    public Color ropeBColor = Color.blue;
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
    ColorDropdown.value = PlayerPrefs.GetInt("Colors", 0); 
    ColorDropdown.onValueChanged.AddListener(SetColor); 
    SetColor(ColorDropdown.value);
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

    public void SetColor(int index)
    {
        switch (index)
        {
            case 0:
                ropeAColor = Color.red;
                ropeBColor = Color.blue;
                break;

            case 1:
                ropeAColor = Color.green;
                ropeBColor = Color.yellow;
                break;

            case 2:
                ropeAColor = Color.black;
                ropeBColor = Color.white;
                break;

            case 3:
                ropeAColor = Color.cyan;
                ropeBColor = Color.magenta;
                break;
            case 4:
                ropeAColor = Color.white;
                ropeBColor = Color.white;
                break;
            default:
                ropeAColor = Color.white;
                ropeBColor = Color.white;
                break;
        }
        PlayerPrefs.SetString("RopeAColor", ColorUtility.ToHtmlStringRGBA(ropeAColor));
        PlayerPrefs.SetString("RopeBColor", ColorUtility.ToHtmlStringRGBA(ropeBColor));
        PlayerPrefs.SetInt("Colors", index);
        PlayerPrefs.Save();
    }
    public void SetFullScreen(bool isFullscreen )
    {
    Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    Screen.fullScreen = isFullscreen;
    PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    PlayerPrefs.Save();
    }
}