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

    private UnityEngine.Events.UnityAction<float> _onBrightness;
    private UnityEngine.Events.UnityAction<float> _onSound;
    private UnityEngine.Events.UnityAction<int>   _onResolution;
    private UnityEngine.Events.UnityAction<bool>  _onFullscreen;

    Resolution[] AllResolutions;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    int SelectedQuality;
    List<string> SelectedQualityList = new List<string>();
    string[] AllQualitynames;

    void Awake()
    {
        // Initialize UI from saved prefs (without firing callbacks)
        if (brightness) brightness.SetValueWithoutNotify(PlayerPrefs.GetFloat("brightness", -2f));
        if (sound) sound.SetValueWithoutNotify(PlayerPrefs.GetFloat("sound", 1.0f));
        if (fullscreen) fullscreen.SetIsOnWithoutNotify(GetSavedFullscreen());
    }

    void Start()
    {
        // // Populate resolution and quality dropdowns
        // Resolution[] AllResolutions = Screen.resolutions;
        // List<string> resolutionStringList = new List<string>();
        // string newRes;

        if (resolution) resolution.ClearOptions();
        AllResolutions = Screen.resolutions;  
        List<string> resolutionStringList = new List<string>();
        SelectedResolutionList.Clear();

        foreach (Resolution res in AllResolutions)
        {
            string newRes = res.width + " x " + res.height;
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(res);
            }
        }
        if (resolution) resolution.AddOptions(resolutionStringList);

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

        // if (brightness) brightness.onValueChanged.AddListener(sm.ChangeBrightness);
        // if (sound) sound.onValueChanged.AddListener(sm.ChangeSound);
        // //if (quality) quality.onValueChanged.AddListener(sm.ChangeQuality);
        // if (resolution) resolution.onValueChanged.AddListener(sm.ChangeResolution);
        // if (fullscreen) fullscreen.onValueChanged.AddListener((bool isOn) => sm.ChangeFullScreen(isOn));

        // Debug.Log($"[Binder] Binding to settingsManager id={settingsManager.Instance.GetInstanceID()} mixer={(settingsManager.Instance ? settingsManager.Instance.name : "NULL")}");

        // Build delegates once
        _onBrightness = v => sm.ChangeBrightness(v);
        _onSound      = v => sm.ChangeSound(v);
        _onResolution = i => sm.ChangeResolution(i);
        _onFullscreen = b => sm.ChangeFullScreen(b);

        if (brightness)  brightness.onValueChanged.AddListener(_onBrightness);
        if (sound)       sound.onValueChanged.AddListener(_onSound);
        if (resolution)  resolution.onValueChanged.AddListener(_onResolution);
        if (fullscreen)  fullscreen.onValueChanged.AddListener(_onFullscreen);

        Debug.Log($"[Binder] Binding to settingsManager id={settingsManager.Instance.GetInstanceID()} mixer={(settingsManager.Instance ? settingsManager.Instance.name : "NULL")}");

    }

    private void UnbindListeners()
    {
        // var sm = settingsManager.Instance;
        // if (sm == null) return; // Guard clause

        // if (brightness) brightness.onValueChanged.RemoveListener(sm.ChangeBrightness);
        // if (sound) sound.onValueChanged.RemoveListener(sm.ChangeSound);
        // //if (quality) quality.onValueChanged.RemoveAllListeners();
        // if (resolution) resolution.onValueChanged.RemoveListener(sm.ChangeResolution);
        // if (fullscreen) fullscreen.onValueChanged.RemoveListener((bool isOn) => sm.ChangeFullScreen(isOn));
        
        if (brightness && _onBrightness != null)  brightness.onValueChanged.RemoveListener(_onBrightness);
        if (sound && _onSound != null)            sound.onValueChanged.RemoveListener(_onSound);
        if (resolution && _onResolution != null)  resolution.onValueChanged.RemoveListener(_onResolution);
        if (fullscreen && _onFullscreen != null)  fullscreen.onValueChanged.RemoveListener(_onFullscreen);
    }

    bool GetSavedFullscreen()
    {
        var raw = PlayerPrefs.GetString("togglefullscreen", bool.TrueString);
        return bool.TryParse(raw, out var val) ? val : true;
    }
}