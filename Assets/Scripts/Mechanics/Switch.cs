using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Vector3 pressedCheckOffset;
    [SerializeField] private Vector3 pressedCheckHalfExtents;
    [SerializeField] private LayerMask pressedCheckLayerMask;
    [SerializeField] private GameObject target;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer model;
    [SerializeField] private List<Material> onMaterials;
    [SerializeField] private List<Material> offMaterials;

    private ISwitchable targetSwitchable;
    private Collider[] buffer;

    private bool onLastFrame = false;

    private void Start()
    {
        buffer = new Collider[1];
        targetSwitchable = target.GetComponent<ISwitchable>();

        if (targetSwitchable == null) Debug.LogError($"No ISwitchable implementation found on target object {target.name}.");
    }

    private void FixedUpdate()
    {
        int hitCount = Physics.OverlapBoxNonAlloc(transform.position + pressedCheckOffset, pressedCheckHalfExtents, buffer, transform.rotation, pressedCheckLayerMask);
        if (hitCount > 0)
        {
            if (!onLastFrame)
            {
                targetSwitchable.On();
                model.SetMaterials(onMaterials);
                onLastFrame = true;
            }
        }
        else
        {
            if (onLastFrame)
            {
                targetSwitchable.Off();
                model.SetMaterials(offMaterials);
                onLastFrame = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + pressedCheckOffset, pressedCheckHalfExtents * 2);
    }
}

public interface ISwitchable
{
    void On();
    void Off();
}
