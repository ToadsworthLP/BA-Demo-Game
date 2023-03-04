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
    [SerializeField] private EventReference bgmEventReference;
    [SerializeField] private BoolContainer isFMODReady;
    [SerializeField] private AdaptiveSoundTriggerRegistry triggerRegistry;
    [SerializeField] private AdaptiveSoundOverrides overrides;

    private EventInstance bgmEventInstance;
    private PARAMETER_ID intensityParameter;

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
        bgmEventInstance.release();
        bgmEventInstance.clearHandle();
    }

    private void Update()
    {
        if (isFMODReady)
        {
            if (adaptiveMusicEnabled)
            {
                triggerRegistry.Update();
                float currentIntensity = (triggerRegistry.ReadValue01() * overrides.intensityMultiplier) + overrides.intensityOffset;
                bgmEventInstance.setParameterByID(intensityParameter, currentIntensity);
            }
            else
            {
                bgmEventInstance.setParameterByID(intensityParameter, overrides.defaultIntensity);
            }
        }
    }

    private void OnFMODInitialized()
    {
        EventDescription eventDescription = RuntimeManager.GetEventDescription(bgmEventReference);

        PARAMETER_DESCRIPTION param;
        eventDescription.getParameterDescriptionByName("Intensity", out param);
        intensityParameter = param.id;

        eventDescription.createInstance(out bgmEventInstance);

        bgmEventInstance.setParameterByID(intensityParameter, 0f);
        bgmEventInstance.start();
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
}
