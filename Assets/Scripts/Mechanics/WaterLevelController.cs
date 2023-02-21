using Cinemachine;
using System.Collections;
using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    [SerializeField] private float maxLevelChangeRate;
    [SerializeField] private float initialWaterLevel;
    [SerializeField] private float waterShoreFadeOutTime;
    [SerializeField] private float waterShoreFadeInTime;
    [SerializeField] private float shoreAlphaClipVisible;
    [SerializeField] private float shoreAlphaClipInvisible;
    [SerializeField] private Vector3 cameraStartShakeVelocity;
    [SerializeField] private Vector3 cameraEndShakeVelocity;
    [SerializeField] private ChangeWaterLevelEvent changeWaterLevelEvent;
    [SerializeField] private Material towerMaterial;
    [SerializeField] private Material[] shoreMaterials;

    private float targetLevel;
    private float shoreAlphaClip;
    private Coroutine waterLevelChangeCoroutine;

    private CinemachineImpulseSource cameraShakeImpulse;

    private void Start()
    {
        cameraShakeImpulse = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        changeWaterLevelEvent.Subscribe(ChangeWaterLevel);
        transform.position = new Vector3(transform.position.x, initialWaterLevel, transform.position.z);
        towerMaterial.SetFloat("_Border", initialWaterLevel);
    }

    private void OnDisable()
    {
        changeWaterLevelEvent.Unsubscribe(ChangeWaterLevel);
        transform.position = new Vector3(transform.position.x, initialWaterLevel, transform.position.z);
        towerMaterial.SetFloat("_Border", initialWaterLevel);
    }

    private void ChangeWaterLevel(ChangeWaterLevelEventArgs args)
    {
        if (targetLevel == args.targetLevel) return;

        targetLevel = args.targetLevel;
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

            if (Mathf.Abs(level - targetLevel) > maxLevelChangeRate * Time.deltaTime)
            {
                level += maxLevelChangeRate * -Mathf.Sign(level - targetLevel) * Time.deltaTime;
            }
            else
            {
                level = targetLevel;
            }

            transform.position = new Vector3(transform.position.x, level, transform.position.z);
            towerMaterial.SetFloat("_Border", level);

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
