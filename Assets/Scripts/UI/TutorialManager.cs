using System;
using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Serializable]
    public class TutorialPage
    {
        public GameObject parentObject;
        public CanvasGroup[] canvasGroup;

        public void SetAlpha(float alpha)
        {
            foreach (CanvasGroup group in canvasGroup)
            {
                group.alpha = alpha;
            }
        }
    }

    [SerializeField] private PlayerController player;
    [SerializeField] private TutorialPage[] pages;
    [SerializeField] private float initialDelay;
    [SerializeField] private float afterDelay;
    [SerializeField] private float fadeTime;
    [SerializeField] private ScriptableObjectEvent tutorialEndEvent;
    [SerializeField] private BoolContainer isFMODReady;
    [SerializeField] private GameObject loadingText;

    private PlayerInput input;

    private int currentPage = 0;
    private float currentFadeTime = 0;

    private void Start()
    {
        player.IsFrozen = true;

        input = new PlayerInput();
        input.Main.Enable();

        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(initialDelay);

        while (currentPage < pages.Length)
        {
            pages[currentPage].parentObject.SetActive(true);

            while (currentFadeTime < fadeTime)
            {
                pages[currentPage].SetAlpha(Mathf.Clamp01(currentFadeTime / fadeTime));
                currentFadeTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            pages[currentPage].SetAlpha(1f);
            currentFadeTime = 0f;

            while (!input.Main.ContinueTutorial.IsPressed())
            {
                yield return null;
            }

            while (currentFadeTime < fadeTime)
            {
                pages[currentPage].SetAlpha(Mathf.Clamp01(1 - (currentFadeTime / fadeTime)));
                currentFadeTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            pages[currentPage].SetAlpha(0f);
            currentFadeTime = 0f;

            pages[currentPage].parentObject.SetActive(false);

            currentPage++;
        }

        yield return new WaitForSeconds(afterDelay);

        if (!isFMODReady) loadingText.SetActive(true);

        while (!isFMODReady) yield return null;

        loadingText.SetActive(false);

        player.IsFrozen = false;

        tutorialEndEvent.Invoke();
    }
}
