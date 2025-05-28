using System.Collections;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    public class SD_WaitForSeconds : MonoBehaviour
    {
        [SerializeField] private float waitTimeInSeconds;

        public float CurrentValue { get => currentValue; } 
        float currentValue = 0f;

        public void StartWait()
        {
            SD_EventManager.Instance.EventMonitoring.ExternalProcessors.Add(GetInstanceID().ToString());
            StartCoroutine(WaitForSeconds());
        }

        IEnumerator WaitForSeconds()
        {
            float elapsedTime = 0f; // Tracks how much time has passed
            float targetTime = waitTimeInSeconds; // The duration for the countdown
            while (elapsedTime < targetTime)
            {
                // Increment elapsed time by the time passed since the last frame
                elapsedTime += Time.deltaTime;

                // Calculate the current value as a normalized progression
                currentValue = Mathf.Lerp(0f, targetTime, elapsedTime / targetTime);

                yield return null; // Wait until the next frame
            }
            SD_EventManager.Instance.EventMonitoring.ExternalProcessors.Remove(GetInstanceID().ToString());
        }
    }
    
}