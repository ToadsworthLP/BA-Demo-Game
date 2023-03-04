using System;
using UnityEngine;

public class AdaptiveSoundTrigger : MonoBehaviour
{
    [Serializable]
    public class Condition
    {
        public LayerMask checkLayer;
        public ConditionShape shape;
        public Vector3 offset;
        public float size;
        public bool invert;
    }

    public enum ConditionShape { CUBE, SPHERE };
    public enum LogicalOperator { AND, OR };

    [SerializeField] private AdaptiveSoundTriggerRegistry registry;
    [SerializeField] private Condition[] conditions;
    [SerializeField] private LogicalOperator logicalOperator;
    [SerializeField] private float weight;

    private Collider[] hitBuffer;

    public float Query()
    {
        bool currentValue = logicalOperator == LogicalOperator.AND;

        foreach (Condition condition in conditions)
        {
            int hits = 0;

            if (condition.shape == ConditionShape.CUBE)
            {
                hits = Physics.OverlapBoxNonAlloc(transform.position + condition.offset, new Vector3(condition.size / 2, condition.size / 2, condition.size / 2), hitBuffer, Quaternion.identity, condition.checkLayer);

            }
            else if (condition.shape == ConditionShape.SPHERE)
            {
                hits = Physics.OverlapSphereNonAlloc(transform.position + condition.offset, condition.size, hitBuffer, condition.checkLayer);
            }

            if (logicalOperator == LogicalOperator.AND)
            {
                currentValue &= condition.invert ? hits <= 0 : hits > 0;
            }
            else if (logicalOperator == LogicalOperator.OR)
            {
                currentValue |= condition.invert ? hits <= 0 : hits > 0;
            }
        }

        return currentValue ? 1f * weight : 0f;
    }

    private void Start()
    {
        hitBuffer = new Collider[1];
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

            if (condition.shape == ConditionShape.CUBE)
            {
                Gizmos.DrawWireCube(transform.position + condition.offset, new Vector3(condition.size, condition.size, condition.size));
            }
            else if (condition.shape == ConditionShape.SPHERE)
            {
                Gizmos.DrawWireSphere(transform.position + condition.offset, condition.size);
            }
        }
    }

#endif
}
