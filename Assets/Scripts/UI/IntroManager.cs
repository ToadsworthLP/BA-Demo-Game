using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Logger logger;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private BoolContainer isFMODReady;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject continueSymbol;
    [SerializeField] private CanvasGroup instructionsCanvasGroup;
    [SerializeField] private float initialDelay;
    [SerializeField] private EventReference clickSound;
    [SerializeField] private float fadeTime;
    [SerializeField] private string nextScenenName;
    [SerializeField] private BoolContainer hasSeenTutorial;

    private PlayerInput input;

    private float startTime;
    private float currentFadeTime;

    private void Start()
    {
        input = new PlayerInput();
        input.Main.Enable();

        bool isInControlGroup = Random.Range(0, 2) == 0;

        audioManager.SetAdaptiveMusicEnabled(!isInControlGroup);

        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.GAME_START, isInControlGroup ? "B" : "A"));

        StartCoroutine(IntroCoroutine());

        hasSeenTutorial.Value = false;
    }

    private IEnumerator IntroCoroutine()
    {
        startTime = Time.time + initialDelay;

        while (startTime > Time.time)
        {
            yield return null;
        }

        if (!isFMODReady)
        {
            loadingText.SetActive(true);

            while (!isFMODReady) yield return null;

            loadingText.SetActive(false);
        }

        continueSymbol.SetActive(true);

        while (!input.Main.ContinueTutorial.IsPressed())
        {
            yield return null;
        }

        RuntimeManager.PlayOneShot(clickSound);

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScenenName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (currentFadeTime < fadeTime)
        {
            instructionsCanvasGroup.alpha = Mathf.Clamp01(1 - (currentFadeTime / fadeTime));
            currentFadeTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        instructionsCanvasGroup.alpha = 0.0f;
        currentFadeTime = 0f;

        op.allowSceneActivation = true;
    }
}
