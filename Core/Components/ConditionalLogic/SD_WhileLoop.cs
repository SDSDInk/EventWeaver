using System.Collections;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    public class SD_WhileLoop : MonoBehaviour
    {
        [SerializeField] private Preconditions loopCondition;

        [SerializeField] private EventHandler WhileLooping, AfterWait;

        private SD_Event ev;

        public bool IsLooping { get => isLooping; }
        bool isLooping = false;

        private void Awake() => ev = GetComponent<SD_Event>();       

        public void Evaluate()
        {
            SD_EventManager.Instance.EventMonitoring.WhileLoopProcessors.Add(GetInstanceID().ToString());
            StartCoroutine(WhileStateSwitch());
        }

        IEnumerator WhileStateSwitch()
        {
            isLooping = true;
            while (SD_Preconditions.EvaluatePreconditions(loopCondition))
            {
                if (ev != null && WhileLooping.processType == EventProcessingType.Sequential)
                    yield return StartCoroutine(
                        SD_EventManager.Instance.SequentialUnityEvent(
                        WhileLooping,
                        SD_EventManager.Instance.WhileLoopProcessingRules(ev.ActivePage),
                        null
                    ));
                else
                    WhileLooping.tasks?.Invoke();

                yield return new WaitForEndOfFrame();
            }
            isLooping = false;

            if (ev != null && AfterWait.processType == EventProcessingType.Sequential)
                yield return StartCoroutine(
                    SD_EventManager.Instance.SequentialUnityEvent(
                    AfterWait,  
                        SD_EventManager.Instance.WhileLoopProcessingRules(ev.ActivePage),
                    null
                ));
            else
                AfterWait.tasks?.Invoke();

            SD_EventManager.Instance.EventMonitoring.WhileLoopProcessors.Remove(GetInstanceID().ToString());
        }
    }
    
}