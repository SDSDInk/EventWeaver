using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDSDInk.EventWeaver{
    public class SD_UpdateEventWeaverObject : MonoBehaviour
    {
        [Header("Settings")]
        // the objects refHash
        [SerializeField] private EventWeaverObjectUpdateType eventWeaverObjectUpdateType;

        [ShowIf("eventWeaverObjectUpdateType", (int)EventWeaverObjectUpdateType.FindByRefHash)]
        [SerializeField, Tooltip("The existing EVO ref hash")] private string existingEventWeaverObjectRefHash = "Existing Event Weaver Object RefHash";
        [ShowIf("eventWeaverObjectUpdateType", (int)EventWeaverObjectUpdateType.FindBynName)]
        [SerializeField, Tooltip("The existing EVO gameobject / transform name")] private string existingEventWeaverObjectName = "Existing Event Weaver Object Name";
        [SerializeField, Tooltip("The new EVO ref hash")] private string newRefHash = "new Ref Hash";

        public void ChangeEventWeaverObjectHash()
        {
            if (eventWeaverObjectUpdateType == EventWeaverObjectUpdateType.FindByRefHash)
            {
                SD_EventWeaverObject ewo = SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.FirstOrDefault(obj => obj.RefHash == existingEventWeaverObjectRefHash);
                if (ewo != null)
                    ewo.RefHash = newRefHash;
                else
                    Debug.LogWarning("Did not find Event Weaver Object with RefHash: " + existingEventWeaverObjectRefHash);
            }
            else
            {
                SD_EventWeaverObject ewo = SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.FirstOrDefault(obj => obj.name == existingEventWeaverObjectName);
                if (ewo != null)
                    ewo.RefHash = newRefHash;
                else
                    Debug.LogWarning("Did not find Event Weaver Object with Name: " + existingEventWeaverObjectName);
            }
        }

        enum EventWeaverObjectUpdateType
        {
            FindByRefHash,
            FindBynName
        }

    }

}