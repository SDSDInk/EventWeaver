using UnityEngine;

namespace SDSDInk.EventWeaver
{
    [CreateAssetMenu(fileName = "Variable", menuName = "EventWeaver/Variable")]
    public class SD_Variable : SD_SaveableSO
    {
#pragma warning disable 
        [SerializeField, TextArea(1,10)] private string description = "";
#pragma warning enable

        [ContextMenu("Unrecord Variable From Player Prefs")]
        public void UnrecordFromPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(this.name);
        }
    }

}