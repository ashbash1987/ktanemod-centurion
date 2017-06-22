using System.Collections;
using System.Reflection;
using System;
using UnityEngine;

public class CenturionEnd : MonoBehaviour
{
    static CenturionEnd()
    {
        _playerSettingsManagerType = ReflectionHelper.FindType("Assets.Scripts.Settings.PlayerSettingsManager");
        if (_playerSettingsManagerType != null)
        {
            _playerSettingsInstanceProperty = _playerSettingsManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            _playerSettingsProperty = _playerSettingsManagerType.GetProperty("PlayerSettings", BindingFlags.Public | BindingFlags.Instance);
        }

        _playerSettingsType = ReflectionHelper.FindType("Assets.Scripts.Settings.PlayerSettings");
        if (_playerSettingsType != null)
        {
            _musicVolumeField = _playerSettingsType.GetField("MusicVolume", BindingFlags.Public | BindingFlags.Instance);
            _sfxVolumeField = _playerSettingsType.GetField("SFXVolume", BindingFlags.Public | BindingFlags.Instance);
        }

        _musicManagerType = ReflectionHelper.FindType("MusicManager");
        if (_musicManagerType != null)
        {
            _musicManagerInstanceField = _musicManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            _setVolumeMethod = _musicManagerType.GetMethod("SetVolume", BindingFlags.Public | BindingFlags.Instance);
        }

        _masterAudioType = ReflectionHelper.FindType("DarkTonic.MasterAudio.MasterAudio");
        if (_masterAudioType != null)
        {
            _masterVolumeLevelProperty = _masterAudioType.GetProperty("MasterVolumeLevel", BindingFlags.Public | BindingFlags.Static);
        }
    }

    public static float GameMusicVolume
    {
        get
        {
            object playerSettingsManager = _playerSettingsInstanceProperty.GetValue(null, null);
            if (playerSettingsManager == null)
            {
                return 1.0f;
            }

            object playerSettings = _playerSettingsProperty.GetValue(playerSettingsManager, null);
            if (playerSettings == null)
            {
                return 1.0f;
            }

            int musicVolume = (int)_musicVolumeField.GetValue(playerSettings);

            return musicVolume / 100.0f;
        }
        set
        {
            object musicManager = _musicManagerInstanceField.GetValue(null);
            if (musicManager == null)
            {
                return;
            }

            _setVolumeMethod.Invoke(musicManager, new object[] { value, true });
        }
    }

    public static float GameSFXVolume
    {
        get
        {
            object playerSettingsManager = _playerSettingsInstanceProperty.GetValue(null, null);
            if (playerSettingsManager == null)
            {
                return 1.0f;
            }

            object playerSettings = _playerSettingsProperty.GetValue(playerSettingsManager, null);
            if (playerSettings == null)
            {
                return 1.0f;
            }

            int sfxVolume = (int)_sfxVolumeField.GetValue(playerSettings);

            return sfxVolume / 100.0f;
        }
        set
        {
            _masterVolumeLevelProperty.SetValue(null, value, null);
        }
    }

    private static CenturionEnd _instance = null;

    private static Type _playerSettingsManagerType = null;
    private static PropertyInfo _playerSettingsInstanceProperty = null;
    private static PropertyInfo _playerSettingsProperty = null;

    private static Type _playerSettingsType = null;
    private static FieldInfo _musicVolumeField = null;
    private static FieldInfo _sfxVolumeField = null;

    private static Type _musicManagerType = null;
    private static FieldInfo _musicManagerInstanceField = null;
    private static MethodInfo _setVolumeMethod = null;

    private static Type _masterAudioType = null;
    private static PropertyInfo _masterVolumeLevelProperty = null;

    private AudioSource _fanfare = null;
    private Coroutine _sequence = null;

    private void Awake()
    {
        _fanfare = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _sequence = StartCoroutine(EndSequence());
    }

    private void OnDisable()
    {
        if (_sequence != null)
        {
            StopCoroutine(_sequence);
            _sequence = null;
        }
    }

    public static CenturionEnd GetInstance(AudioClip fanfareClip)
    {
        if (_instance != null)
        {
            return _instance;
        }

        GameObject newGO = new GameObject("TheEnd");
        DontDestroyOnLoad(newGO);

        AudioSource audioSource = newGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = fanfareClip;

        _instance = newGO.AddComponent<CenturionEnd>();

        return _instance;
    }

    private IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(0.999f);

        float oldSFXVolume = GameSFXVolume;
        float oldMusicVolume = GameMusicVolume;

        GameSFXVolume = 0.0f;
        GameMusicVolume = 0.0f;

        _fanfare.volume = GameMusicVolume;
        _fanfare.Play();

        while (_fanfare.isPlaying)
        {
            yield return null;
        }

        GameSFXVolume = oldSFXVolume;
        GameMusicVolume = oldMusicVolume;
    }
}
