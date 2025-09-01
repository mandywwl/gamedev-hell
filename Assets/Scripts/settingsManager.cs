using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
// using UnityEngine.UI;
// using TMPro;

public class settingsManager : MonoBehaviour
{
    // public TMP_Dropdown ResDropDown;
    // public TMP_Dropdown QualityDropDown;
    // public Toggle FullScreenToggle;
    // public Slider Brightness;
    // public Slider Sound;

    // public static settingsManager Instance { get; private set; }
    private static settingsManager _instance;
    private static bool isShuttingDown = false;

    public static settingsManager Instance
    {
        get
        {
            if (isShuttingDown) return null;

            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = UnityEngine.Object.FindAnyObjectByType<settingsManager>(); // future-proof using new API
            }

            if (_instance == null)
            {
                // If no instance exists, create a new GameObject and add this script to it
                GameObject singletonObject = new GameObject("settingsManager (Singleton)");
                _instance = singletonObject.AddComponent<settingsManager>();
                Debug.Log("settingsManager instance created automatically.");
            }

            return _instance;
        }
    }

    // Serialized Fields
    [SerializeField] private VolumeProfile brightnessProfile; // NOTE: Assign in Inspector
    [SerializeField]private AudioMixer mainMixer;
    [SerializeField] private string masterParam = "MasterVolume";


    private ColorAdjustments colourAdj;

    // List populated by SettingsUIBinder script
    public List<Resolution> Resolutions;
    
    private bool IsFullScreen;
    private int SelectedResolution;

    // List<Resolution> SelectedResolutionList = new List<Resolution>();

    // int SelectedQuality;
    // List<string> SelectedQualityList = new List<string>();
    // string[] AllQualitynames;

    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    void Awake()
    {
        // if (brightnessProfile == null)
        // {
        //     Debug.LogError("No Volume Profile assigned!");
        //     return;
        // }

        // if (!brightnessProfile.TryGet(out colourAdj))
        // {
        //     Debug.LogError("BrightnessSetting profile has no ColorAdjustments override!");
        //     return;
        // }

        // colourAdj.postExposure.overrideState = true;

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Brightness setup
        if (brightnessProfile != null && brightnessProfile.TryGet(out colourAdj))
        {
            colourAdj.postExposure.overrideState = true;
        }
    }

    void Start()
    {
        // Load saved settings or set defaults
        float savedVolume = PlayerPrefs.GetFloat("sound", 1.0f);
        ChangeSound(savedVolume);

        float savedBrightness = PlayerPrefs.GetFloat("brightness", 0.5f);
        ChangeBrightness(savedBrightness);

    }

    public void InitializeScreenSettings()
    {
        var rawFullscreen = PlayerPrefs.GetString("togglefullscreen", bool.TrueString);
        IsFullScreen = bool.TryParse(rawFullscreen, out var val) ? val : true;

        SelectedResolution = PlayerPrefs.GetInt("resolution", 0);

        // Ensure saved resolution index is valid for the available resolutions
        if (Resolutions != null && SelectedResolution < Resolutions.Count)
        {
            Screen.SetResolution(Resolutions[SelectedResolution].width, Resolutions[SelectedResolution].height, IsFullScreen);
        }
    }

    public void ChangeResolution(int index)
    {
        if (Resolutions == null || Resolutions.Count == 0) return;

        PlayerPrefs.SetInt("resolution", index);
        SelectedResolution = index;
        Screen.SetResolution(Resolutions[SelectedResolution].width, Resolutions[SelectedResolution].height, IsFullScreen);
        PlayerPrefs.Save();
    }

    // public void ChangeQuality(int index)
    // {
    //     PlayerPrefs.SetInt("quality", index);
    //     SelectedQuality = index;
    //     QualitySettings.SetQualityLevel(SelectedQuality, true);

    //     PlayerPrefs.Save();
    // }

    public void ChangeFullScreen(bool isOn)
    {
        IsFullScreen = isOn;
        PlayerPrefs.SetString("togglefullscreen", IsFullScreen.ToString());

        if (Resolutions != null && Resolutions.Count > SelectedResolution)
        {
            Screen.SetResolution(Resolutions[SelectedResolution].width, Resolutions[SelectedResolution].height, IsFullScreen);
        }

        PlayerPrefs.Save();
    }

    public void ChangeBrightness(float value)
    {
        PlayerPrefs.SetFloat("brightness", value);
        if (colourAdj != null)
        {
            colourAdj.postExposure.value = value;
        }
        PlayerPrefs.Save();
    }

    public void ChangeSound(float value)
    {
        PlayerPrefs.SetFloat("sound", value);
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;

        Debug.Log($"[Settings] slider={value:F3} -> {db:F1} dB | mixer={(mainMixer ? mainMixer.name : "NULL")} | param={masterParam}");

        if (!mainMixer) return;

        mainMixer.SetFloat(masterParam, db);
        float readBack;
        bool ok = mainMixer.GetFloat(masterParam, out readBack);
        Debug.Log($"[Settings] SetFloat ok? {ok} | readBack={readBack:F1} dB");
        PlayerPrefs.Save();
    }

}
