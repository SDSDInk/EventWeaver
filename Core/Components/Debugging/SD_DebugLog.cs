using UnityEngine;
using UnityEngine.InputSystem;

namespace SDSDInk.EventWeaver{
    public class SD_DebugLog : MonoBehaviour
    {
        public void DebugLog(string message)
        {
            Debug.Log(transform.name +": " + message);
        }

        public void DebugLogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        public void DebugLogError(string message)
        {
            Debug.LogError(message);
        }
    }
}