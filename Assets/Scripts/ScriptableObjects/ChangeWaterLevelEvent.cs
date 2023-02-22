using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Event Objects/Change Water Level")]
public class ChangeWaterLevelEvent : GenericScriptableObjectEvent<ChangeWaterLevelEventArgs> { }

public struct ChangeWaterLevelEventArgs
{
    public float targetLevel;
    public float changeRate;
}