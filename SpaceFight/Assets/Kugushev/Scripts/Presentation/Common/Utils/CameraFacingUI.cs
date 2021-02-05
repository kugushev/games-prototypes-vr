﻿using System;
using UnityEngine;

namespace Kugushev.Scripts.Presentation.Common.Utils
{
    [RequireComponent(typeof(RectTransform))]
    public class CameraFacingUI : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Camera _camera;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _camera = Camera.main;
        }

        private void Update()
        {
            // due to unexpected rect transform behavior we need to use reflected vector 
            var vector = transform.position - _camera.transform.position;
            _rectTransform.rotation = Quaternion.LookRotation(vector);
        }
    }
}