using System.Collections;
using UnityEngine;

public class SwitchableObject : MonoBehaviour, ISwitchable
{
    [SerializeField] private Transform objectToSwitch;
    [SerializeField] private Transform objectToAnimate;
    [SerializeField] private float animationSpeed;
    [SerializeField] private bool invert;

    private Coroutine animationCoroutine;

    private float initialScale;
    private float targetScale;
    private float currentScale;

    private void Start()
    {
        initialScale = objectToAnimate.localScale.x;

        if (!invert)
        {
            objectToAnimate.localScale = Vector3.zero;
            currentScale = 0;
            objectToSwitch.gameObject.SetActive(false);
        }
        else
        {
            currentScale = initialScale;
        }
    }

    public void On()
    {
        targetScale = invert ? 0 : initialScale;
        objectToSwitch.gameObject.SetActive(!invert);

        if (animationCoroutine == null) animationCoroutine = StartCoroutine(AnimateSwitching());
    }

    public void Off()
    {
        targetScale = invert ? initialScale : 0;
        objectToSwitch.gameObject.SetActive(invert);

        if (animationCoroutine == null) animationCoroutine = StartCoroutine(AnimateSwitching());
    }

    private IEnumerator AnimateSwitching()
    {
        while (Mathf.Abs(currentScale - targetScale) > 0.01f)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, 1 - Mathf.Exp(-animationSpeed * Time.deltaTime));
            objectToAnimate.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return new WaitForEndOfFrame();
        }

        currentScale = targetScale;
        objectToAnimate.localScale = new Vector3(currentScale, currentScale, currentScale);
        animationCoroutine = null;
    }
}
