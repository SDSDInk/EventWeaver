using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SDSDInk.EventWeaver
{
    public class SD_EventManager : MonoBehaviour
    {
        public static SD_EventManager Instance;

        [SerializeField, Tooltip("Does this Instance persist accross scenes?")]
        private bool persistsAcrossScenes = true;

        [Tooltip("The settings profile that Event Weaver will use to establish things like your save system, dialogue system, input and more")]
        public SD_EventWeaverSettings settingsProfile;

        // all switches that will be used in the project
        public List<StateSwitch> Switches { get { return switches; } set { switches = value; } }
        [SerializeField, Tooltip("All switches being used by the event manager (Typically all in the project)")]
        private List<StateSwitch> switches = new();

        // default switches list for new games
        public List<StateSwitch> DefaultSwitches { get { return defaultSwitches; } }
        private List<StateSwitch> defaultSwitches = new();

        // all variables that will be used in the project
        public List<Variable> Variables { get { return variables; } set { variables = value; }  }
        [SerializeField, Tooltip("All variables being used by the event manager (Typically all in the project)")]
        private List<Variable> variables = new();

        // default variables list for new games
        public List<Variable> DefaultVariables { get { return defaultVariables; } }
        private List<Variable> defaultVariables = new();

        // events and their interaction will be triggered this often - higher numbers mean less responsive events
        public float EventUpdateTime { get => eventUpdateTime; set => eventUpdateTime = value; }
        [SerializeField, Tooltip("how often do events get updated? lower = more often")]
        private float eventUpdateTime = 0.1f;

        // the player gameobject that moves around in the scene. Should be able to be teleported.
        public GameObject PlayerGameObject {get => playerGameObject; set => playerGameObject = value; }
        [SerializeField, Tooltip("The Player GameObject When the Event Manager refers to the player")]
        private GameObject playerGameObject;

        // Events and tasks monitored by event weaver
        public EventMonitoring EventMonitoring { get { return eventMonitoring; } }
        [SerializeField, Tooltip("Used internally to track events and their tasks")]
        private EventMonitoring eventMonitoring;

        public UnityEvent OnBecameInSessionTrue;
        public UnityEvent OnBecameInSessionFalse;

        // the players player ID used to obtain if the player is triggering an event
        public int PlayerID { get => PlayerGameObject.GetInstanceID(); }

        // External Process allows special event components to persist while this condition is true
        public bool OneOrMoreExternalProcessors { get => eventMonitoring.ExternalProcessors.Count > 0;}

        public bool OneOrMoreWhileLoopProcessors { get => eventMonitoring.WhileLoopProcessors.Count > 0;}

        // if any of event weavers events are in session, then so is event weaver
        public bool InSession { get => EventMonitoring.EventsInSession.Count > 0; }
        bool lastInSession = false; // used to check if InSession has changed

        // one or more events are within range of the playerGameObject
        public bool HasActiveTrigger { get => EventMonitoring.ActiveTriggers.Count > 0; }

        private void Awake()
        {
            // Ensure there's only one instance
            if (Instance == null)
            {
                Instance = this;
                if (persistsAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }

                defaultSwitches = SD_EventWeaverCommons.CloneList(switches, s => s.Clone());
                defaultVariables = SD_EventWeaverCommons.CloneList(variables, v => v.Clone());

                InvokeRepeating("UpdateEvents", 0, eventUpdateTime);
            }
            // get rid of any other managers at runtime
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            UpdateProperties();
        }

        // show the user their switches a bit more organized
        private void OnValidate()
        {
            UpdateProperties();
        }

        public void UpdateProperties()
        {
            for (int i = 0; i < switches.Count; i++)
            {
                if (switches[i].stateSwitch != null)
                {
                    UpdateSwitchInspectorName(switches[i]);
                    switches[i].uniqueID = switches[i].stateSwitch.UniqueID;
                }
                else
                    switches[i].name = "No State Switch in Switch Slot!";
            }
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].variable != null)
                {
                    UpdateVariableInspectorName(variables[i]);
                    variables[i].uniqueID = variables[i].variable.UniqueID;
                }
                else
                    variables[i].name = "No Variable in Variable Slot!";
            }
        }

        // check here which active trigger is closest
        void GetClosestEventToPlayer()
        {
            if (playerGameObject != null)
            {
                if (eventMonitoring.ActiveTriggers.Count == 0)
                    return;

                SD_Event chosenOne = null;
                float shortestDistance = float.MaxValue;
                foreach (var item in eventMonitoring.ActiveTriggers)
                    item.ClosestEventToPlayer = false;

                foreach (var item in eventMonitoring.ActiveTriggers)
                {
                    float distanceToPlayer = Vector3.Distance(item.transform.position, playerGameObject.transform.position);
                    if (distanceToPlayer < shortestDistance)
                    {
                        shortestDistance = distanceToPlayer;
                        chosenOne = item;
                    }
                }
                chosenOne.ClosestEventToPlayer = true;
            }
        }

        // Remove all active triggers, triggered by the player, from the Active Triggers list
        IEnumerator RemoveAllActiveTriggersAfterTeleport()
        {
            // happens the frame after the player teleported
            yield return new WaitForEndOfFrame();
            eventMonitoring.ActiveTriggers.Clear();
        }

        // used with Sequential Unity Event to halt processing based on input conditions
        IEnumerator IsProcessing(System.Func<bool> ProcessCondition)
        {
            while (ProcessCondition())
            {
                yield return new WaitForEndOfFrame();
            }
        }

        // updates a switches name for inspector appearances
        void UpdateSwitchInspectorName(StateSwitch s) => s.name = s.stateSwitch.name + " (" + s.state.ToString() + ")";

        // updates a variables name for inspector appearances
        void UpdateVariableInspectorName(Variable v) => v.name = v.variable.name + " (" + v.value.ToString() + ")";

        #region Public Methods

        // Used in all instances where sequentually iterating over a Unity Event
        public System.Func<bool> StandardProcessingRules(EventPage activePage)
        {
            // Return a lambda that evaluates the conditions when invoked
            return () => ((
                (activePage != null && activePage.IsWaiting)
                || settingsProfile.DialogueSystemInSession
                || settingsProfile.ActionKeyPressed
                || OneOrMoreExternalProcessors
                || OneOrMoreWhileLoopProcessors));
        }
        
        // Since a while loop is started using standard processing rules, calling any further methods will 'lock' the system
        // To allow sequential events in a while loop, we ignore the while loop that started the event
        public System.Func<bool> WhileLoopProcessingRules(EventPage activePage)
        {
            // Return a lambda that evaluates the conditions when invoked
            return () => ((
                (activePage != null && activePage.IsWaiting)
                || settingsProfile.DialogueSystemInSession
                || settingsProfile.ActionKeyPressed
                || OneOrMoreExternalProcessors));
        }

        // updates all events in the events list
        // events are added to the list when their OnEnable is executed
        // events are also removed from the list when OnDisable is executed
        public void UpdateEvents()
        {
            eventMonitoring.Tasks?.Invoke();
            GetClosestEventToPlayer();

            // check if InSession Has changed
            if (InSession != lastInSession)
            {
                switch (InSession)
                {
                    case true:
                        OnBecameInSessionTrue?.Invoke();
                        break;
                    case false:
                        OnBecameInSessionFalse?.Invoke();
                        break;
                };
                lastInSession = InSession;
            }
        }

        // We want to remove any active triggers if the player has teleported
        public void RemoveAllActiveTriggers()
        {
            // called in a coroutine so that it waits a frame before updating
            StartCoroutine(RemoveAllActiveTriggersAfterTeleport());
        }

        // Sequentually iterates through each UnityEvent task, and waits for the IsProcessing to be false
        public IEnumerator SequentialUnityEvent(EventHandler handler, System.Func<bool> processCondition, System.Action updateMethod = null)
        {
            if (handler == null) yield break;
            UnityEvent unityEvent = handler.tasks;
            UnityEventBase eventBase = unityEvent;
            if (eventBase == null) yield break;

            //Debug.Log(unityEvent.GetPersistentEventCount());
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                string methodName = unityEvent.GetPersistentMethodName(i);
                UnityEngine.Object target = unityEvent.GetPersistentTarget(i);

                //Debug.Log(i);
                if (target == null) continue;

                MethodInfo safeMethod = null;
                var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                // Loop through all the methods
                for (int j = 0; j < methods.Length; j++)
                {
                    if (methods[j].Name == methodName)
                    {
                        // Log the parameter count of this method
                        //Debug.Log($"Method: {methods[j].Name} has {methods[j].GetParameters().Length} parameters.");

                        // Get valid method info with the expected parameters from the event base
                        MethodInfo methodInfo = UnityEventBase.GetValidMethodInfo(target, methodName, SD_EventWeaverCommons.GetParameterTypes(eventBase, i));

                        // Check if the parameters of the method match the expected method or if the method has no parameters
                        if (methods[j].GetParameters().Length == methodInfo.GetParameters().Length || methodInfo.GetParameters().Length == 0)
                        {
                            safeMethod = methodInfo;
                            //Debug.Log($"Found matching method: {safeMethod.Name} with {safeMethod.GetParameters().Length} parameters.");
                        }
                    }
                }


                object[] parameters = SD_EventWeaverCommons.GetParameterValues(eventBase, i, safeMethod);

                if (safeMethod != null && parameters != null)
                {
                    safeMethod.Invoke(target, parameters);
                }

                yield return IsProcessing(processCondition);
            }

            // Handle Runtime Listeners
            var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
            var persistentCalls = field?.GetValue(eventBase);

            if (persistentCalls != null)
            {
                var callsField = persistentCalls.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance);
                var calls = callsField?.GetValue(persistentCalls) as System.Collections.IList;

                if (calls != null)
                {
                    for (int j = 0; j < calls.Count; j++)
                    {
                        var delegateField = calls[j].GetType().GetField("Delegate", BindingFlags.NonPublic | BindingFlags.Instance);
                        var methodDelegate = delegateField?.GetValue(calls[j]) as System.Delegate;

                        if (methodDelegate != null)
                        {
                            var invokeList = methodDelegate.GetInvocationList();
                            foreach (var del in invokeList)
                            {
                                string methodName = del.Method.Name;
                                UnityEngine.Object target = del.Target as UnityEngine.Object;

                                if (target == null)
                                {
                                    Debug.LogWarning($"Method {methodName} has no target (likely a static method). Skipping.");
                                    continue;
                                }

                                MethodInfo safeMethod = null;
                                var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                                // Loop through all the methods
                                for (int k = 0; k < methods.Length; k++)
                                {
                                    if (methods[k].Name == methodName)
                                    {
                                        // Log the parameter count of this method
                                        //Debug.Log($"Method: {methods[k].Name} has {methods[k].GetParameters().Length} parameters.");

                                        // Get valid method info with the expected parameters from the event base
                                        MethodInfo methodInfo = UnityEventBase.GetValidMethodInfo(target, methodName, SD_EventWeaverCommons.GetParameterTypes(eventBase, j));

                                        // Check if the parameters of the method match the expected method or if the method has no parameters
                                        if (methods[k].GetParameters().Length == methodInfo.GetParameters().Length || methodInfo.GetParameters().Length == 0)
                                        {
                                            safeMethod = methodInfo;
                                            //Debug.Log($"Found matching method: {safeMethod.Name} with {safeMethod.GetParameters().Length} parameters.");
                                        }
                                    }
                                }

                                //Debug.Log($"Invoking runtime method: {del.Method.Name} on {target}");
                                object[] parameters = SD_EventWeaverCommons.GetParameterValues(eventBase, j, safeMethod);

                                Debug.Log(del.Method.Name);

                                safeMethod.Invoke(target, parameters);
                                yield return IsProcessing(processCondition);
                            }
                        }
                    }
                }
            }

            updateMethod?.Invoke();
            handler.inProcess = null;
        }

        // finds a variable in the variables list
        public Variable GetVariable(SD_Variable searchFor) => Variables.FirstOrDefault(v => v.variable == searchFor);
        public Variable GetVariable(SD_Variable searchFor, List<Variable> list) => list.FirstOrDefault(v => v.variable == searchFor);        
        public Variable GetVariable(string searchFor) => Variables.FirstOrDefault(v => v.variable.name == searchFor);
        

        // finds a stateSwitch in the switches list
        public StateSwitch GetStateSwitch(SD_StateSwitch searchFor) => Switches.FirstOrDefault(v => v.stateSwitch == searchFor);
        public StateSwitch GetStateSwitch(SD_StateSwitch searchFor, List<StateSwitch> list) => list.FirstOrDefault(v => v.stateSwitch == searchFor);
        public StateSwitch GetStateSwitch(string searchFor) => Switches.FirstOrDefault(v => v.stateSwitch.name == searchFor);

        public void SetStateSwitch(StateSwitch stateSwitch, bool value)
        {
            stateSwitch.state = value;
            UpdateProperties();
        }

        public void SetVariable(Variable variable, float value)
        {
            variable.value = value;
            UpdateProperties();
        }
        
        public void ResetEventManager()
        {
            EventMonitoring.EventWeaverObjects.Clear();
            EventMonitoring.ExternalProcessors.Clear();
            EventMonitoring.ActiveTriggers.Clear();
            EventMonitoring.Tasks.RemoveAllListeners();
            EventMonitoring.WhileLoopProcessors.Clear();
            EventMonitoring.EventsInSession.Clear();
        }


        #endregion

        // gets all switches / variables in the project and adds them to the lists
