using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer; // Reference to the AudioMixer
    public void SetVolume(float volume)
    {
         audioMixer.SetFloat("volume", volume); // Set the volume parameter in the AudioMixer)
    }
}
