using System;
using UnityEngine;

public class AdaptiveSoundTrigger : MonoBehaviour
{
    [Serializable]
    public class Condition
    {
        public LayerMask checkLayer;
        public Vector3 offset;
        public Vector3 halfExtents;
        public GameObject specificTarget;
        public bool invert;
    }

    public enum ConditionShape { CUBE, SPHERE };
    public enum LogicalOperator { AND, OR };

    [SerializeField] private AdaptiveSoundTriggerRegistry registry;
    [SerializeField] private Condition[] conditions;
    [SerializeField] private LogicalOperator logicalOperator;
    [SerializeField] private bool invert;
    [SerializeField] private float weight;

    private Collider[] hitBuffer;

    public float Query()
    {
        int passedConditions = 0;

        foreach (Condition condition in conditions)
        {
            int hits = 0;

            hits = Physics.OverlapBoxNonAlloc(transform.position + condition.offset, condition.halfExtents, hitBuffer, Quaternion.identity, condition.checkLayer);

            if (condition.specificTarget != null)
            {
                bool found = false;

                for (int i = 0; i < hits; i++)
                {
                    if (hitBuffer[i].gameObject == condition.specificTarget) found |= true;
                }

                if (!found) hits = 0;
            }

            if (condition.invert)
            {
                if (hits == 0) passedConditions++;
            }
            else
            {
                if (hits > 0) passedConditions++;
            }
        }

        bool result;
        if (logicalOperator == LogicalOperator.AND)
        {
            result = passedConditions == conditions.Length;
        }
        else
        {
            result = passedConditions > 0;
        }

        return ((invert ? !result : result) ? 1f : 0f) * weight;
    }

    private void Start()
    {
        hitBuffer = new Collider[4];
        registry.Register(this);
    }

    private void OnDestroy()
    {
        registry.Unregister(this);
    }

#if UNITY_EDITOR

    private Color[] previewColors = new Color[5] { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
    private void OnDrawGizmos()
    {
        if (conditions.Length == 0) return;

        for (int i = 0; i < conditions.Length; i++)
        {
            Condition condition = conditions[i];
            Gizmos.color = previewColors[i % previewColors.Length];

            Gizmos.DrawWireCube(transform.position + condition.offset, condition.halfExtents * 2);
        }
    }

#endif
}
