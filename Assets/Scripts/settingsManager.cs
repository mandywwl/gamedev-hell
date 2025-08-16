using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

public class settingsManager : MonoBehaviour
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullScreenToggle;

    public TMP_Dropdown QualityDropDown;

    public Slider Brightness;
    public Slider Volume;

    Resolution[] AllResolutions;
    bool IsFullScreen;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    int SelectedQuality;
    List<string> SelectedQualityList = new List<string>();
    string[] AllQualitynames;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsFullScreen = true;
        AllResolutions = Screen.resolutions;

        List<string> resolutionStringList = new List<string>();
        string newRes;
        foreach (Resolution res in AllResolutions)
        {
            newRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(res);
            }
        }
        ResDropDown.AddOptions(resolutionStringList);

        AllQualitynames = QualitySettings.names;
        foreach (string qual in AllQualitynames)
        {
            SelectedQualityList.Add(qual);
        }
        QualityDropDown.AddOptions(SelectedQualityList);

    }

    public void ChangeResolution()
    {
        PlayerPrefs.SetInt("resolution", ResDropDown.value);
        SelectedResolution = ResDropDown.value;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, IsFullScreen);
    }

    public void ChangeQuality()
    {
        PlayerPrefs.SetInt("quality", QualityDropDown.value);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality"));
    }

    public void ChangeFullScreen()
    {
        IsFullScreen = FullScreenToggle.isOn;
        PlayerPrefs.SetString("togglefullscreen", IsFullScreen.ToString());
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, IsFullScreen);
    }

    public void ChangeBrightness()
    {
        PlayerPrefs.SetFloat("brightness", Brightness.value);
        Screen.brightness = PlayerPrefs.GetFloat("brightness");
    }

    public void ChangeVolume()
    {
        PlayerPrefs.SetFloat("volume", Volume.value);
        AudioListener.volume = PlayerPrefs.GetFloat("volume");
        // REMEMBER TO ADD THIS TO EVERY SCENE ----> AudioListener.volume = PlayerPrefs.GetFloat("volume");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
