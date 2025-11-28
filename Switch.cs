// Switch.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;

public class Switch : MonoBehaviour
{
    [Header("ThingSpeak Settings (leave blank to disable)")]
    [Tooltip("ThingSpeak channel ID (numbers only)")]
    public string channelID = "3184195";

    [Tooltip("ThingSpeak Read API key")]
    public string readAPIKey = "6I0SUVI72MAGQA5S";

    [Tooltip("Field number to read from (usually 1)")]
    public int fieldNumber = 1;

    [Tooltip("Seconds between ThingSpeak reads")]
    public float refreshInterval = 5f;

    [Header("Behavior")]
    [Tooltip("If true, ThingSpeak polling runs. If false, only keyboard controls work.")]
    public bool enableThingSpeakPolling = true;

    private Light myLight;
    private Coroutine pollingCoroutine;

    void Awake()
    {
        myLight = GetComponent<Light>();
        if (myLight == null)
        {
            Debug.LogError($"Switch.cs on GameObject '{gameObject.name}' needs a Light component.");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        // start polling only if enabled and keys are set
        if (enableThingSpeakPolling && !string.IsNullOrEmpty(channelID) && !string.IsNullOrEmpty(readAPIKey))
        {
            // Just to be safe, ensure interval is sensible
            if (refreshInterval <= 0f) refreshInterval = 5f;
            pollingCoroutine = StartCoroutine(ReadThingSpeakLoop());
        }
    }

    void OnDisable()
    {
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
    }

    void Update()
    {
        // Keyboard override (instant)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                myLight.enabled = true;
                Debug.Log("Switch: Keyboard -> Light ON");
            }

            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                myLight.enabled = false;
                Debug.Log("Switch: Keyboard -> Light OFF");
            }
        }
    }

    IEnumerator ReadThingSpeakLoop()
    {
        // Build URL template once
        string urlTemplate = $"https://api.thingspeak.com/channels/{channelID}/fields/{{0}}/last.json?api_key={readAPIKey}";

        while (true)
        {
            string url = string.Format(urlTemplate, fieldNumber);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // send and wait
#if UNITY_2020_1_OR_NEWER
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
#else
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogWarning($"ThingSpeak read failed: {request.error}");
                }
                else
                {
                    var json = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(json))
                    {
                        ThingSpeakResponse resp = null;
                        try
                        {
                            resp = JsonUtility.FromJson<ThingSpeakResponse>(json);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"ThingSpeak JSON parse error: {ex.Message}\nRaw: {json}");
                        }

                        if (resp != null && !string.IsNullOrEmpty(resp.field1))
                        {
                            // Try parse integer safely
                            if (int.TryParse(resp.field1.Trim(), out int val))
                            {
                                if (val == 1 && !myLight.enabled)
                                {
                                    myLight.enabled = true;
                                    Debug.Log("ThingSpeak -> Motion=1 : Light ON");
                                }
                                else if (val == 0 && myLight.enabled)
                                {
                                    myLight.enabled = false;
                                    Debug.Log("ThingSpeak -> Motion=0 : Light OFF");
                                }
                                else
                                {
                                    // same state - nothing to do
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"ThingSpeak returned non-integer field1: '{resp.field1}'");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("ThingSpeak response missing field1 or was empty.");
                        }
                    }
                }
            }

            yield return new WaitForSeconds(refreshInterval);
        }
    }

    // Simple class to match ThingSpeak's last.json response (we only need field1)
    [System.Serializable]
    private class ThingSpeakResponse
    {
        public string field1;
    }
}
