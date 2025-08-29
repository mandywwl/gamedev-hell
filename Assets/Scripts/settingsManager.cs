using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class settingsManager : MonoBehaviour
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullScreenToggle;

    //public TMP_Dropdown QualityDropDown;

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

    public static settingsManager Instance { get; private set; }

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

        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        //QualityDropDown.AddOptions(SelectedQualityList);

    }

    public void ChangeResolution(int index)
    {
        if (SelectedResolutionList == null || SelectedResolutionList.Count == 0) return;

        PlayerPrefs.SetInt("resolution", index);
        SelectedResolution = index;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, IsFullScreen);

        PlayerPrefs.Save();
    }

    public void ChangeQuality(int index)
    {
        PlayerPrefs.SetInt("quality", index);
        SelectedQuality = index;
        QualitySettings.SetQualityLevel(SelectedQuality, true);

        PlayerPrefs.Save();
    }

    public void ChangeFullScreen(bool isOn)
    {
        IsFullScreen = isOn;
        PlayerPrefs.SetString("togglefullscreen", IsFullScreen.ToString());
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, IsFullScreen);

        PlayerPrefs.Save();
    }

    public void ChangeBrightness(float value1)
    {
        PlayerPrefs.SetFloat("brightness", value1);
        colourAdj.postExposure.value = PlayerPrefs.GetFloat("brightness");

        PlayerPrefs.Save();
    }

    public void ChangeSound(float value1)
    {
        PlayerPrefs.SetFloat("sound", value1);
        AudioListener.volume = PlayerPrefs.GetFloat("sound");
        // REMEMBER TO ADD THIS TO EVERY SCENE ----> AudioListener.volume = PlayerPrefs.GetFloat("sound");

        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
