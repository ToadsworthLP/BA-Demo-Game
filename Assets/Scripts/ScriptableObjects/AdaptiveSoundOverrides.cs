using UnityEngine;

[CreateAssetMenu(fileName = "New Adaptive Sound Overrides", menuName = "Adaptive Sound Overrides")]
public class AdaptiveSoundOverrides : ScriptableObject
{
    public float intensityOffset = 0f;
    public float intensityMultiplier = 1f;
    public float defaultIntensity = 0f;
}
