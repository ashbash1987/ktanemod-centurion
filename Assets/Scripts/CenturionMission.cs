using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class CenturionMission : MonoBehaviour
{
    static CenturionMission()
    {
        _gameplayStateType = ReflectionHelper.FindType("GameplayState");
        if (_gameplayStateType != null)
        {
            _missionField = _gameplayStateType.GetField("Mission", BindingFlags.Public | BindingFlags.Instance);
        }

        _missionType = ReflectionHelper.FindType("Assets.Scripts.Missions.Mission");
        if (_missionType != null)
        {
            _idProperty = _missionType.GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);
        }
    }

    private const string MISSION_ID = "mod_Centurion_CenturionMission";

    private static Type _gameplayStateType = null;
    private static FieldInfo _missionField = null;

    private static Type _missionType = null;
    private static PropertyInfo _idProperty = null;

    public AudioClip fanfareClip = null;

    private KMBombInfo _bombInfo = null;
    private AudioSource _fmnTension = null;
    private Coroutine _fmnChecker = null;
    private float _oldMusicVolume = 0.0f;

    private bool InCenturionMission
    {
        get
        {
            object gameplayStateObject = FindObjectOfType(_gameplayStateType);
            if (gameplayStateObject == null)
            {
                return false;
            }

            object mission = _missionField.GetValue(gameplayStateObject);
            if (mission == null)
            {
                return false;
            }

            string missionID = (string)_idProperty.GetValue(mission, null);
            if (missionID != null)
            {
                return missionID.Equals(MISSION_ID, StringComparison.InvariantCulture);
            }

            return false;
        }
    }

    private void Start()
    {
        if (!InCenturionMission)
        {
            return;
        }

        _bombInfo = GetComponent<KMBombInfo>();
        _bombInfo.OnBombSolved += OnBombSolved;
        _bombInfo.OnBombExploded += OnBombExploded;

        _fmnTension = GetComponent<AudioSource>();
        _fmnChecker = StartCoroutine(CheckForFMN());
    }

    private void OnDestroy()
    {
        if (_bombInfo != null)
        {
            _bombInfo.OnBombSolved -= OnBombSolved;
            _bombInfo.OnBombExploded -= OnBombExploded;
        }
    }

    private IEnumerator CheckForFMN()
    {
        while (true)
        {
            if (_bombInfo.GetSolvedModuleNames().Count == _bombInfo.GetSolvableModuleNames().Count - 1)
            {
                OnFMNSolveBegin();
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnFMNSolveBegin()
    {
        _oldMusicVolume = CenturionEnd.GameMusicVolume;
        CenturionEnd.GameMusicVolume = 0.0f;

        _fmnTension.volume = CenturionEnd.GameMusicVolume * 0.75f;
        _fmnTension.Play();
    }

    private void OnBombSolved()
    {
        if (_fmnTension.isPlaying)
        {
            _fmnTension.Stop();
        }

        CenturionEnd.GameMusicVolume = _oldMusicVolume;

        CenturionEnd.GetInstance(fanfareClip).gameObject.SetActive(true);
    }

    private void OnBombExploded()
    {
        if (_fmnChecker != null)
        {
            StopCoroutine(_fmnChecker);
            _fmnChecker = null;
        }

        if (_fmnTension.isPlaying)
        {
            _fmnTension.Stop();
        }

        CenturionEnd.GameMusicVolume = _oldMusicVolume;
    }
}
