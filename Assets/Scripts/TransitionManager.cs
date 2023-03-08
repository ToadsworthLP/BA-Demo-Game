using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Events")]
    [SerializeField] private ScriptableObjectEvent tutorialEndEvent;
    [SerializeField] private ScriptableObjectEvent playerDeathEvent;
    [SerializeField] private ScriptableObjectEvent clearStageEvent;
    [SerializeField] private ScriptableObjectEvent resetStageEvent;
    [SerializeField] private ScriptableObjectEvent startStageEvent;

    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private string tutorialEndAnimationTriggerName;
    [SerializeField] private string deathAnimationTriggerName;
    [SerializeField] private string clearAnimationTriggerName;
    [SerializeField] private string resetAnimationTriggerName;
    [SerializeField] private string startAnimationTriggerName;
    [SerializeField] private string gameEndTriggerName;
    [SerializeField] private float deathAnimationDelay;
    [SerializeField] private float clearAnimationDelay;
    [SerializeField] private float resetAnimationDelay;

    [Header("Scene Management")]
    [SerializeField] private int currentStage;
    [SerializeField] private string[] stageSceneNames;
    [SerializeField] private string endSceneName;

    [Header("Logging")]
    [SerializeField] private Logger logger;

    private float activateLoadedSceneTime;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerDeathEvent.Subscribe(OnPlayerDeath);
            clearStageEvent.Subscribe(OnStageCleared);
            resetStageEvent.Subscribe(OnStageReset);
            tutorialEndEvent.Subscribe(OnAfterTutorial);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnAfterTutorial()
    {
        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.STAGE_START, context: currentStage.ToString()));

        animator.SetTrigger(tutorialEndAnimationTriggerName);
        startStageEvent.Invoke();
    }

    private void OnPlayerDeath()
    {
        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.DEATH, context: currentStage.ToString()));

        animator.SetTrigger(deathAnimationTriggerName);
        StartCoroutine(LoadSceneDelayed(deathAnimationDelay, SceneManager.GetActiveScene().name));
    }

    private void OnStageCleared()
    {
        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.STAGE_CLEAR, context: currentStage.ToString()));

        animator.SetTrigger(clearAnimationTriggerName);
        currentStage++;

        if (currentStage >= stageSceneNames.Length)
        {
            logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.GAME_CLEAR, context: DateTime.UtcNow.Ticks.ToString()));
            StartCoroutine(LoadEnd(clearAnimationDelay, endSceneName));
        }
        else
        {
            StartCoroutine(LoadSceneDelayed(clearAnimationDelay, stageSceneNames[currentStage]));
        }
    }

    private void OnStageReset()
    {
        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.RESET, context: currentStage.ToString()));

        animator.SetTrigger(resetAnimationTriggerName);
        StartCoroutine(LoadSceneDelayed(deathAnimationDelay, SceneManager.GetActiveScene().name));
    }

    private IEnumerator LoadSceneDelayed(float beforeDelay, string scene)
    {
        activateLoadedSceneTime = Time.time + beforeDelay;

        yield return new WaitForSeconds(beforeDelay);

        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (activateLoadedSceneTime > Time.time)
        {
            yield return null;
        }

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        animator.SetTrigger(startAnimationTriggerName);

        logger.Append(new Logger.LogEntry(Logger.LogEntry.Category.STAGE_START, context: currentStage.ToString()));

        startStageEvent.Invoke();
    }

    private IEnumerator LoadEnd(float beforeDelay, string scene)
    {
        activateLoadedSceneTime = Time.time + beforeDelay;

        yield return new WaitForSeconds(beforeDelay);

        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (activateLoadedSceneTime > Time.time)
        {
            yield return null;
        }

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        animator.SetTrigger(gameEndTriggerName);
    }
}
