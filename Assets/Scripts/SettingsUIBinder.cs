using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUIBinder : MonoBehaviour
{
    [Header("Controls in Game scene")]
    public Slider brightness;
    public Slider sound;
    //public TMP_Dropdown quality;
    public TMP_Dropdown resolution;
    public Toggle fullscreen;

    Resolution[] AllResolutions;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    int SelectedQuality;
    List<string> SelectedQualityList = new List<string>();
    string[] AllQualitynames;

    void Awake()
    {
        // Initialize UI from saved prefs (without firing callbacks)
        if (brightness) brightness.SetValueWithoutNotify(PlayerPrefs.GetFloat("brightness", 0.5f));
        if (sound) sound.SetValueWithoutNotify(PlayerPrefs.GetFloat("sound", 1.0f));
        if (fullscreen) fullscreen.SetIsOnWithoutNotify(GetSavedFullscreen());
    }

    void Start()
    {
        // Populate resolution and quality dropdowns
        Resolution[] AllResolutions = Screen.resolutions;
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
        resolution.AddOptions(resolutionStringList);

        // Give the populated list to the settingsManager so it knows what resolutions are available
        if (settingsManager.Instance != null)
        {
            settingsManager.Instance.Resolutions = SelectedResolutionList;
            settingsManager.Instance.InitializeScreenSettings();
        }
        
        if (resolution) resolution.SetValueWithoutNotify(PlayerPrefs.GetInt("resolution", resolution.value));

        // AllQualitynames = QualitySettings.names;
        // foreach (string qual in AllQualitynames)
        // {
        //     SelectedQualityList.Add(qual);
        // }
        //quality.AddOptions(SelectedQualityList);
        //if (quality) quality.SetValueWithoutNotify(PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel()));
        
    }

    void OnEnable()
    {
        BindListeners();
    }

    void OnDisable()
    {
        UnbindListeners();
    }

    private void BindListeners()
    {
        var sm = settingsManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SettingsUIBinder] No settingsManager.Instance found at runtime.");
            return;
        }

        if (brightness) brightness.onValueChanged.AddListener(sm.ChangeBrightness);
        if (sound) sound.onValueChanged.AddListener(sm.ChangeSound);
        //if (quality) quality.onValueChanged.AddListener(sm.ChangeQuality);
        if (resolution) resolution.onValueChanged.AddListener(sm.ChangeResolution);
        if (fullscreen) fullscreen.onValueChanged.AddListener((bool isOn) => sm.ChangeFullScreen(isOn));

        Debug.Log($"[Binder] Binding to settingsManager id={settingsManager.Instance.GetInstanceID()} mixer={(settingsManager.Instance ? settingsManager.Instance.name : "NULL")}");

    }

    private void UnbindListeners()
    {
        var sm = settingsManager.Instance;
        if (sm == null) return; // Guard clause
        
        if (brightness) brightness.onValueChanged.RemoveListener(sm.ChangeBrightness);
        if (sound) sound.onValueChanged.RemoveListener(sm.ChangeSound);
        //if (quality) quality.onValueChanged.RemoveAllListeners();
        if (resolution) resolution.onValueChanged.RemoveListener(sm.ChangeResolution);
        if (fullscreen) fullscreen.onValueChanged.RemoveListener((bool isOn) => sm.ChangeFullScreen(isOn));
    }

    bool GetSavedFullscreen()
    {
        var raw = PlayerPrefs.GetString("togglefullscreen", bool.TrueString);
        return bool.TryParse(raw, out var val) ? val : true;
    }
}