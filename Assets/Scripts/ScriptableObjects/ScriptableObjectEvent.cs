using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Event", menuName = "Event Objects/Untyped")]
public class ScriptableObjectEvent : ScriptableObject
{
    [SerializeField] private UnityEvent unityEvent;

    private void OnEnable()
    {
        unityEvent = new UnityEvent();
    }

    public void Subscribe(UnityAction handler)
    {
        unityEvent.AddListener(handler);
    }

    public void Unsubscribe(UnityAction handler)
    {
        unityEvent.RemoveListener(handler);
    }

    public void Clear()
    {
        unityEvent.RemoveAllListeners();
    }

    public void Invoke()
    {
        unityEvent?.Invoke();
    }
}