using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDSDInk.EventWeaver{
    public class SD_FindEventWeaverObject : MonoBehaviour
    {
        [Header("Settings")]
        // the objects refHash
        [SerializeField, Tooltip("How will you look up the existing Event Weaver Object?")] private EventWeaverObjectFindType eventWeaverObjectFindType;

        [ShowIf("eventWeaverObjectFindType", (int)EventWeaverObjectFindType.FindByRefHash)]
        [SerializeField, Tooltip("The existing EVO ref hash to reference")] private string existingEventWeaverObjectRefHash = "Event Weaver Object RefHash";
        [ShowIf("eventWeaverObjectFindType", (int)EventWeaverObjectFindType.FindBynName)]
        [SerializeField, Tooltip("The existing EVO gameobject / transform name to reference")] private string existingEventWeaverObjectName = "Event Weaver Object Name";

        public SD_EventWeaverObject FindWeaverObject { get => eventWeaverObject; }
        SD_EventWeaverObject eventWeaverObject;

        public SD_EventWeaverObject ChangeEventWeaverObjectHash()
        {
            if (eventWeaverObjectFindType == EventWeaverObjectFindType.FindByRefHash)
            {
                eventWeaverObject = SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.FirstOrDefault(obj => obj.RefHash == existingEventWeaverObjectRefHash);
                if (eventWeaverObject == null)
                    Debug.LogWarning("Did not find Event Weaver Object with RefHash: " + existingEventWeaverObjectRefHash);
            }
            else
            {
                eventWeaverObject = SD_EventManager.Instance.EventMonitoring.EventWeaverObjects.FirstOrDefault(obj => obj.name == existingEventWeaverObjectName);
                if (eventWeaverObject == null)
                    Debug.LogWarning("Did not find Event Weaver Object with Name: " + existingEventWeaverObjectName);
            }
            return eventWeaverObject;
        }

        enum EventWeaverObjectFindType
        {
            FindByRefHash,
            FindBynName
        }

    }

}