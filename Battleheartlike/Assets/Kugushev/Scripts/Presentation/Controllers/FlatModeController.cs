﻿using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Game.Models.Characters.Abstractions;
using Kugushev.Scripts.Game.Services;
using Kugushev.Scripts.Presentation.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kugushev.Scripts.Presentation.Controllers
{
    /// <summary>
    /// Alternative input and other things for debugging in not VR mode
    /// </summary>
    public class FlatModeController : MonoBehaviour
    {
        [SerializeField] private GameObject xrRig;
        [SerializeField] private PlayerInteractionsService playerInteractionsService;
        [SerializeField] private Character character;
        [SerializeField] private Camera flatCamera;

        private void Awake()
        {
            xrRig.SetActive(false);
        }

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                var mousePosition = mouse.position.ReadValue();
                Ray ray = flatCamera.ScreenPointToRay(mousePosition);
                Debug.DrawRay(ray.origin, ray.direction);
                if (Physics.Raycast(ray, out var hit))
                {
                    var interactable = hit.collider.GetComponent<PlayerInteractableComponent>();
                    Character passive = null;
                    if (!ReferenceEquals(null, interactable))
                        passive = interactable.Character;

                    var position = new Position(hit.point);
                    var success = playerInteractionsService.TryFindAndSetBehavior(character, passive, position);
                    if (!success) 
                        Debug.LogWarning($"Can't interact by {character} with {passive} at {position}");
                }
                
            }
        }
    }
}