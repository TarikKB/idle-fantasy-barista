using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingsScript : MonoBehaviour
{

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Dropdown maxFPSDropdown;

    void Start()
    {
        // Load saved volume settings
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        int savedMaxFPS = PlayerPrefs.GetInt("MaxFPS", 0);

        // Set sliders to saved values
        musicVolumeSlider.value = savedMusicVolume;
        sfxVolumeSlider.value = savedSFXVolume;
        maxFPSDropdown.value = savedMaxFPS;

        // Apply the volume settings
        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
        SetMaxFPS(savedMaxFPS);
        
    }
    public void SetMusicVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMaxFPS(int maxFPS)
    {
        int calculatedFPS = (int)(Mathf.Pow(2, maxFPS) * 30);
        // print ("Setting Max FPS to: " + calculatedFPS);
        Application.targetFrameRate = calculatedFPS;
        PlayerPrefs.SetInt("MaxFPS", maxFPS);
        PlayerPrefs.Save();
    }
}
