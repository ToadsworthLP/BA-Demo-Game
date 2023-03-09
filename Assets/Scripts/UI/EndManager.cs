using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EndManager : MonoBehaviour
{
    [SerializeField] private Logger logger;
    [SerializeField] private EventReference clickSound;
    [SerializeField] private EventReference successSound;
    [SerializeField, TextArea] private string successMessage;
    [SerializeField, TextArea] private string errorMessage;
    [SerializeField] private Button submitButton;
    [SerializeField] private GameObject submitText;
    [SerializeField] private GameObject submittingText;
    [SerializeField] private GameObject resultsDisplay;
    [SerializeField] private TMPro.TMP_Text resultsText;

    private Coroutine sendCoroutine;

    private void Start()
    {
        Debug.Log($"Log: {logger.ToString()}");
    }

    public void Send()
    {
        if (sendCoroutine != null) return;
        sendCoroutine = StartCoroutine(SendCoroutine());
    }

    IEnumerator SendCoroutine()
    {
        submitButton.interactable = false;
        submitText.SetActive(false);
        submittingText.SetActive(true);

        RuntimeManager.PlayOneShot(clickSound);

        using (UnityWebRequest request = UnityWebRequest.Post("http://toadsworth.ddns.net:5001/survey", $"\"{logger.ToString()}\"", "application/json"))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                submitButton.interactable = true;

                resultsText.text = errorMessage + request.error;
            }
            else
            {
                Debug.Log(successMessage);
                RuntimeManager.PlayOneShot(successSound);

                resultsText.text = successMessage;
            }
        }

        resultsDisplay.SetActive(true);
        submitText.SetActive(true);
        submittingText.SetActive(false);

        sendCoroutine = null;
    }
}
