using UnityEngine;
using UnityEngine.Events;

public class GenericScriptableObjectEvent<TEventArgs> : ScriptableObject
{
    [SerializeField] private UnityEvent<TEventArgs> unityEvent;

    public void Subscribe(UnityAction<TEventArgs> handler)
    {
        unityEvent.AddListener(handler);
    }

    public void Unsubscribe(UnityAction<TEventArgs> handler)
    {
        unityEvent.RemoveListener(handler);
    }

    public void Clear()
    {
        unityEvent.RemoveAllListeners();
    }

    public void Invoke(TEventArgs args)
    {
        unityEvent?.Invoke(args);
    }
}
