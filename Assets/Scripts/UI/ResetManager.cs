using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour
{
    [SerializeField] private ScriptableObjectEvent resetStageEvent;
    [SerializeField] private float delay;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image progressBar;
    [SerializeField] private CanvasRenderer[] renderers;

    private PlayerInput input;

    private float currentProgress;
    private bool isResetting = false;

    private void Start()
    {
        input = new PlayerInput();
        input.Main.Enable();
    }

    private void Update()
    {
        if (isResetting) return;

        bool pressed = input.Main.Restart.IsPressed();

        if (pressed)
        {
            currentProgress += Time.deltaTime;
        }
        else
        {
            currentProgress -= Time.deltaTime;
        }

        currentProgress = Mathf.Clamp(currentProgress, 0, delay);

        if (currentProgress >= delay)
        {
            resetStageEvent.Invoke();
            isResetting = true;
        }

        if (!isResetting)
        {
            UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        if (currentProgress > 0)
        {
            if (!canvas.gameObject.activeInHierarchy) canvas.gameObject.SetActive(true);

            float progress01 = currentProgress / delay;
            progressBar.fillAmount = progress01;

            float progressAlpha01 = Mathf.Clamp(progress01 * 10, 0, 1);
            foreach (CanvasRenderer renderer in renderers)
            {
                renderer.SetAlpha(progressAlpha01);
            }
        }
        else
        {
            canvas.gameObject.SetActive(false);
        }
    }
}
