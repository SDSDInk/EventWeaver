using System.Collections;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    public class SD_IfStatement : MonoBehaviour
    {
        private SD_Event ev;

        [SerializeField] private bool runOnEnable;
        [SerializeField] private Preconditions preconditions;

        [SerializeField] private EventHandler IfTrue, Else;

        string id = "";

        private void Awake()
        {
            // make its own event instance, even if there is one already on the object
            ev = gameObject.AddComponent<SD_Event>();
            id = GetInstanceID().ToString();
        }

        private void OnEnable()
        {
            if (runOnEnable)
                Evaluate();
        }

        private void OnDisable()
        {
            if (IfTrue.processType == EventProcessingType.Sequential || Else.processType == EventProcessingType.Sequential)
            {
                IfComplete();
            }
        }

        public void Evaluate()
        {
            StartCoroutine(RunIfStatement());
        }

        IEnumerator RunIfStatement()
        {
            if (SD_Preconditions.EvaluatePreconditions(preconditions))
            {
                if (IfTrue.processType == EventProcessingType.Sequential)
                {
                    SD_EventManager.Instance.EventMonitoring.WhileLoopProcessors.Add(id);
                    yield return StartCoroutine(
                        SD_EventManager.Instance.SequentialUnityEvent(
                        IfTrue,
                        SD_EventManager.Instance.WhileLoopProcessingRules(ev.ActivePage),
                        ()=>
                        {
                            ev.UpdateEvent();
                            IfComplete();
                        }
                    ));
                }
                else
                    IfTrue.tasks?.Invoke();
            }
            else
            {
                if (Else.processType == EventProcessingType.Sequential)
                {
                    SD_EventManager.Instance.EventMonitoring.WhileLoopProcessors.Add(id);
                    yield return StartCoroutine(
                        SD_EventManager.Instance.SequentialUnityEvent(
                        Else,
                        SD_EventManager.Instance.WhileLoopProcessingRules(ev.ActivePage),
                        () =>
                        {
                            ev.UpdateEvent();
                            IfComplete();
                        }
                    ));
                }
                else
                    Else.tasks?.Invoke();
            }
        }

        void IfComplete()
        {
            StopAllCoroutines();
            SD_EventManager.Instance.EventMonitoring.WhileLoopProcessors.Remove(id);
            ev.RemoveFromSession();
        }
    }
    
}