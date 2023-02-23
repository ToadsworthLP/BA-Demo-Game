using UnityEngine;

public class AlignToFloor : MonoBehaviour
{
    [SerializeField] private float turnSharpness;
    [SerializeField] private float raycastLength;
    [SerializeField] private float raycastOffset;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask raycastLayerMask;

    private RaycastHit[] hitBuffer;
    private Vector3 targetUp;

    private void Start()
    {
        hitBuffer = new RaycastHit[8];
        targetUp = transform.up;
    }

    private void Update()
    {
        transform.up = Vector3.Slerp(transform.up, targetUp, 1 - Mathf.Exp(-turnSharpness * Time.deltaTime)).normalized;
    }

    private void FixedUpdate()
    {
        int hitCount = Physics.RaycastNonAlloc(raycastOrigin.position + (Vector3.up * raycastOffset), Vector3.down, hitBuffer, raycastLength, raycastLayerMask);
        if (hitCount > 0)
        {
            // Find closest hit that isn't a self-hit
            float currentClosestDistance = float.PositiveInfinity;
            int currentClosestIndex = -1;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];

                if (hit.collider.gameObject == gameObject) continue;

                if (hit.distance < currentClosestDistance)
                {
                    currentClosestDistance = hit.distance;
                    currentClosestIndex = i;
                }
            }

            if (currentClosestIndex >= 0)
            {
                targetUp = hitBuffer[currentClosestIndex].normal;
            }
            else
            {
                targetUp = Vector3.up;
            }
        }
        else
        {
            targetUp = Vector3.up;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(raycastOrigin.position + (Vector3.up * raycastOffset), raycastOrigin.position + (Vector3.up * raycastOffset) + (Vector3.down * raycastLength));
    }
}
