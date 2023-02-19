using System.Collections;
using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    [SerializeField] private float maxChangeRate;
    [SerializeField] private ChangeWaterLevelEvent changeWaterLevelEvent;

    private float targetLevel;
    private Coroutine waterLevelChangeCoroutine;

    private void OnEnable()
    {
        changeWaterLevelEvent.Subscribe(ChangeWaterLevel);
    }

    private void OnDisable()
    {
        changeWaterLevelEvent.Unsubscribe(ChangeWaterLevel);
    }

    private void ChangeWaterLevel(ChangeWaterLevelEventArgs args)
    {
        targetLevel = args.targetLevel;
        if (waterLevelChangeCoroutine == null) waterLevelChangeCoroutine = StartCoroutine(WaterLevelChanger());
    }

    private IEnumerator WaterLevelChanger()
    {
        while (transform.position.y != targetLevel)
        {
            if (Mathf.Abs(transform.position.y - targetLevel) > maxChangeRate * Time.deltaTime)
            {
                transform.position += new Vector3(0, maxChangeRate * -Mathf.Sign(transform.position.y - targetLevel) * Time.deltaTime, 0);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, targetLevel, transform.position.z);
            }

            yield return new WaitForEndOfFrame();
        }

        waterLevelChangeCoroutine = null;
    }
}
