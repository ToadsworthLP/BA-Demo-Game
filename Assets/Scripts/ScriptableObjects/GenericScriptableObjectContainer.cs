using UnityEngine;

public class GenericScriptableObjectContainer<T> : ScriptableObject
{
    public T Value;

    public static implicit operator T(GenericScriptableObjectContainer<T> container)
    {
        return container.Value;
    }
}
