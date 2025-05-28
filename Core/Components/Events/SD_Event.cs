using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace SDSDInk.EventWeaver
{
    [RequireComponent(typeof(SD_EventWeaverObject))]
    public class SD_Event : MonoBehaviour
    {
        private SD_EventManager eventManager;

        [SerializeField, Tooltip("Does this Event need a specific trigger?")]
        private bool requiresTag = true;

        [SerializeField, Tooltip("What tags are used to activate this event's triggers")]
        private string[] triggerTags = { "Player" };

        [SerializeField, Tooltip("Does the event manager store this event while its being interacted with, effectively putting the event manager into session")]
        private bool isAMonitoredEvent = true;

        [SerializeField, Tooltip("Does this events triggers need to be emptied of all required tag types before it will run again?")]
        private bool leaveTriggerToInteractAgain = false;

        [SerializeField, Tooltip("A prefab with a sprite to indicate that this event is the closest event to player. This will be the default action prefab if event page action prefabs are left blank.")]
        private GameObject globalActionPrefab;

        [SerializeField, Tooltip("The event pages of this event")]
        private EventPage[] events = new EventPage[0];

        [SerializeField, Tooltip("When first trigger enters / all triggers have left")]
        UnityEvent onTriggered, onEmptied, onActivePageNull;

        public bool debugMode = false;

        public List<GameObject> CurrentInteractors { get => currentInteractors.Keys.ToList(); }
        Dictionary<GameObject, int> currentInteractors = new Dictionary<GameObject, int>();

        public bool ClosestEventToPlayer { get => closestEventToPlayer; set => closestEventToPlayer = value; }
        private bool closestEventToPlayer = false;

        private bool interactLocked = false, onActivePageNullCalled = false;

        private GameObject graphicHolder, indicator;

        public EventPage ActivePage { get => activePage; }
        private EventPage activePage, previousPage;

        [SerializeField, HideInInspector] internal bool[] pagesToShow = new bool[1] { true };

        #region Trigger Entries

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (ContainsProperTags(other) || !requiresTag)
            {
                EnteredTrigger(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                ExitedTrigger(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                EnteredTrigger(other.gameObject);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                ExitedTrigger(other.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                EnteredTrigger(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                ExitedTrigger(other.gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                EnteredTrigger(other.gameObject);
        }

        private void OnCollisionExit(Collision other)
        {
            if (ContainsProperTags(other) || !requiresTag)
                ExitedTrigger(other.gameObject);
        }

        private void EnteredTrigger(GameObject _gameObject)
        {
            EnterIntoDictinoary(_gameObject);
        }

        private void ExitedTrigger(GameObject _gameObject)
        {
            RemoveFromDictionary(_gameObject);
        }

        void EnterIntoDictinoary(GameObject _gameObject)
        {
            // Add this trigger to the list of gameobjects in the objects trigger
            if (!currentInteractors.ContainsKey(_gameObject))
            {
                // first object to enter this trigger zone
                // triggers again once emptied
                if (currentInteractors.Count == 0)
                    onTriggered?.Invoke();

                currentInteractors[_gameObject] = _gameObject.GetInstanceID(); // Add it to the dictionary
                if (currentInteractors[_gameObject] == eventManager.PlayerID)
                {
                    eventManager.EventMonitoring.ActiveTriggers.Add(this);
                }
            }
        }

        void RemoveFromDictionary(GameObject _gameObject)
        {
            // Remove it from the list
            if (currentInteractors.ContainsKey(_gameObject))
            {
                if (currentInteractors[_gameObject] == eventManager.PlayerID)
                {
                    eventManager.EventMonitoring.ActiveTriggers.Remove(this);
                    closestEventToPlayer = false;
                }

                currentInteractors.Remove(_gameObject); // Add it to the dictionary

                // list is empty
                if (currentInteractors.Count == 0)
                {
                    onEmptied?.Invoke();
                    interactLocked = false;
                }
            }
        }

        //method to check in both 3d and 2d if the trigger that has entered meets the proper criteria
        private bool ContainsProperTags<T>(T colliderType)
        {
            if (colliderType is Collider2D other2D)
            {
                for (int i = 0; i < triggerTags.Length; i++)
                    if (triggerTags[i].ToLower() == other2D.tag.ToLower())
                        return true;
            }
            else if (colliderType is Collider other3D)
            {
                for (int i = 0; i < triggerTags.Length; i++)
                    if (triggerTags[i].ToLower() == other3D.tag.ToLower())
                        return true;
            }
            else if (colliderType is Collision collision3D)
            {
                for (int i = 0; i < triggerTags.Length; i++)
                    if (triggerTags[i].ToLower() == collision3D.transform.tag.ToLower())
                        return true;
            }
            else if (colliderType is Collision collision2D)
            {
                for (int i = 0; i < triggerTags.Length; i++)
                    if (triggerTags[i].ToLower() == collision2D.transform.tag.ToLower())
                        return true;
            }
            return false;
        }
        #endregion

        private void OnValidate() => UpdateEventTitles();

        private void Awake()
        {
            // get a local reference to the event manager
            eventManager = SD_EventManager.Instance;

            for (int i = 0; i < events.Length; i++)
            {
                //update each event so that the transform reference exists
                events[i].parent = transform;
            }
        }

        // remove this event from the event update list
        private void OnDisable()
        {
            eventManager.EventMonitoring.Tasks.RemoveListener(UpdateEvent);
        }

        // update this event and add it to the update list
        private void OnEnable()
        {
            activePage = null;
            previousPage = null;
            UpdateEvent();
            eventManager.EventMonitoring.Tasks.AddListener(UpdateEvent);
        }

        // Updates this event
        public void UpdateEvent()
        {
            PageSetup();
            //PageInteraction();
        }

        private void Update()
        {
            PageInteraction();
        }

        // Allows the event to be interacted with
        void PageInteraction()
        {
            // if already in session from onPageActive then autoStart wont happen
            // once onPageActive is complete (if sent into session) then _inSession will be null
            if (SD_EventManager.Instance.InSession || activePage == null || activePage.onInteract.inProcess != null || interactLocked || !ClosestEventToPlayer)
            {
                RemoveIndicator();
                return;
            }

            if(indicator == null)
                indicator = GenerateIndicator();

            bool actionKey = activePage.triggerCondition == TriggerConditions.ActionKey && SD_EventManager.Instance.settingsProfile.ActionKeyPressed && (currentInteractors.Count > 0 || !requiresTag);
            bool touchedByHero = (activePage.triggerCondition == TriggerConditions.OnCollision || activePage.triggerCondition == TriggerConditions.OnTrigger) && (currentInteractors.Count > 0 || !requiresTag);
            bool parallelProcess = activePage.triggerCondition == TriggerConditions.ParallelProcess;

            // set the interaction into session
            if (actionKey || touchedByHero || parallelProcess)
            {
                Interact();
            }
        }

        // sets up the event that should be displayed
        void PageSetup()
        {
            SelectActivePage();
            SetPageGraphics();
            UpdateEventTitles();
            if (activePage != null && previousPage != activePage)
            {
                previousPage = activePage;
                if (activePage.onPageActive.tasks.GetPersistentEventCount() > 0) {
                    if (activePage.onPageActive.processType == EventProcessingType.Sequential)
                        activePage.onPageActive.inProcess = StartCoroutine(InvokeUnityEventSequentially(activePage.onPageActive));
                    else
                        activePage.onPageActive.tasks?.Invoke();
                }
            }
            if (activePage == null && onActivePageNullCalled == false)
            {
                onActivePageNullCalled = true;
                onActivePageNull?.Invoke();
            }
        }

        // updates the active page based on the preconditions set forth in the events
        // it starts from the end looking backwards; so events should be ordered with this in mind
        void SelectActivePage()
        {
            if (events.Length == 0)
                return;

            // Check for pages with switches first
            for (int i = events.Length - 1; i > -1; i--)
            {
                // if this page is disabled look for another page
                // or if its processing and its already the active page
                if ((events[i].disablePage))
                    continue;

                if (SD_Preconditions.EvaluatePreconditions(events[i].preconditions))
                {
                    SetActivePage(events[i]);
                    onActivePageNullCalled = false;
                    return; // Found a matching page, exit early   
                }
            }
            SetActivePage(null);
        }

        // Sets the active page
        void SetActivePage(EventPage page)
        {
            if (activePage != null)
                activePage.isActivePage = false;

            if (page != null)
            {
                activePage = page;
                page.isActivePage = true;
                UpdateEventTitles();
            }
            else
                activePage = null;
        }

        // if a custom prefab is needed instead of a sprite
        void SetPageGraphics()
        {
            if (activePage == null)
                return;

            //using a special gameobject for this page
            if (activePage.graphicPrefab != null)
            {
                if (graphicHolder == null)
                {
                    graphicHolder = new GameObject();
                    graphicHolder.transform.parent = transform;
                    graphicHolder.transform.localPosition = Vector3.zero;
                    graphicHolder.transform.name = "GraphicHolder";
                }

                // otherwise remove the old graphic
                else
                {
                    Destroy(graphicHolder.transform.GetChild(0));
                }

                // and create the gameobject

                GameObject gp = Instantiate(activePage.graphicPrefab);
                gp.transform.parent = graphicHolder.transform;
                gp.transform.localPosition = Vector3.zero;
                gp.transform.localScale = Vector3.one;
            }
        }

        // updates the name of each page in the inspector
        void UpdateEventTitles()
        {
            if (events.Length == 0)
                return;

            // show user what page they're on
            for (int i = 0; i < events.Length; i++)
                events[i].page = "Page " + i + ": " + (events[i].eventName != "" ? events[i].eventName : "Untitled Event");
            //events[i].page = "Page " + i + ": " + (events[i].isActivePage ? "(isActivePage)" : "") + " " + (events[i].eventName!=""?events[i].eventName: "Untitled Event");
        }

        // when the correct trigger has been set, or called from an event or other script
        void Interact()
        {
            // true if set in the event settings
            interactLocked = leaveTriggerToInteractAgain;
            if (activePage.onInteract.tasks.GetPersistentEventCount() > 0)
            {
                if (activePage.onInteract.processType == EventProcessingType.Sequential)
                    activePage.onInteract.inProcess = StartCoroutine(InvokeUnityEventSequentially(activePage.onInteract));
                else
                    activePage.onInteract.tasks?.Invoke();
            }
        }

        public void ThrowEventInSession()
        {
            if(!eventManager.EventMonitoring.EventsInSession.Contains(this))
                eventManager.EventMonitoring.EventsInSession.Add(this);
        }

        public IEnumerator InvokeUnityEventSequentially(EventHandler handler)
        {
            // throw this event into session so that the event manager knows an event is running
            if (isAMonitoredEvent && !eventManager.EventMonitoring.EventsInSession.Contains(this))
                eventManager.EventMonitoring.EventsInSession.Add(this);

            // remove the indicator just in case
            RemoveIndicator();

            // yield and wait for this other coroutine to finish
            yield return StartCoroutine(
                SD_EventManager.Instance.SequentialUnityEvent(
                    handler,
                    SD_EventManager.Instance.StandardProcessingRules(activePage),
                    ()=>
                    {
                        UpdateEvent();
                        RemoveFromSession();
                    }
                ));
        }

        #region Public Methods
        GameObject GenerateIndicator()
        {
            GameObject _indicator = null;

            if ((globalActionPrefab != null || activePage.actionPrefab != null) && indicator == null)
            {
                GameObject indic = activePage.actionPrefab == null ? globalActionPrefab : activePage.actionPrefab;
                _indicator = Instantiate(indic, transform);
                _indicator.transform.localPosition = Vector3.zero;
            }

            return _indicator;
        }
       
        public void RemoveIndicator()
        {
            if (indicator != null)
                Destroy(indicator);
        }

        // Force Remove this event from session monitoring
        public void RemoveFromSession() {
            if (eventManager.EventMonitoring.EventsInSession.Contains(this))
            {
                StopAllCoroutines();// so that any sequential coroutines are stopped
                eventManager.EventMonitoring.EventsInSession.Remove(this);
            }
        }
        #endregion
    }

    public enum TriggerConditions { 
        ActionKey, 
        OnTrigger,
        OnCollision,
        ParallelProcess
    }

    public enum EventProcessingType {
        Sequential,
        Batch
    }

    [System.Serializable]
    public class EventHandler
    {
        public EventProcessingType processType;
        public UnityEvent tasks;
        public Coroutine inProcess;

        public EventHandler() { }
        public EventHandler(EventProcessingType _processingMode) {
            processType = _processingMode;
        }
    }

    [System.Serializable]
    public class EventPage
    {
        [HideInInspector] public string page = "";

        [HideInInspector] public Transform parent;

        [HideInInspector] public bool isActivePage = false;

        [Header("Event Page Settings")]
        [Tooltip("Optional: For organization purposes")]
        public string eventName = "event name";

        [Space(5)]
        [Tooltip("Overrides the global action prefab")]
        public GameObject actionPrefab;

        public GameObject graphicPrefab;

        public TriggerConditions triggerCondition;

        public Preconditions preconditions;

        [Header("Event Page Advanced Settings")]
        public bool showAdvancedSettings = false;

        [ShowIf("showAdvancedSettings", true)]
        public bool disablePage = false;

        [Header("Event Page Progression")]
        public EventHandler onPageActive = new EventHandler(EventProcessingType.Batch);

        [Space(10)]
        public EventHandler onInteract;

        [HideInInspector]
        public bool showOnPageActive, showOnInteract;

        public bool IsWaiting { get { return isWaiting; } set { isWaiting = value; } }
        bool isWaiting = false;
    }
}