using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public enum CameraType
    {
        Main,
        Winner 
    }

    [Serializable]
    public class CameraSet
    {
        public CameraType type;
        public CinemachineCamera virtualCamera;
    }

    public struct CameraContext
    {
        public float transitionTime;
        public Vector3 positionOffset;
        public Transform target;

        public CameraContext(float transitionTime, Vector3 positionOffset = default, Transform target = null)
        {
            this.transitionTime = transitionTime;
            this.positionOffset = positionOffset;
            this.target = target;
        }
    }


    [SerializeField] CameraSet[] cameras;

    Dictionary<CameraType, CameraSet> camerasMap = new();

    CinemachineBrain brain;

    private void Awake()
    {
        foreach (var item in cameras)
        {
            camerasMap[item.type] = item;
        }

        if (Camera.main != null)
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
        }
    }


    public void SwitchCamera(CameraType type, CameraContext context)
    {
        if (brain != null)
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(
              CinemachineBlendDefinition.Styles.EaseInOut, context.transitionTime);
        }

        if (camerasMap.TryGetValue(type, out CameraSet cameraSet))
        {
            var targetCamera = cameraSet.virtualCamera;

            if (context.target != null)
            {
                targetCamera.transform.position = context.target.position + context.positionOffset;
                targetCamera.LookAt = context.target;
            }
            else targetCamera.LookAt = null;
        }

        foreach (var kvp in camerasMap)
        {
            int newPriority = (kvp.Key == type) ? 10 : 0;
            kvp.Value.virtualCamera.Priority = newPriority;
        }
    }
}