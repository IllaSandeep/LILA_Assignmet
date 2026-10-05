using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Settings artwork and its interactive controls in both the Main Menu
/// and the in-run pause menu.
/// </summary>
public sealed class SettingsPanelController : MonoBehaviour
{
    private const string MusicKey = "SETTINGS_MUSIC_VOLUME";
    private const string SfxKey = "SETTINGS_SFX_VOLUME";
    private const string VibrationKey = "SETTINGS_VIBRATION_ENABLED";
    private const float DefaultVolume = 0.64f;

    private static readonly Dictionary<AudioSource, float> MusicSources = new Dictionary<AudioSource, float>();
    private static readonly Dictionary<AudioSource, float> SfxSources = new Dictionary<AudioSource, float>();
    private static bool settingsLoaded;
    private static float musicVolume = DefaultVolume;
    private static float sfxVolume = DefaultVolume;
    private static bool vibrationEnabled = true;

    [Header("Scene Return Targets")]
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private MenuUIController menuController;
    [SerializeField] private TMP_FontAsset uiFont;

    [Header("Controls")]
    [SerializeField] private bool buildControlsAtRuntime = true;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button vibrationButton;
    [SerializeField] private Button resetSettingsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI vibrationLabel;
    [SerializeField] private RectTransform vibrationKnob;

    private bool controlsBuilt;
    private bool listenersBound;

    public static float MusicVolume
    {
        get { LoadSettings(); return musicVolume; }
    }

    public static float SfxVolume
    {
        get { LoadSettings(); return sfxVolume; }
    }

    public static bool VibrationEnabled
    {
        get { LoadSettings(); return vibrationEnabled; }
    }

    private void OnEnable()
    {
        LoadSettings();
        if (buildControlsAtRuntime && !controlsBuilt)
            BuildControls();

        if (musicSlider == null || sfxSlider == null || vibrationButton == null ||
            resetSettingsButton == null || backButton == null || closeButton == null ||
            vibrationLabel == null || vibrationKnob == null)
        {
            Debug.LogError("SettingsPanelController is missing one or more Settings controls.", this);
            return;
        }

        BindControls();

        musicSlider.SetValueWithoutNotify(musicVolume);
        sfxSlider.SetValueWithoutNotify(sfxVolume);
        UpdateVibrationVisual();
        ApplyVolumesToRegisteredSources();
    }

    private void BuildControls()
    {
        controlsBuilt = true;

        // These coordinates match the 1920 x 1080 Canvas and the supplied 16:9 art.
        musicSlider = CreateVolumeSlider("MusicVolumeSlider", 223f, 219f);
        sfxSlider = CreateVolumeSlider("SfxVolumeSlider", 223f, 56f);

        CreateVibrationToggle();
        resetSettingsButton = CreateOverlayButton("ResetSettingsButton", new Vector2(0f, -248f), new Vector2(490f, 92f));
        backButton = CreateOverlayButton("BackButton", new Vector2(0f, -382f), new Vector2(710f, 122f));
        closeButton = CreateOverlayButton("CloseButton", new Vector2(770f, 382f), new Vector2(154f, 154f));
    }

    private void BindControls()
    {
        if (listenersBound)
            return;

        listenersBound = true;
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        vibrationButton.onClick.AddListener(ToggleVibration);
        resetSettingsButton.onClick.AddListener(ResetSettings);
        backButton.onClick.AddListener(CloseSettings);
        closeButton.onClick.AddListener(CloseSettings);
    }

