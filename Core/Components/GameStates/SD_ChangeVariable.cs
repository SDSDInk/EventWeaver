using UnityEngine;

namespace SDSDInk.EventWeaver{

    enum VariableUpdateType { Set, Adjust, Multiply, Divide }

    public class SD_ChangeVariable : MonoBehaviour
    {
        [SerializeField, Tooltip("the variable to be effected")] private SD_Variable variable;
        [SerializeField, Tooltip("how will this variable be updated?")] private VariableUpdateType variableUpdateType;
        [SerializeField, Tooltip("the value that effects the update type")] private float value;
        [SerializeField, Tooltip("is this variable effected by Time.DeltaTime?")] private bool multiplyByTimeDeltaTime;
        [SerializeField, Tooltip("is this variable effected by Time.FixedDeltaTime?")] private bool multiplyByTimeFixedDeltaTime;

        SD_EventManager EventManager { get => SD_EventManager.Instance; }

        public void UpdateVariable()
        {
            float val = value;
            if (multiplyByTimeDeltaTime)            
                val = value * Time.deltaTime;            
            else if (multiplyByTimeFixedDeltaTime)            
                val = value * Time.fixedDeltaTime;

            Variable _var = EventManager.GetVariable(variable);
            switch (variableUpdateType)
            {
                case VariableUpdateType.Set:
                    EventManager.SetVariable(_var, val);
                    break;
                case VariableUpdateType.Adjust:
                    float newValAdd = _var.value + val;
                    EventManager.SetVariable(_var, newValAdd);
                    break;
                case VariableUpdateType.Multiply:
                    float newValMul = _var.value * val;
                    EventManager.SetVariable(_var, newValMul);
                    break;
                case VariableUpdateType.Divide:
                    float newValDiv = _var.value / val;
                    EventManager.SetVariable(_var, newValDiv);
                    break;
            }
            //SD_EventWeaverSettings.RecordVariableData(_var);
            //EventManager.UpdateVariableInspectorName(_var);
        }
    }
}