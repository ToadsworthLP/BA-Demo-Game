using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Vector3 pressedCheckOffset;
    [SerializeField] private Vector3 pressedCheckHalfExtents;
    [SerializeField] private LayerMask pressedCheckLayerMask;
    [SerializeField] private GameObject[] targets;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer model;
    [SerializeField] private List<Material> onMaterials;
    [SerializeField] private List<Material> offMaterials;

    private ISwitchable[] targetSwitchables;
    private Collider[] buffer;

    private bool onLastFrame = false;

    private void Start()
    {
        buffer = new Collider[1];
        targetSwitchables = new ISwitchable[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targetSwitchables[i] = targets[i].GetComponent<ISwitchable>();
            if (targetSwitchables[i] == null) Debug.LogError($"No ISwitchable implementation found on target object {targets[i].name}.");
        }
    }

    private void FixedUpdate()
    {
        int hitCount = Physics.OverlapBoxNonAlloc(transform.position + pressedCheckOffset, pressedCheckHalfExtents, buffer, transform.rotation, pressedCheckLayerMask);
        if (hitCount > 0)
        {
            if (!onLastFrame)
            {
                foreach (ISwitchable targetSwitchable in targetSwitchables)
                {
                    targetSwitchable.On();
                }

                model.SetMaterials(onMaterials);
                onLastFrame = true;
            }
        }
        else
        {
            if (onLastFrame)
            {
                foreach (ISwitchable targetSwitchable in targetSwitchables)
                {
                    targetSwitchable.Off();
                }

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
