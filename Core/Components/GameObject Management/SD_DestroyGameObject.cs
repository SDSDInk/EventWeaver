using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    public class SD_DestroyGameObject : MonoBehaviour
    {
        [SerializeField] bool destroyThisGOOverTime;
        [SerializeField] float timeUntilDestroyed;
        bool schedualedToDestroy = false;
        DateTime timeAtDisable;

        private void OnEnable()
        {
            if (destroyThisGOOverTime)
            {
                if (schedualedToDestroy)
                { 
                    TimeSpan difference = DateTime.Now - timeAtDisable;
                    // already schedualed but was disabled, so account for the time already waited and start from there                
                    timeUntilDestroyed = timeUntilDestroyed - (float)difference.TotalSeconds;
                    if(timeUntilDestroyed <= 0)
                        timeUntilDestroyed = 0;
                }
                
                schedualedToDestroy = true;
                Invoke("DestroyThisGameObject", timeUntilDestroyed);
            }
        }

        private void OnDisable()
        {
            timeAtDisable = DateTime.Now; // otherwise it wouldhave been destroyed
            CancelInvoke("DestroyThisGameObject");
        }

        public void DestroyThisGameObject()
        {
            schedualedToDestroy = false;
            Destroy(gameObject);
        }

        public void DestroyGameObject(GameObject _gameobject)
        {
            Destroy(_gameobject);
        }
    }
}