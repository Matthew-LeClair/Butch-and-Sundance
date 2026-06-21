using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer; // Reference to the AudioMixer
    public void SetVolume(float Volume)
    {
        Debug.Log("Volume set to: " + Volume); // Log the volume value for debugging purposes

        audioMixer.SetFloat("Volume", Volume); // Set the volume parameter in the AudioMixer)
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex); // Set the quality level based on the index
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen; // Set the fullscreen mode based on the boolean value
    }
}
