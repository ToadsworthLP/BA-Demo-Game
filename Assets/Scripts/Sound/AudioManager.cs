using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("General")]
    [SerializeField] private bool adaptiveMusicEnabled;
    [SerializeField, BankRef] private string mainBank;
    [SerializeField] private BoolContainer isFMODReady;
    [SerializeField] private AdaptiveSoundTriggerRegistry triggerRegistry;
    [SerializeField] private AdaptiveSoundOverrides overrides;

    [Header("BGM")]
    [SerializeField] private EventReference bgmEventReference;
    [SerializeField] private float stageClearBgmFadeSpeed;
    [SerializeField] private string intensityParameterName;
    [SerializeField] private ScriptableObjectEvent clearStageEvent;
    [SerializeField] private ScriptableObjectEvent startStageEvent;

    [Header("Waterfall SFX")]
    [SerializeField] private EventReference waterfallSfxEventReference;
    [SerializeField] private float waterfallSfxFadeSpeed;
    [SerializeField] private ChangeWaterLevelEvent changeTargetWaterLevelEvent;
    [SerializeField] private FloatContainer currentWaterLevel;

    private EventInstance bgmEventInstance;
    private EventInstance waterfallEventInstance;
    private PARAMETER_ID intensityParameter;

    private float initialBgmVolume;
    private float currentBgmVolume;
    private float targetBgmVolume;

    private float targetWaterLevel;
    private float initialWaterfallVolume;
    private float currentWaterfallVolume;
    private float targetWaterfallVolume;

    public void SetAdaptiveMusicEnabled(bool value)
    {
        adaptiveMusicEnabled = value;
    }

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
            clearStageEvent.Unsubscribe(OnStageCleared);
            startStageEvent.Unsubscribe(OnStageStart);
            changeTargetWaterLevelEvent.Unsubscribe(OnWaterLevelChanged);

            bgmEventInstance.release();
            bgmEventInstance.clearHandle();

            waterfallEventInstance.release();
            waterfallEventInstance.clearHandle();

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

            if (targetWaterLevel == currentWaterLevel)
            {
                targetWaterfallVolume = 0;
            }

            if (currentWaterfallVolume != targetWaterfallVolume)
            {
                if (Mathf.Abs(currentWaterfallVolume - targetWaterfallVolume) < 0.01f)
                {
                    targetWaterfallVolume = currentWaterfallVolume;
                }
                else
                {
                    currentWaterfallVolume = Mathf.Lerp(currentWaterfallVolume, targetWaterfallVolume, waterfallSfxFadeSpeed * Time.deltaTime);
                }

                waterfallEventInstance.setVolume(currentWaterfallVolume);
            }
        }
    }

    private void OnFMODInitialized()
    {
        EventDescription bgmEventDescription = RuntimeManager.GetEventDescription(bgmEventReference);

        PARAMETER_DESCRIPTION param;
        bgmEventDescription.getParameterDescriptionByName(intensityParameterName, out param);
        intensityParameter = param.id;

        bgmEventDescription.createInstance(out bgmEventInstance);

        bgmEventInstance.setParameterByID(intensityParameter, 0f);
        bgmEventInstance.start();

        bgmEventInstance.getVolume(out currentBgmVolume);
        initialBgmVolume = currentBgmVolume;
        targetBgmVolume = currentBgmVolume;

        clearStageEvent.Subscribe(OnStageCleared);
        startStageEvent.Subscribe(OnStageStart);

        EventDescription waterfallEventDescription = RuntimeManager.GetEventDescription(waterfallSfxEventReference);
        waterfallEventDescription.createInstance(out waterfallEventInstance);
        waterfallEventInstance.start();

        waterfallEventInstance.getVolume(out initialWaterfallVolume);
        waterfallEventInstance.setVolume(currentWaterfallVolume);

        changeTargetWaterLevelEvent.Subscribe(OnWaterLevelChanged);
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
        targetWaterLevel = currentWaterLevel;
    }

    private void OnWaterLevelChanged(ChangeWaterLevelEventArgs args)
    {
        targetWaterfallVolume = initialWaterfallVolume;
        targetWaterLevel = args.targetLevel;
    }
}
