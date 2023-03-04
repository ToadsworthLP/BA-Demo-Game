using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Adaptive Sound Trigger Registry", menuName = "Adaptive Sound Trigger Registry")]
public class AdaptiveSoundTriggerRegistry : ScriptableObject
{
    private IList<AdaptiveSoundTrigger> triggers = new List<AdaptiveSoundTrigger>();
    private IList<float> cachedResults = new List<float>();
    private int updateIndex = 0;

    public void Register(AdaptiveSoundTrigger trigger)
    {
        triggers.Add(trigger);
        cachedResults.Add(0);
    }

    public void Unregister(AdaptiveSoundTrigger trigger)
    {
        int index = triggers.IndexOf(trigger);
        triggers.RemoveAt(index);
        cachedResults.RemoveAt(0);
        updateIndex = 0;
    }

    public void Update()
    {
        if (triggers.Count == 0) return;

        // It would be completely unnecessary to update each trigger every frame, so only one is actually queried per frame while the rest is cached
        cachedResults[updateIndex] = triggers[updateIndex].Query();
        updateIndex++;
        updateIndex %= triggers.Count;
    }

    public float ReadValue01()
    {
        if (triggers.Count == 0) return 0;

        float sum = 0;
        foreach (float cached in cachedResults)
        {
            sum += cached;
        }

        return sum / triggers.Count;
    }
}