    private Slider CreateVolumeSlider(string objectName, float x, float y)
    {
        GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Slider));
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        SetCenteredRect(sliderRect, transform, new Vector2(x, y), new Vector2(632f, 48f));

        Image backing = sliderObject.GetComponent<Image>();
        backing.color = new Color(0.008f, 0.055f, 0.095f, 0.98f);
        backing.raycastTarget = true;

        GameObject trackObject = new GameObject("Track", typeof(RectTransform), typeof(Image));
        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackRect.SetParent(sliderRect, false);
        trackRect.anchorMin = new Vector2(0.045f, 0.5f);
        trackRect.anchorMax = new Vector2(0.955f, 0.5f);
        trackRect.sizeDelta = new Vector2(0f, 10f);
        trackRect.anchoredPosition = Vector2.zero;
        Image track = trackObject.GetComponent<Image>();
        track.color = new Color(0.015f, 0.12f, 0.19f, 1f);
        track.raycastTarget = false;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.SetParent(trackRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fill = fillObject.GetComponent<Image>();
        fill.color = new Color(0.06f, 0.84f, 1f, 1f);
        fill.raycastTarget = false;

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.SetParent(sliderRect, false);
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(38f, 38f);
        Image handle = handleObject.GetComponent<Image>();
        handle.sprite = GetCircleSprite();
        handle.color = new Color(0.58f, 0.97f, 1f, 1f);
        handle.raycastTarget = true;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.transition = Selectable.Transition.None;
        slider.targetGraphic = handle;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = DefaultVolume;
        return slider;
    }

    private void CreateVibrationToggle()
    {
        GameObject toggleObject = new GameObject("VibrationToggle", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
        SetCenteredRect(toggleRect, transform, new Vector2(500f, -112f), new Vector2(220f, 74f));

        Image background = toggleObject.GetComponent<Image>();
        background.color = new Color(0.015f, 0.075f, 0.13f, 1f);
        background.raycastTarget = true;
        Button button = toggleObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        vibrationButton = button;

        Outline outline = toggleObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.72f, 1f, 1f);
        outline.effectDistance = new Vector2(3f, 3f);

        GameObject labelObject = new GameObject("State", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(toggleRect, false);
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(5f, 0f);
        labelRect.offsetMax = new Vector2(-36f, 0f);
        vibrationLabel = labelObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(vibrationLabel, 29f, TextAlignmentOptions.Center);
        vibrationLabel.raycastTarget = false;

        GameObject knobObject = new GameObject("Knob", typeof(RectTransform), typeof(Image));
        vibrationKnob = knobObject.GetComponent<RectTransform>();
        vibrationKnob.SetParent(toggleRect, false);
        vibrationKnob.anchorMin = new Vector2(0.5f, 0.5f);
        vibrationKnob.anchorMax = new Vector2(0.5f, 0.5f);
        vibrationKnob.sizeDelta = new Vector2(48f, 48f);
        Image knob = knobObject.GetComponent<Image>();
        knob.sprite = GetCircleSprite();
        knob.color = new Color(0.12f, 0.93f, 1f, 1f);
        knob.raycastTarget = false;
    }

    private Button CreateOverlayButton(string objectName, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, transform, position, size);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }

    private void SetCenteredRect(RectTransform rect, Transform parent, Vector2 position, Vector2 size)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void ConfigureText(TextMeshProUGUI text, float size, TextAlignmentOptions alignment)
    {
        if (uiFont != null)
            text.font = uiFont;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.enableAutoSizing = false;
        text.text = "ON";
    }

    private void UpdateVibrationVisual()
    {
        vibrationLabel.text = vibrationEnabled ? "ON" : "OFF";
        vibrationKnob.anchoredPosition = vibrationEnabled ? new Vector2(72f, 0f) : new Vector2(-72f, 0f);
    }

    private void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.Save();
        ApplyVolumesToSources(MusicSources, musicVolume);
    }

    private void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.Save();
        ApplyVolumesToSources(SfxSources, sfxVolume);
    }

    private void ToggleVibration()
    {
        vibrationEnabled = !vibrationEnabled;
        PlayerPrefs.SetInt(VibrationKey, vibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateVibrationVisual();
    }

    private void ResetSettings()
    {
        musicSlider.SetValueWithoutNotify(DefaultVolume);
        sfxSlider.SetValueWithoutNotify(DefaultVolume);
        musicVolume = DefaultVolume;
        sfxVolume = DefaultVolume;
        vibrationEnabled = true;

        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.SetInt(VibrationKey, 1);
        PlayerPrefs.Save();
        UpdateVibrationVisual();
        ApplyVolumesToRegisteredSources();
    }

    private void CloseSettings()
    {
        if (pauseManager != null)
        {
            pauseManager.CloseSettings();
            return;
        }

        if (menuController != null)
            menuController.ShowMainMenu();
        else
            gameObject.SetActive(false);
    }

    private static void LoadSettings()
    {
        if (settingsLoaded)
            return;

        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, DefaultVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, DefaultVolume));
        vibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) != 0;
        settingsLoaded = true;
    }

    private static void ApplyVolumesToRegisteredSources()
    {
        ApplyVolumesToSources(MusicSources, musicVolume);
        ApplyVolumesToSources(SfxSources, sfxVolume);
    }

    private static void ApplyVolumesToSources(Dictionary<AudioSource, float> sources, float volume)
    {
        List<AudioSource> destroyedSources = null;
        foreach (KeyValuePair<AudioSource, float> item in sources)
        {
            if (item.Key == null)
            {
                if (destroyedSources == null)
                    destroyedSources = new List<AudioSource>();
                destroyedSources.Add(item.Key);
                continue;
            }

            item.Key.volume = Mathf.Clamp01(item.Value * volume);
        }

        if (destroyedSources != null)
        {
            foreach (AudioSource source in destroyedSources)
                sources.Remove(source);
        }
    }

    /// <summary>Register a music AudioSource so its volume follows the Music slider.</summary>
    public static void RegisterMusicSource(AudioSource source) => RegisterSource(source, MusicSources, MusicVolume);

    /// <summary>Register an effects AudioSource so its volume follows the SFX slider.</summary>
    public static void RegisterSfxSource(AudioSource source) => RegisterSource(source, SfxSources, SfxVolume);

    public static void UnregisterMusicSource(AudioSource source) => UnregisterSource(source, MusicSources);
    public static void UnregisterSfxSource(AudioSource source) => UnregisterSource(source, SfxSources);

    /// <summary>Call this when the game triggers haptics; it respects the saved toggle.</summary>
    public static void VibrateIfEnabled()
    {
        if (VibrationEnabled && Application.isMobilePlatform)
            Handheld.Vibrate();
    }

    private static void RegisterSource(AudioSource source, Dictionary<AudioSource, float> sources, float volume)
    {
        if (source == null)
            return;

        if (!sources.ContainsKey(source))
            sources.Add(source, source.volume);
        source.volume = Mathf.Clamp01(sources[source] * volume);
    }

    private static void UnregisterSource(AudioSource source, Dictionary<AudioSource, float> sources)
    {
        if (source != null && sources.TryGetValue(source, out float originalVolume))
        {
            source.volume = originalVolume;
            sources.Remove(source);
        }
    }

    private static Sprite circleSprite;

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "SettingsControlCircle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.44f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        circleSprite.name = "SettingsControlCircle";
        return circleSprite;
    }
}
