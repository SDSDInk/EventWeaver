using UnityEngine;

namespace SDSDInk.EventWeaver {
    [CreateAssetMenu(fileName = "State Switch", menuName = "EventWeaver/State Switch")]
    public class SD_StateSwitch : SD_SaveableSO
    {
#pragma warning disable 
        [SerializeField, TextArea(1,10)] private string description = "";
#pragma warning enable

        [ContextMenu("Unrecord StateSwitch From Player Prefs")]
        public void UnrecordFromPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(this.name);
        }

    }

}