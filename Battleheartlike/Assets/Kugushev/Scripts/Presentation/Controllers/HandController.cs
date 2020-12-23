﻿using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Game.Models.Characters.Abstractions;
using Kugushev.Scripts.Game.Services;
using Kugushev.Scripts.Presentation.Components;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Kugushev.Scripts.Presentation.Controllers
{
    [RequireComponent(typeof(XRController))]
    public class HandController : MonoBehaviour
    {
        [SerializeField] private InteractionsService interactionsService;
        [SerializeField] private PlayableCharacter character;
        private XRController _xrController;

        private void Awake()
        {
            _xrController = GetComponent<XRController>();
        }

        private void FixedUpdate()
        {
            if (_xrController.inputDevice.IsPressed(InputHelpers.Button.Trigger, out bool isPressed) && isPressed)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit,
                    Mathf.Infinity))
                {
                    var interactable = hit.collider.GetComponent<PlayerInteractableComponent>();
                    Character passive = null;
                    if (!ReferenceEquals(null, interactable))
                        passive = interactable.Character;

                    var position = new Position(hit.point);
                    interactionsService.ExecuteInteraction(character, passive, position);
                }
            }

            // if (_xrController.inputDevice.IsPressed(InputHelpers.Button.PrimaryButton, out bool button) && button)
            // {
            //     unit.Attack();
            // }
        }
    }
}