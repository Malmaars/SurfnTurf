using System;
using UnityEngine;
using UnityEngine.UI;

public class FMODVolumeSlider : MonoBehaviour
{
    //lower the master volume with the slider thats on this object
    private Slider slider;
    FMOD.Studio.Bus masterBus;
    [SerializeField] private String busName = "bus:/"; // The name of the bus to control
    [SerializeField] private String prefName = "MasterVolume"; // The name of the bus to control

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(SetVolume);
        slider.value = PlayerPrefs.GetFloat(prefName, 1f);
        masterBus = FMODUnity.RuntimeManager.GetBus(busName);
    }
    private void SetVolume(float value)
    {
        //set master bus volume
        masterBus.setVolume(value);
        PlayerPrefs.SetFloat(prefName, value);
        PlayerPrefs.Save();
    }

}
