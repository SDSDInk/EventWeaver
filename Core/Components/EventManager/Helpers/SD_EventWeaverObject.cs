using System;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    public class SD_EventWeaverObject : MonoBehaviour
    {
        [Tooltip("How will you be referencing this object later?")]
        public string RefHash = "";

        // Initialize the Event Weaver Object on Awake
        private void Awake() => Initialize();        

        public void Initialize(string _refHash = "")
        {
            // if set from another script
            RefHash = _refHash;

            // if there is no refhash then give it one
            if(RefHash == string.Empty)
                RefHash = SD_EventWeaverCommons.GenerateMD5Hash(transform.name + DateTime.Now).ToString();

            // add this object to the Event Weaver Objects Pool
            if (!SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.Contains(this))
                SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.Add(this);
        }

        private void OnDestroy()
        {
            // remove from the pool
            if (SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.Contains(this))
                SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.Remove(this);
        }
    }
}