using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private bool adaptiveMusicEnabled;
    [SerializeField, BankRef] private string mainBank;
    [SerializeField] private string intensityParameterName;
    [SerializeField] private EventReference bgmEventReference;
    [SerializeField] private BoolContainer isFMODReady;
    [SerializeField] private AdaptiveSoundTriggerRegistry triggerRegistry;
    [SerializeField] private AdaptiveSoundOverrides overrides;
    [SerializeField] private float stageClearBgmFadeSpeed;
    [SerializeField] private ScriptableObjectEvent clearStageEvent;
    [SerializeField] private ScriptableObjectEvent startStageEvent;

    private EventInstance bgmEventInstance;
    private PARAMETER_ID intensityParameter;

    private float initialBgmVolume;
    private float currentBgmVolume;
    private float targetBgmVolume;

    private void Start()
    {
        if (Instance == null)
        {
            isFMODReady.Value = false;

            Instance = this;
            DontDestroyOnLoad(gameObject);

            StartCoroutine(InitializeFMOD(OnFMODInitialized));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            bgmEventInstance.release();
            bgmEventInstance.clearHandle();

            clearStageEvent.Unsubscribe(OnStageCleared);
            startStageEvent.Unsubscribe(OnStageStart);

            Instance = null;
        }
    }

    private void Update()
    {
        if (isFMODReady)
        {
            if (adaptiveMusicEnabled)
            {
                triggerRegistry.Update();
                float currentIntensity = (triggerRegistry.ReadValue01() * overrides.intensityMultiplier) + overrides.intensityOffset;
                currentIntensity = Mathf.Clamp01(currentIntensity);
                bgmEventInstance.setParameterByID(intensityParameter, currentIntensity);
            }
            else
            {
                bgmEventInstance.setParameterByID(intensityParameter, overrides.defaultIntensity);
            }

            if (currentBgmVolume != targetBgmVolume)
            {
                if (Mathf.Abs(currentBgmVolume - targetBgmVolume) < 0.01f)
                {
                    targetBgmVolume = currentBgmVolume;
                }
                else
                {
                    currentBgmVolume = Mathf.Lerp(currentBgmVolume, targetBgmVolume, stageClearBgmFadeSpeed * Time.deltaTime);
                }

                bgmEventInstance.setVolume(currentBgmVolume);
            }
        }
    }

    private void OnFMODInitialized()
    {
        EventDescription eventDescription = RuntimeManager.GetEventDescription(bgmEventReference);

        PARAMETER_DESCRIPTION param;
        eventDescription.getParameterDescriptionByName(intensityParameterName, out param);
        intensityParameter = param.id;

        eventDescription.createInstance(out bgmEventInstance);

        bgmEventInstance.setParameterByID(intensityParameter, 0f);
        bgmEventInstance.start();

        bgmEventInstance.getVolume(out currentBgmVolume);
        initialBgmVolume = currentBgmVolume;
        targetBgmVolume = currentBgmVolume;

        clearStageEvent.Subscribe(OnStageCleared);
        startStageEvent.Subscribe(OnStageStart);
    }

    private IEnumerator InitializeFMOD(Action onInitialized)
    {
        // Load banks in the background
        RuntimeManager.LoadBank(mainBank, true);

        // Keep yielding the co-routine until all the bank loading is done
        // (for platforms with asynchronous bank loading)
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Keep yielding the co-routine until all the sample data loading is done
        while (RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        onInitialized();

        isFMODReady.Value = true;
    }

    private void OnStageCleared()
    {
        targetBgmVolume = 0.001f;
    }

    private void OnStageStart()
    {
        bgmEventInstance.setParameterByID(intensityParameter, overrides.intensityOffset, true);
        targetBgmVolume = initialBgmVolume;
    }
}
