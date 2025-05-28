using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDSDInk.EventWeaver{
    public class SD_InstantiateGameObject : MonoBehaviour
    {
        enum TransformType { Vector3, Transform }
        enum Scope { World, Local }

        [Header("Settings")]
        [SerializeField, Tooltip("The prefab reference that will be instantiated")] private GameObject prefabToInstantiate;
        // Where will the gameobject be parented to?
        [SerializeField, Tooltip("Where will the instantiated prefab be parented to? No Parent will result in global placement")] private Transform instantiateParent;

        [Header("Position")]
        [SerializeField, Tooltip("Is this gameobject placed via Vector3, or another objects transform position")] private TransformType transformPosition;
        [ShowIf("transformPosition", (int)TransformType.Vector3)]
        [SerializeField, Tooltip("Transform scope (Local or World)")] private Scope positionScope;

        // Location Change Settings
        [ShowIf("transformPosition", (int)TransformType.Vector3)]
        [SerializeField, Tooltip("The objects spawn location in the specified scope")] private Vector3 instantiateLocation = Vector3.zero;
        [ShowIf("transformPosition", (int)TransformType.Transform)]
        [SerializeField, Tooltip("The objects spawn location in the specified scope")] private Transform positionTransform;

        // X-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the X position.")]
        [SerializeField] private Vector2 xLocationThreshold = new Vector2(0, 0);

        // Y-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Y position.")]
        [SerializeField] private Vector2 yLocationThreshold = new Vector2(0, 0);

        // Z-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Z position.")]
        [SerializeField] private Vector2 zLocationThreshold = new Vector2(0, 0);

        [Header("Rotation")]
        [SerializeField, Tooltip("Is this gameobject placed via Vector3, or another objects transform position")] private TransformType transformRotation;
        [ShowIf("transformRotation", (int)TransformType.Vector3)]
        [SerializeField, Tooltip("Rotation scope (Local or World)")] private Scope rotationScope;

        [ShowIf("transformRotation", (int)TransformType.Transform)]
        [SerializeField, Tooltip("The objects spawn rotation in the specified scope")] private Transform rotationTransform;
        [ShowIf("transformRotation", (int)TransformType.Vector3)]
        [SerializeField, Tooltip("The objects spawn rotation in the specified scope")] private Vector3 instantiateRotation = Vector3.zero;

        // X-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the X Rotation.")]
        [SerializeField] private Vector2 xRotationThreshold = new Vector2(0, 0);

        // Y-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Y Rotation.")]
        [SerializeField] private Vector2 yRotationThreshold = new Vector2(0, 0);

        // Z-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Z Rotation.")]
        [SerializeField] private Vector2 zRotationThreshold = new Vector2(0, 0);

        [Header("Scale")]
        [SerializeField, Tooltip("")] private TransformType transformScale;
        [ShowIf("transformScale", (int)TransformType.Vector3)]
        [SerializeField, Tooltip("The objects spawn scale")] private Vector3 instantiateScale = Vector3.one;

        [ShowIf("transformScale", (int)TransformType.Transform)]
        [SerializeField, Tooltip("The objects spawn scale")] private Transform scaleTransform;

        // X-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the X Scale.")]
        [SerializeField] private Vector2 xScaleThreshold = new Vector2(0, 0);

        // Y-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Y Scale.")]
        [SerializeField] private Vector2 yScaleThreshold = new Vector2(0, 0);

        // Z-axis randomization range
        [Tooltip("The minimum and maximum random offset applied to the Z Scale.")]
        [SerializeField] private Vector2 zScaleThreshold = new Vector2(0, 0);

        public GameObject LastInstantiated { get => lastInstantiated; }
        GameObject lastInstantiated;

        public void InstantiateGameObject()
        {
            Debug.Log("Dropping");

            GameObject newObj = Instantiate(prefabToInstantiate, instantiateParent != null ? instantiateParent : null);
            if (newObj == null)
                return;

            Vector3 locationOffset = new Vector3(
                Random.Range(xLocationThreshold.x, xLocationThreshold.y), 
                Random.Range(yLocationThreshold.x, yLocationThreshold.y), 
                Random.Range(zLocationThreshold.x, zLocationThreshold.y)
            );

            Vector3 rotationOffset = new Vector3(
                Random.Range(xRotationThreshold.x, xRotationThreshold.y),
                Random.Range(yRotationThreshold.x, yRotationThreshold.y),
                Random.Range(zRotationThreshold.x, zRotationThreshold.y)
            );

            Vector3 scaleOffset = new Vector3(
                Random.Range(xScaleThreshold.x, xScaleThreshold.y),
                Random.Range(yScaleThreshold.x, yScaleThreshold.y),
                Random.Range(zScaleThreshold.x, zScaleThreshold.y)
            );

            if (transformPosition == TransformType.Vector3)
            {
                if (positionScope == Scope.World)
                    newObj.transform.position = instantiateLocation + locationOffset;
                else
                    newObj.transform.localPosition = instantiateLocation + locationOffset;
            }

            else if(positionTransform != null)
            {
                if (positionScope == Scope.World)
                    newObj.transform.position = positionTransform.transform.position + locationOffset;
                else
                    newObj.transform.localPosition = positionTransform.transform.localPosition + locationOffset;
            }

            if (transformRotation == TransformType.Vector3)
            {
                if (rotationScope == Scope.World)
                    newObj.transform.rotation = Quaternion.Euler(
                        instantiateRotation.x + rotationOffset.x, 
                        instantiateRotation.y + rotationOffset.y, 
                        instantiateRotation.z + rotationOffset.z
                    );
                else
                    newObj.transform.localRotation = Quaternion.Euler(
                        instantiateRotation.x + rotationOffset.x,
                        instantiateRotation.y + rotationOffset.y,
                        instantiateRotation.z + rotationOffset.z
                    );
            }
            else if(rotationTransform != null)
            {

                if (rotationScope == Scope.World)
                    newObj.transform.rotation = Quaternion.Euler(
                        rotationTransform.rotation.x + rotationOffset.x,
                        rotationTransform.rotation.y + rotationOffset.y,
                        rotationTransform.rotation.z + rotationOffset.z
                    );
                else
                    newObj.transform.localRotation = Quaternion.Euler(
                        rotationTransform.localRotation.x + rotationOffset.x,
                        rotationTransform.localRotation.y + rotationOffset.y,
                        rotationTransform.localRotation.z + rotationOffset.z
                    );
            }

            if (transformScale == TransformType.Vector3)
            {
                newObj.transform.localScale = instantiateScale + scaleOffset;
            }
            else
            {
                if (scaleTransform != null)
                    newObj.transform.localScale = scaleTransform.transform.localScale + scaleOffset;
            }
        }

        private void OnDrawGizmos()
        {
            // Define min and max offsets
            Vector2 minOffset = new Vector2(xLocationThreshold.x, yLocationThreshold.x);
            Vector2 maxOffset = new Vector2(xLocationThreshold.y, yLocationThreshold.y);

            // Calculate width and height
            float width = Mathf.Abs(maxOffset.x - minOffset.x);
            float height = Mathf.Abs(maxOffset.y - minOffset.y);

            // Get center position
            Vector3 center = transformPosition != TransformType.Vector3 ? positionTransform.position : instantiateLocation;

            // Draw X Range as a Red Rectangle (Horizontal Bounds)
            Gizmos.color = Color.red;
            DrawRectangleXY(center, width, height);
        }

        /// <summary>
        /// Draws a rectangle properly aligned to the **XY plane** in 2D.
        /// </summary>
        private void DrawRectangleXY(Vector3 center, float width, float height)
        {
            Vector3 halfWidth = new Vector3(width / 2f, 0, 0);  // X-axis direction
            Vector3 halfHeight = new Vector3(0, height / 2f, 0); // Y-axis direction

            // Define the four corners of the rectangle
            Vector3 topLeft = center + halfHeight - halfWidth;
            Vector3 topRight = center + halfHeight + halfWidth;
            Vector3 bottomLeft = center - halfHeight - halfWidth;
            Vector3 bottomRight = center - halfHeight + halfWidth;

            // Draw rectangle edges
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        //private GameObject CreateAReferenecedGameObject(GameObject prefab)
        //{
        //    GameObject newObj = Instantiate(prefab, instantiateParent != null ? instantiateParent : null);
        //    if (newObj == null)
        //        return null;

        //    SD_EventWeaverObject reference = newObj.GetComponent<SD_EventWeaverObject>();
        //    if (reference == null)            
        //        reference = newObj.AddComponent<SD_EventWeaverObject>();            

        //    reference.Initialize(refHash);

        //    newObj.transform.localScale = Vector3.one;

        //    return newObj;
        //}
    }

}