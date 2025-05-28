using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace SDSDInk.EventWeaver{

    enum StateSwitchUpdateType { Toggle, True, False }

    public class SD_ChangeStateSwitch : MonoBehaviour
    {
        [SerializeField, Tooltip("the stateswitch to be effected")] private SD_StateSwitch stateSwitch;
        [SerializeField, Tooltip("how will this switch be updated?")] private StateSwitchUpdateType stateSwitchUpdateType;

        SD_EventManager EventManager { get => SD_EventManager.Instance; }

        public void UpdateStateSwitch()
        {
            StateSwitch _sta = EventManager.GetStateSwitch(stateSwitch);
            switch (stateSwitchUpdateType)
            {
                case StateSwitchUpdateType.Toggle:
                    EventManager.SetStateSwitch(_sta, !_sta.state);
                    break;
                case StateSwitchUpdateType.True:
                    EventManager.SetStateSwitch(_sta, true);
                    break;
                case StateSwitchUpdateType.False:
                    EventManager.SetStateSwitch(_sta, false);
                    break;
            }
            //SD_EventManager.Instance.settingsProfile.RecordSwitchData(_sta);
            //EventManager.UpdateSwitchInspectorName(_sta);
        }
    }
}