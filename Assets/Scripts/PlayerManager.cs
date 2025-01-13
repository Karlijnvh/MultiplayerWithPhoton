using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace PV.Multiplayer
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Camera")]
        public CinemachineVirtualCameraBase cinemachineCamera;
        public Transform cameraTransform;

        private void Awake()
        {
            RemoveParentOfAll();
        }

        private void RemoveParentOfAll()
        {
            cameraTransform.SetParent(null);
            cinemachineCamera.transform.SetParent(null);
            GameObject parent = transform.parent.gameObject;
            transform.SetParent(null);
            Destroy(parent);
        }
    }
}
