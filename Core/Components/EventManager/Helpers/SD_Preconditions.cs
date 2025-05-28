using System.Linq;
using UnityEngine;

namespace SDSDInk.EventWeaver {

    public static class SD_Preconditions {
        
        public static bool EvaluateVariable(float evaluatedValue, ComparisonTypes comparisonType, float value)
        {
            ComparisonTypes ct = comparisonType;
            if (ct == ComparisonTypes.EqualTo)
                return evaluatedValue == value;
            else if (ct == ComparisonTypes.GreaterThan)
                return evaluatedValue > value;
            else if (ct == ComparisonTypes.LessThan)
                return evaluatedValue < value;
            else if (ct == ComparisonTypes.NotEqualTo)
                return evaluatedValue != value;
            else if (ct == ComparisonTypes.LessThanOrEqualTo)
                return evaluatedValue <= value;
            else if (ct == ComparisonTypes.GreaterThanOrEqualTo)            
                return evaluatedValue >= value;
            return false;
        }

        public static bool EvaluatePreconditions(Preconditions preconditions)
        {
            ////////////////////////////////////////////////////////////////////////////
            // Evaluate State Switches
            var switches = preconditions.switches;
            bool[] _switches = new bool[preconditions.switches.Length];
            if (switches.Length > 0)
                for (int j = 0; j < switches.Length; j++)
                {
                    if (SD_EventManager.Instance.GetStateSwitch(switches[j].stateSwitch).state == switches[j].state)
                        _switches[j] = true;
                }

            ////////////////////////////////////////////////////////////////////////////
            // Evaluate Variables
            var vars = preconditions.variables;
            bool[] _variables = new bool[preconditions.variables.Length];
            if (vars.Length > 0)
                for (int j = 0; j < vars.Length; j++)
                {
                    if (EvaluateVariable(SD_EventManager.Instance.GetVariable(vars[j].variable).value, vars[j].comparisonType, vars[j].value))
                        _variables[j] = true;
                }

            ////////////////////////////////////////////////////////////////////////////
            // Evalutate Time Of Day conditions
            var todC = preconditions.timeOfDayConditions;
            bool[] _timeOfDayConditions = new bool[preconditions.timeOfDayConditions.Length];
            if (todC.Length > 0)
                for (int j = 0; j < todC.Length; j++)
                {
                    if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.TimeOfDay, todC[j].comparisonType, todC[j].timeIs))
                        _timeOfDayConditions[j] = true;
                }

            ////////////////////////////////////////////////////////////////////////////
            // Evaluate day related conditions
            var drC = preconditions.dayRelatedConditions;
            bool[] _dayRelatedConditions = new bool[preconditions.dayRelatedConditions.Length];
            if (drC.Length > 0)
                for (int j = 0; j < drC.Length; j++)
                {
                    switch (drC[j].dayRelatedConditionType)
                    {
                        case DayRelatedConditionType.IfDaysSurvivedIs:
                            if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.DaysSurvived, drC[j].comparisonType, drC[j].value))
                                _dayRelatedConditions[j] = true;
                            break;
                        case DayRelatedConditionType.IfDayOfWeekIs:
                            if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.CurrentDayOfTheWeek(), drC[j].comparisonType, drC[j].value))
                                _dayRelatedConditions[j] = true;
                            break;
                        case DayRelatedConditionType.IfDayOfMonthIs:
                            if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.CurrentDayOfTheMonth(), drC[j].comparisonType, drC[j].value))
                                _dayRelatedConditions[j] = true;
                            break;
                        case DayRelatedConditionType.IfMonthIs:
                            if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.CurrentMonth(), drC[j].comparisonType, drC[j].value))
                                _dayRelatedConditions[j] = true;
                            break;
                        case DayRelatedConditionType.IfYearIs:
                            if (EvaluateVariable(SD_EventManager.Instance.settingsProfile.CurrentYear(), drC[j].comparisonType, drC[j].value))
                                _dayRelatedConditions[j] = true;
                            break;
                    }
                }

            ////////////////////////////////////////////////////////////////////////////
            // Evaluate State Switches
            var goc = preconditions.gameObjectConditions;
            bool[] _gameObjectConditions = new bool[preconditions.gameObjectConditions.Length];
            if (_gameObjectConditions.Length > 0)
                for (int j = 0; j < _gameObjectConditions.Length; j++)
                {
                    _gameObjectConditions[j] = goc[j].gameObject.activeSelf;
                }

            bool isSupposedToBeInSession = preconditions.eventWeaverInSession ? SD_EventManager.Instance.InSession : true;

            ////////////////////////////////////////////////////////////////////////////
            return
                isSupposedToBeInSession &&
                // All Switch conditions (0 Conditions = true)    
                _switches.All(match => match) &&
                // All Variable conditions (0 Conditions = true)
                _variables.All(match => match) &&
                // All time of Day conditions (0 Conditions = true)
                _timeOfDayConditions.All(match => match) &&
                // All day related conditions (0 Conditions = true)
                _dayRelatedConditions.All(match => match) &&
                // All GameObject conditions (0 Conditions = true)
                _gameObjectConditions.All(match => match);
        }
    }

    public enum ComparisonTypes { EqualTo, GreaterThanOrEqualTo, LessThanOrEqualTo, GreaterThan, LessThan, NotEqualTo }

    [System.Serializable]
    public class Preconditions
    {
        public bool eventWeaverInSession = false;
        public SwitchCondition[] switches;
        public VariableConditon[] variables;
        public TimeOfDayCondition[] timeOfDayConditions;
        public DayRelatedCondition[] dayRelatedConditions;
        public GameObjectCondition[] gameObjectConditions;
    }

    [System.Serializable]
    public class SwitchCondition
    {
        public SD_StateSwitch stateSwitch;
        public bool state = false;
    }

    [System.Serializable]
    public class VariableConditon
    {
        public SD_Variable variable;
        public ComparisonTypes comparisonType;
        public float value;
    }

    [System.Serializable]
    public class TimeOfDayCondition
    {
        public ComparisonTypes comparisonType;
        public float timeIs;
    }

    [System.Serializable]
    public class DayRelatedCondition
    {
        public DayRelatedConditionType dayRelatedConditionType;
        public ComparisonTypes comparisonType;
        public float value;
    }

    [System.Serializable]
    public class GameObjectCondition
    {
        public GameObject gameObject;
        public bool state = false;
    }

}