#if UNITY_EDITOR


        [ContextMenu("Log Event Manager Stats")]
        public void LogEventManagerStats()
        {
            Debug.Log(
                $"EventWeaverObjects.Count: {EventMonitoring.EventWeaverObjects.Count}\n" +
                $"ExternalProcessors.Count: {EventMonitoring.ExternalProcessors.Count}\n" +
                $"ActiveTriggers.Count: {EventMonitoring.ActiveTriggers.Count}\n" +
                $"Tasks.GetPersistentEventCount: {EventMonitoring.Tasks.GetPersistentEventCount()}\n" +
                $"WhileLoopProcessors.Count: {EventMonitoring.WhileLoopProcessors.Count}\n" +
                $"EventsInSession.Count: {EventMonitoring.EventsInSession.Count}\n"
            );

            foreach (var item in EventMonitoring.ExternalProcessors)
            {
                Debug.Log(item);
            }
        }

        [ContextMenu("Load All State Switches in Project")]
        public void GetAllStateSwitches()
        {
            // Find all SD_StateSwitch assets in the project
            string[] guids = AssetDatabase.FindAssets("t:SD_StateSwitch");

            // Create a temporary list for the switches
            List<StateSwitch> tempSwitches = new List<StateSwitch>();
            HashSet<string> existingNames = new HashSet<string>(); // Track duplicates by name

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SD_StateSwitch asset = AssetDatabase.LoadAssetAtPath<SD_StateSwitch>(path);

                if (asset != null && existingNames.Add(asset.name)) // Ensure unique names
                {
                    // Create a new StateSwitch and populate its data
                    StateSwitch stateSwitch = new StateSwitch(asset, false);
                    UpdateSwitchInspectorName(stateSwitch);
                    tempSwitches.Add(stateSwitch);
                }
            }

            // Assign the array
            switches = tempSwitches.ToList();

            Debug.Log($"Loaded {switches.Count} unique SD_StateSwitch assets.");
        }

        [ContextMenu("Load All Variables in Project")]
        public void GetAllVariables()
        {
            // Find all SD_Variable assets in the project
            string[] guids = AssetDatabase.FindAssets("t:SD_Variable");

            // Create a temporary list for the switches
            List<Variable> tempVars = new List<Variable>();
            HashSet<string> existingNames = new HashSet<string>(); // Track duplicates by name

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SD_Variable asset = AssetDatabase.LoadAssetAtPath<SD_Variable>(path);

                if (asset != null && existingNames.Add(asset.name)) // Ensure unique names
                {
                    // Create a new StateSwitch and populate its data
                    Variable variable = new Variable(asset, 0);
                    UpdateVariableInspectorName(variable);
                    tempVars.Add(variable);
                }
            }

            // Assign the array
            variables = tempVars.ToList();

            Debug.Log($"Loaded {variables.Count} unique SD_StateSwitch assets.");
        }
