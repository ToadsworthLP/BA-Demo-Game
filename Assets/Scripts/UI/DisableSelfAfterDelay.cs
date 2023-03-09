using System.Collections;
using UnityEngine;

public class DisableSelfAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay;

    private CanvasGroup canvasGroup;

    private float startTime;
    private float endTime;
    private Coroutine animationCoroutine;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (animationCoroutine != null) return;

        startTime = Time.time;
        endTime = startTime + delay;
        animationCoroutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (Time.time < endTime)
        {
            float progress = Mathf.Clamp01((Time.time - startTime) / delay);

            if (progress <= 0.1)
            {
                canvasGroup.alpha = progress * 10;
            }
            else if (progress >= 0.9 && progress < 1)
            {
                canvasGroup.alpha = (1 - progress) * 10;
            }
            else
            {
                canvasGroup.alpha = 1;
            }

            yield return null;
        }

        animationCoroutine = null;
        gameObject.SetActive(false);
    }
}
