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

    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private string tutorialEndAnimationTriggerName;
    [SerializeField] private string deathAnimationTriggerName;
    [SerializeField] private string clearAnimationTriggerName;
    [SerializeField] private string resetAnimationTriggerName;
    [SerializeField] private string startAnimationTriggerName;
    [SerializeField] private float deathAnimationDelay;
    [SerializeField] private float clearAnimationDelay;
    [SerializeField] private float resetAnimationDelay;

    [Header("Scene Management")]
    [SerializeField] private int currentStage;
    [SerializeField] private string[] stageSceneNames;

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
        animator.SetTrigger(tutorialEndAnimationTriggerName);
    }

    private void OnPlayerDeath()
    {
        animator.SetTrigger(deathAnimationTriggerName);
        StartCoroutine(LoadSceneDelayed(deathAnimationDelay, SceneManager.GetActiveScene().name));
    }

    private void OnStageCleared()
    {
        animator.SetTrigger(clearAnimationTriggerName);
        currentStage++;
        StartCoroutine(LoadSceneDelayed(clearAnimationDelay, stageSceneNames[currentStage]));
    }

    private void OnStageReset()
    {
        animator.SetTrigger(resetAnimationTriggerName);
        StartCoroutine(LoadSceneDelayed(deathAnimationDelay, SceneManager.GetActiveScene().name));
    }

    private IEnumerator LoadSceneDelayed(float beforeDelay, string scene)
    {
        yield return new WaitForSeconds(beforeDelay);

        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

        while (!op.isDone)
        {
            yield return null;
        }

        animator.SetTrigger(startAnimationTriggerName);
    }
}
