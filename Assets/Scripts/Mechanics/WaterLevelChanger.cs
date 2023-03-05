using FMODUnity;
using UnityEngine;

public class WaterLevelChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private float targetWaterLevel;
    [SerializeField] private float changeRate;
    [SerializeField] private ChangeWaterLevelEvent changeTargetWaterLevelEvent;
    [SerializeField] private FloatContainer currentWaterLevel;
    [SerializeField] private ChangeWaterLevelEvent previewOnEvent;
    [SerializeField] private ChangeWaterLevelEvent previewOffEvent;
    [SerializeField] private GameObject rendererObject;
    [SerializeField] private float turnAnimationNormalSpeed;
    [SerializeField] private float turnAnimationActiveSpeed;
    [SerializeField] private float turnAnimationTransitionDuration;
    [SerializeField] private EventReference activateSound;

    private float activeTimeRemaining;
    private float currentAnimationSpeed;
    private float targetAnimationSpeed;

    private ParticleSystem particles;

    public void Focus(InteractionContext context)
    {
        rendererObject.layer = Constants.FOCUSED_INTERACTABLE_LAYER;
        previewOnEvent.Invoke(new ChangeWaterLevelEventArgs()
        {
            targetLevel = targetWaterLevel,
            changeRate = changeRate
        });
    }

    public void Interact(InteractionContext context)
    {
        changeTargetWaterLevelEvent.Invoke(new ChangeWaterLevelEventArgs()
        {
            targetLevel = targetWaterLevel,
            changeRate = changeRate
        });

        RuntimeManager.PlayOneShot(activateSound, transform.position);
        particles.Play();

        activeTimeRemaining = Mathf.Abs(currentWaterLevel - targetWaterLevel) / changeRate;
        targetAnimationSpeed = turnAnimationActiveSpeed;
    }

    public void Unfocus(InteractionContext context)
    {
        rendererObject.layer = Constants.INTERACTABLE_LAYER;

        previewOffEvent.Invoke(new ChangeWaterLevelEventArgs()
        {
            targetLevel = targetWaterLevel,
            changeRate = changeRate
        });
    }

    private void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        rendererObject.transform.Rotate(transform.up, currentAnimationSpeed * Time.deltaTime, Space.Self);

        currentAnimationSpeed = Mathf.MoveTowards(currentAnimationSpeed, targetAnimationSpeed, turnAnimationTransitionDuration);

        if (activeTimeRemaining > 0)
        {
            activeTimeRemaining -= Time.deltaTime;
        }
        else
        {
            activeTimeRemaining = 0;
            targetAnimationSpeed = turnAnimationNormalSpeed;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new Vector3(transform.position.x, targetWaterLevel, transform.position.z), new Vector3(3, 0.01f, 3));
    }
}
