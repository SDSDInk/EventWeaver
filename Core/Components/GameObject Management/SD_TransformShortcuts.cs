using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDSDInk.EventWeaver{
    public class SD_TransformShortcuts : MonoBehaviour
    {
        SD_EventManager eventManager;

        private void Start() => eventManager = SD_EventManager.Instance;

        // Where target is the thing we are moving, and the parameters are where its being moved
        public Transform Target { set => target = value; }
        private Transform target;

        // Sets the player defined in the Event Manager as the TransformTarget
        public void TargetIs_Player() => target = eventManager.PlayerGameObject.transform; 

        // Looks through all TransformTargets in the EventManager for one with matching hash
        public void TargetIs_TransformTargetByHash(string hash) => target = eventManager.EventMonitoring.EventWeaverObjects.FirstOrDefault(t => t.RefHash == hash).transform; 

        // Looks through all Events In Session for one with matching name value
        public void TargetIs_TransformTargetByEventName(string name) => target = eventManager.EventMonitoring.EventsInSession.FirstOrDefault(t => t.name == name).transform; 

        // Looks through all Events In Session for one with matching name value
        public void TargetIs_Transform(Transform _target) =>  target = _target; 

        // for memorizing TransformTarget location
        private Vector3 memorizedLocation;

        public void SetLayerByString(string newLayerName)
        {
            if (target == null)
                target = transform;
            int newLayer = LayerMask.NameToLayer(newLayerName);
            if (newLayer == -1)
            {
                Debug.LogWarning($"Layer '{newLayerName}' does not exist.");
                return;
            }
            target.gameObject.layer = newLayer;
        }
        public void SetLayerByInt(int newLayer)
        {
            if (target == null)
                target = transform;
            target.gameObject.layer = newLayer;
        }

        private void UpdatePlayerSystemsAfterTeleport()
        {
            if (eventManager.PlayerGameObject == null)
                return;

            if (target == eventManager.PlayerGameObject.transform)
            {
                eventManager.RemoveAllActiveTriggers();
            }
        }

        public void SetTargetLocalTransformPositionZero() { if (target != null) target.localPosition = Vector3.zero; }
        public void SetTargetTransformPositionZero() { if (target != null) target.localPosition = Vector3.zero; }

        #region Memorize / Recall Position
        // Memorize The TransformTargets World Position
        public void MemorizeTargetWorldPosition()
        {
            if (target != null)
                memorizedLocation = target.position;
            UpdatePlayerSystemsAfterTeleport();
        }

        // Memorize The TransformTargets Local Position
        public void MemorizeTargetLocalPosition()
        {
            if (target != null)
                memorizedLocation = target.localPosition;
            UpdatePlayerSystemsAfterTeleport();
        }

        // Recall TransformTargets World Position to Memorized Location
        public void RecallTargetToWorldPosition()
        {
            if (target != null)
                target.position = memorizedLocation;
            UpdatePlayerSystemsAfterTeleport();
        }

        // Recall TransformTargets Local Position to Memorized local Location
        public void RecallTargetToLocalPosition()
        {
            if (target != null)
                target.localPosition = memorizedLocation;
            UpdatePlayerSystemsAfterTeleport();
        }
        #endregion

        #region MoveTargetTo: Transform
        // moves the TransformTarget to a selected transforms world position
        public void MoveTargetToWorldPosition(Transform transform)
        {
            if (target != null)
            {
                target.position = transform.position;
                UpdatePlayerSystemsAfterTeleport();
            }
        }

        // moves the TransformTarget to a selected transforms local position
        public void MoveTargetToLocalPosition(Transform transform)
        {
            if (target != null)
            {
                target.localPosition = transform.localPosition;
                UpdatePlayerSystemsAfterTeleport();
            }
        }
        #endregion

        #region MoveTargetTo: TransformTarget By Hash
        // moves the TransformTarget to a selected transforms world position
        public void MoveTargetToWorldPosition(string transformTargetHash)
        {
            SD_EventWeaverObject obj = eventManager.EventMonitoring.EventWeaverObjects.FirstOrDefault(t => t.RefHash == transformTargetHash);
            if (obj == null)
                return;

            Transform moveToTarget = obj.transform;
            if (target != null && moveToTarget != null)
                target.position = moveToTarget.position;
            UpdatePlayerSystemsAfterTeleport();
        }

        // moves the TransformTarget to a selected transforms local position
        public void MoveTargetToLocalPosition(string transformTargetHash)
        {
            SD_EventWeaverObject obj = eventManager.EventMonitoring.EventWeaverObjects.FirstOrDefault(t => t.RefHash == transformTargetHash);
            if (obj == null)
                return;

            Transform moveToTarget = obj.transform;
            if (target != null && moveToTarget != null)
                target.localPosition = moveToTarget.localPosition;
            UpdatePlayerSystemsAfterTeleport();
        }
        #endregion

        // these wont work in the inspector without something like odin
        #region MoveTargetTo: Vector2/3
        // moves the Transform target to the input world position
        public void MoveTargetToWorldPosition(Vector2 position)
        {
            if (target != null)
                target.position = position;
            UpdatePlayerSystemsAfterTeleport();
        }

        // moves the Transform target to the input local position
        public void MoveTargetToLocalPosition(Vector2 position)
        {
            if (target != null)
                target.localPosition = position;
            UpdatePlayerSystemsAfterTeleport();
        }

        // moves the Transform target to the input world position
        public void MoveTargetToWorldPosition(Vector3 position)
        {
            if (target != null)
                target.position = position;
            UpdatePlayerSystemsAfterTeleport();
        }

        // moves the Transform target to the input local position
        public void MoveTargetToLocalPosition(Vector3 position)
        {
            if (target != null)
                target.localPosition = position;
            UpdatePlayerSystemsAfterTeleport();
        }
        #endregion
    }
}