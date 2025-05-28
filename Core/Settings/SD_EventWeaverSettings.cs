using System.Linq;
using UnityEngine;

namespace SDSDInk.EventWeaver {

    public class SD_EventWeaverSettings : MonoBehaviour
    {
        // saves the variable data to whereEver things are saved
        public virtual void RecordVariableData(Variable v)
        {
        }

        // saves the state switch data to whereever things are saved
        public virtual void RecordSwitchData(StateSwitch s)
        {
        }

        // the action key from your input settings
        public virtual bool ActionKeyPressed
        {
            get
            {
                return Input.GetKeyDown(KeyCode.Tab);
            }
        }

        // is your dialogue system in session (displaying text?)
        public virtual bool DialogueSystemInSession
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// In game Time settings
        /// </summary>
        #region Time Settings
        // how does your project know what time of day it is?
        public virtual float TimeOfDay
        {
            get
            {
                return -1;
            }
        }

        // Get the Time Of Day In Text Format
        public virtual string GetTimeText(float _timeOfDay = -1) => "";
        #endregion

        /// <summary>
        /// In game calendar settings
        /// </summary>
        #region InGameCalendar

        // how does your project know how many days the hero has been in game or survived?
        public virtual float DaysSurvived
        {
            get
            {
                return 0;
            }
        }

        // how does your project know what day of the week it is? 
        public virtual float CurrentDayOfTheWeek(Vector2Int overrideDate = default)
        {
            return 0;
        }

        // how does your project know what the current day of the month it is
        public virtual float CurrentDayOfTheMonth(Vector2Int overrideDate = default)
        {
            return 0;
        }

        // how does your project know what the current month is
        public virtual float CurrentMonth(Vector2Int overrideDate = default)
        {
            if (SD_GameMaster.Instance.DayNightManager != null)
                return SD_GameMaster.Instance.DayNightManager.GetCurrentMonth(overrideDate);
            return 0;
        }

        // how does your project know what the current month is
        public virtual float CurrentYear(Vector2Int overrideDate = default)
        {
            return 0;
        }

        public virtual string GetDayMonthYear(bool showCurrentCalendarDay = true, bool showWeekdayNames = true, bool showMonthNames = true, bool showYears = true)
        {
            return "No Date Implemented";
        }

        public virtual string GetDayMonthYear(Vector2Int overrideDate = default, bool shortForm = false)
        {
            return "No Date Implemented";
        }

        // user Displays default date;
        public virtual string GetDayMonthYear()
        {
            return "No Date Implemented";
        }

        // user Displays default date in short form;
        public virtual string GetDayMonthYearShort()
        {
            return "No Date Implemented";
        }

        // user has full control over Date Display
        public virtual string GetDayMonthYear(Vector2Int overrideDate = default, bool showCurrentCalendarDay = true, bool showWeekdayNames = true, bool showMonthNames = true, bool showYears = true, bool shortForm = false)
        {
            return "No Date Implemented";
        }

        #endregion

    }
}