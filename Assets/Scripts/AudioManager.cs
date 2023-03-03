using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField, Range(0, 1)] private float intensity;
    [SerializeField, BankRef] private string mainBank;
    [SerializeField] private EventReference bgmEventReference;
    [SerializeField] private BoolContainer isFMODReady;

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
            bgmEventInstance.setParameterByID(intensityParameter, intensity);
        }
    }

    private void OnFMODInitialized()
    {
        EventDescription eventDescription = RuntimeManager.GetEventDescription(bgmEventReference);

        FMOD.Studio.PARAMETER_DESCRIPTION param;
        eventDescription.getParameterDescriptionByName("Intensity", out param);
        intensityParameter = param.id;

        eventDescription.createInstance(out bgmEventInstance);

        bgmEventInstance.setParameterByID(intensityParameter, intensity);
        bgmEventInstance.start();
    }

    private IEnumerator InitializeFMOD(Action onInitialized)
    {
        // Iterate all the Studio Banks and start them loading in the background
        // including the audio sample data
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
