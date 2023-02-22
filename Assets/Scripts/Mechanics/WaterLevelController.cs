using Cinemachine;
using System.Collections;
using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    [SerializeField] private float initialWaterLevel;
    [SerializeField] private float waterShoreFadeOutTime;
    [SerializeField] private float waterShoreFadeInTime;
    [SerializeField] private float shoreAlphaClipVisible;
    [SerializeField] private float shoreAlphaClipInvisible;
    [SerializeField] private Vector3 cameraStartShakeVelocity;
    [SerializeField] private Vector3 cameraEndShakeVelocity;
    [SerializeField] private FloatContainer currentWaterLevel;
    [SerializeField] private ChangeWaterLevelEvent changeTargetWaterLevelEvent;
    [SerializeField] private Material towerMaterial;
    [SerializeField] private Material[] shoreMaterials;

    private float currentChangeRate;
    private float targetLevel;
    private float shoreAlphaClip;
    private Coroutine waterLevelChangeCoroutine;

    private CinemachineImpulseSource cameraShakeImpulse;

    private void Start()
    {
        cameraShakeImpulse = GetComponent<CinemachineImpulseSource>();
        currentWaterLevel.Value = initialWaterLevel;

        shoreAlphaClip = shoreAlphaClipVisible;
    }

    private void OnEnable()
    {
        changeTargetWaterLevelEvent.Subscribe(ChangeWaterLevel);
        UpdateShoreAlphaClipThreshold(shoreAlphaClipVisible);
        towerMaterial.SetFloat("_Border", initialWaterLevel);
        currentWaterLevel.Value = initialWaterLevel;
    }

    private void OnDisable()
    {
        changeTargetWaterLevelEvent.Unsubscribe(ChangeWaterLevel);
        UpdateShoreAlphaClipThreshold(shoreAlphaClipInvisible);
        towerMaterial.SetFloat("_Border", initialWaterLevel);
        currentWaterLevel.Value = initialWaterLevel;
    }

    private void ChangeWaterLevel(ChangeWaterLevelEventArgs args)
    {
        if (targetLevel == args.targetLevel) return;

        targetLevel = args.targetLevel;
        currentChangeRate = args.changeRate;

        if (waterLevelChangeCoroutine == null)
        {
            waterLevelChangeCoroutine = StartCoroutine(WaterLevelChanger());
        }
        else
        {
            StopCoroutine(waterLevelChangeCoroutine);
            waterLevelChangeCoroutine = StartCoroutine(WaterLevelChanger());
        }
    }

    private IEnumerator WaterLevelChanger()
    {
        cameraShakeImpulse.GenerateImpulse(cameraStartShakeVelocity);

        while (shoreAlphaClip < shoreAlphaClipInvisible)
        {
            shoreAlphaClip += 0.5f / waterShoreFadeOutTime * Time.deltaTime;
            UpdateShoreAlphaClipThreshold(shoreAlphaClip);
            yield return new WaitForEndOfFrame();
        }

        shoreAlphaClip = shoreAlphaClipInvisible;
        UpdateShoreAlphaClipThreshold(shoreAlphaClip);

        while (transform.position.y != targetLevel)
        {
            float level = transform.position.y;

            if (Mathf.Abs(level - targetLevel) > currentChangeRate * Time.deltaTime)
            {
                level += currentChangeRate * -Mathf.Sign(level - targetLevel) * Time.deltaTime;
            }
            else
            {
                level = targetLevel;
            }

            transform.position = new Vector3(transform.position.x, level, transform.position.z);
            towerMaterial.SetFloat("_Border", level);
            currentWaterLevel.Value = level;

            yield return new WaitForEndOfFrame();
        }

        cameraShakeImpulse.GenerateImpulse(cameraEndShakeVelocity);

        while (shoreAlphaClip > shoreAlphaClipVisible)
        {
            shoreAlphaClip -= 0.5f / waterShoreFadeInTime * Time.deltaTime;
            UpdateShoreAlphaClipThreshold(shoreAlphaClip);
            yield return new WaitForEndOfFrame();
        }

        shoreAlphaClip = shoreAlphaClipVisible;
        UpdateShoreAlphaClipThreshold(shoreAlphaClip);

        waterLevelChangeCoroutine = null;
    }

    private void UpdateShoreAlphaClipThreshold(float value)
    {
        foreach (Material material in shoreMaterials)
        {
            material.SetFloat("_AlphaClipThreshold", value);
        }
    }
}
