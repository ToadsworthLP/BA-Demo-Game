using UnityEngine;

public class LevelOverridesSetter : MonoBehaviour
{
    [SerializeField] private AdaptiveSoundOverrides overrides;
    [SerializeField] private float intensityMultiplier = 1f;
    [SerializeField] private float intensityOffset = 0f;
    [SerializeField] private float defaultIntensity = 0f;

    private void Start()
    {
        overrides.intensityMultiplier = intensityMultiplier;
        overrides.intensityOffset = intensityOffset;
        overrides.defaultIntensity = defaultIntensity;
    }
}
