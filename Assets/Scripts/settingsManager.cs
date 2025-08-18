using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using NUnit.Framework;

public class settingsManager : MonoBehaviour
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullScreenToggle;

    public TMP_Dropdown QualityDropDown;

    public Slider Brightness;
    [SerializeField] private VolumeProfile brightnessProfile;
    private ColorAdjustments colourAdj;

    public Slider Sound;

    Resolution[] AllResolutions;
    bool IsFullScreen;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    int SelectedQuality;
    List<string> SelectedQualityList = new List<string>();
    string[] AllQualitynames;

    void Awake()
    {
        if (brightnessProfile == null)
        {
            Debug.LogError("No Volume Profile assigned!");
            return;
        }

        if (!brightnessProfile.TryGet(out colourAdj))
        {
            Debug.LogError("BrightnessSetting profile has no ColorAdjustments override!");
            return;
        }

        colourAdj.postExposure.overrideState = true;
        colourAdj.postExposure.value = PlayerPrefs.GetFloat("brightness", 0f);
    }

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
        SelectedQuality = QualityDropDown.value;
        QualitySettings.SetQualityLevel(SelectedQuality, true);

        //Debug.Log("Quality settings: " + QualitySettings.GetQualityLevel());
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
        colourAdj.postExposure.value = PlayerPrefs.GetFloat("brightness");
        Debug.Log("Brightness: " + colourAdj.postExposure.value);
    }

    public void ChangeSound()
    {
        PlayerPrefs.SetFloat("sound", Sound.value);
        AudioListener.volume = PlayerPrefs.GetFloat("sound");
        // REMEMBER TO ADD THIS TO EVERY SCENE ----> AudioListener.volume = PlayerPrefs.GetFloat("sound");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