#endif

    }

    [System.Serializable]
    public class StateSwitch
    {
        [HideInInspector] public string name;
        [HideInInspector] public string uniqueID = "";// for saving the scriptable object

        public SD_StateSwitch stateSwitch;
        public bool state;

        public StateSwitch() { }

        public StateSwitch(SD_StateSwitch _stateSwitch, bool _state)
        {
            stateSwitch = _stateSwitch;
            state = _state;
            uniqueID = stateSwitch.UniqueID;
        }

        public StateSwitch Clone() => new StateSwitch(stateSwitch, state);

    }

    [System.Serializable]
    public class Variable
    {
        [HideInInspector] public string name;
        [HideInInspector] public string uniqueID = "";// for saving the scriptable object

        public SD_Variable variable;
        public float value;

        public Variable() { }

        public Variable(SD_Variable _variable, float _value)
        {
            variable = _variable;
            value = _value;
            uniqueID = variable.UniqueID;
        }

        public Variable Clone() => new (variable, value);
    }

    [System.Serializable]
    public class EventMonitoring
    {
        // Events being run by the Event Master
        public UnityEvent Tasks;

        // Events that are in session (Interacted with / running)
        // useful for auto locking player movement during an event
        // individual events can ignore this rule
        public List<SD_Event> EventsInSession = new();

        // A list of all the pottential Transform Targets for cross referencing Transform locations
        [Tooltip("All Objects that can be referenced from event weaver interface. Use a proper hash for easy lookup")]
        public List<SD_EventWeaverObject> EventWeaverObjects = new();

        // Triggers that are currently activated by the player 
        public List<SD_Event> ActiveTriggers = new();

        // External processors like waits conditions

        public List<string> ExternalProcessors = new List<string>();

        // While loops are handled differently
        public List<string> WhileLoopProcessors = new List<string>();
    }

}