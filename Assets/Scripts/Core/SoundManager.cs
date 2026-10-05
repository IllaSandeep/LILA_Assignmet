using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent, scene-wide audio playback for music, gameplay effects, and UI clicks.
/// Place this on the persistent MainMenu GameManager object.
/// </summary>
public sealed class SoundManager : MonoBehaviour
{
    private const string MusicVolumeKey = "SETTINGS_MUSIC_VOLUME";
    private const string SfxVolumeKey = "SETTINGS_SFX_VOLUME";
    private const float DefaultVolume = 0.64f;

    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField, Range(0f, 1f)] private float musicVolume = DefaultVolume;

    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip playerDamageClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip nemesisWarningClip;
    [SerializeField] private AudioClip nemesisSpawnClip;
    [SerializeField] private AudioClip heavyImpactClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = DefaultVolume;

    [Header("UI SFX")]
    [SerializeField] private AudioClip uiClickClip;

    [Header("SFX Cooldowns")]
    [SerializeField, Min(0f)] private float playerShootCooldown = 0.1f;
    [SerializeField, Min(0f)] private float enemyHitCooldown = 0.06f;
    [SerializeField, Min(0f)] private float enemyDeathCooldown = 0.06f;
    [SerializeField, Min(0f)] private float playerDamageCooldown = 0.15f;
    [SerializeField, Min(0f)] private float heavyImpactCooldown = 0.08f;
    [SerializeField, Min(0f)] private float uiClickCooldown = 0.08f;

    private readonly HashSet<Button> hookedButtons = new HashSet<Button>();
    private readonly HashSet<Slider> hookedSliders = new HashSet<Slider>();
    private readonly HashSet<Toggle> hookedToggles = new HashSet<Toggle>();

    private float nextPlayerShootTime;
    private float nextEnemyHitTime;
    private float nextEnemyDeathTime;
    private float nextPlayerDamageTime;
    private float nextHeavyImpactTime;
    private float nextUiClickTime;
    private float nextPowerUpTime;
    private float nextLevelUpTime;
    private float nextNemesisWarningTime;
    private float nextNemesisSpawnTime;
    private bool gameplayAudioSuppressed;
    private bool gameOverPlayed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveAudioSources();
        LoadSavedVolumes();
        ConfigureAudioSources();
        PlayBackgroundMusicIfNeeded();
        WarnForMissingClips();

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this)
            return;

        HookUiControlsInScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        Instance = null;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void PlayPlayerShoot()
    {
        PlayGameplaySfx(playerShootClip, playerShootCooldown, ref nextPlayerShootTime);
    }

    public void PlayPlayerDamage()
    {
        PlayGameplaySfx(playerDamageClip, playerDamageCooldown, ref nextPlayerDamageTime);
    }

    public void PlayEnemyHit()
    {
        PlayGameplaySfx(enemyHitClip, enemyHitCooldown, ref nextEnemyHitTime);
    }

    public void PlayEnemyDeath()
    {
        PlayGameplaySfx(enemyDeathClip, enemyDeathCooldown, ref nextEnemyDeathTime);
    }

    public void PlayPowerUp()
    {
        PlayGameplaySfx(powerUpClip, 0.05f, ref nextPowerUpTime);
    }

    public void PlayLevelUp()
    {
        PlayGameplaySfx(levelUpClip, 0.1f, ref nextLevelUpTime);
    }

    public void PlayNemesisWarning()
    {
        PlayGameplaySfx(nemesisWarningClip, 0.5f, ref nextNemesisWarningTime);
    }

    public void PlayNemesisSpawn()
    {
        PlayGameplaySfx(nemesisSpawnClip, 0.5f, ref nextNemesisSpawnTime);
    }

    public void PlayHeavyImpact()
    {
        PlayGameplaySfx(heavyImpactClip, heavyImpactCooldown, ref nextHeavyImpactTime);
    }

    public void PlayUIClick()
    {
        if (sfxSource == null || uiClickClip == null)
            return;

        float now = Time.unscaledTime;
        if (now < nextUiClickTime)
            return;

        nextUiClickTime = now + uiClickCooldown;
        sfxSource.PlayOneShot(uiClickClip);
    }

    /// <summary>
    /// Suppresses subsequent gameplay effects when the existing run-end flow fires.
    /// UI clicks remain available for restart and menu buttons.
    /// </summary>
    public void PlayGameOver()
    {
        if (gameOverPlayed)
            return;

        gameOverPlayed = true;
        gameplayAudioSuppressed = true;

        if (sfxSource != null && gameOverClip != null)
            sfxSource.PlayOneShot(gameOverClip);
    }

    /// <summary>Re-enables gameplay effects after a run restart or mock-ad revive.</summary>
    public void ResumeGameplayAudio()
    {
        gameOverPlayed = false;
        gameplayAudioSuppressed = false;
    }

    /// <summary>
    /// Adds one central click sound to interactive controls created at runtime,
    /// such as the Main Menu settings controls.
    /// </summary>
    public void HookUiControl(Selectable control)
    {
        if (control == null)
            return;

        Button button = control as Button;
        if (button != null && hookedButtons.Add(button))
            button.onClick.AddListener(PlayUIClick);

        Slider slider = control as Slider;
        if (slider != null && hookedSliders.Add(slider))
            slider.onValueChanged.AddListener(HandleSliderChanged);

        Toggle toggle = control as Toggle;
        if (toggle != null && hookedToggles.Add(toggle))
            toggle.onValueChanged.AddListener(HandleToggleChanged);
    }

    private void HandleSliderChanged(float _) => PlayUIClick();

    private void HandleToggleChanged(bool _) => PlayUIClick();

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hookedButtons.RemoveWhere(button => button == null);
        hookedSliders.RemoveWhere(slider => slider == null);
        hookedToggles.RemoveWhere(toggle => toggle == null);

        if (scene.name == "Game")
            ResumeGameplayAudio();

        HookUiControlsInScene(scene);
    }

    private void HookUiControlsInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Selectable[] controls = roots[i].GetComponentsInChildren<Selectable>(true);
            for (int j = 0; j < controls.Length; j++)
                HookUiControl(controls[j]);
        }
    }

    private void PlayGameplaySfx(AudioClip clip, float cooldown, ref float nextAllowedTime)
    {
        if (gameplayAudioSuppressed || sfxSource == null || clip == null)
            return;

        float now = Time.unscaledTime;
        if (now < nextAllowedTime)
            return;

        nextAllowedTime = now + cooldown;
        sfxSource.PlayOneShot(clip);
    }

    private void ResolveAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (musicSource == null && sources.Length > 0)
            musicSource = sources[0];
        if (sfxSource == null && sources.Length > 1)
            sfxSource = sources[1];

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void LoadSavedVolumes()
    {
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume));
    }

    private void ConfigureAudioSources()
    {
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
        musicSource.clip = backgroundMusicClip;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = sfxVolume;
    }

    private void PlayBackgroundMusicIfNeeded()
    {
        if (backgroundMusicClip == null || musicSource == null)
            return;

        if (musicSource.clip != backgroundMusicClip)
            musicSource.clip = backgroundMusicClip;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    private void WarnForMissingClips()
    {
        StringBuilder missing = new StringBuilder();
        AddMissingClip(missing, "background music", backgroundMusicClip);
        AddMissingClip(missing, "player shoot", playerShootClip);
        AddMissingClip(missing, "player damage", playerDamageClip);
        AddMissingClip(missing, "enemy hit", enemyHitClip);
        AddMissingClip(missing, "enemy death", enemyDeathClip);
        AddMissingClip(missing, "power-up", powerUpClip);
        AddMissingClip(missing, "level-up", levelUpClip);
        AddMissingClip(missing, "Nemesis warning", nemesisWarningClip);
        AddMissingClip(missing, "Nemesis spawn", nemesisSpawnClip);
        AddMissingClip(missing, "heavy impact", heavyImpactClip);
        AddMissingClip(missing, "UI click", uiClickClip);
        AddMissingClip(missing, "game over", gameOverClip);

        if (missing.Length > 0)
        {
            Debug.LogWarning(
                "SoundManager is missing audio clips for: " + missing +
                ". Assign available clips in the Inspector.",
                this
            );
        }
    }

    private static void AddMissingClip(StringBuilder missing, string name, AudioClip clip)
    {
        if (clip != null)
            return;

        if (missing.Length > 0)
            missing.Append(", ");
        missing.Append(name);
    }
}
