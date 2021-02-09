﻿using Kugushev.Scripts.Game.Entities.Abstractions;
using Kugushev.Scripts.Game.Managers;
using Kugushev.Scripts.Presentation.PresentationModels.Abstractions;
using UnityEngine;

namespace Kugushev.Scripts.Presentation.PresentationModels
{
    public class OrdersPresentationModel: BasePresentationModel
    {
        [SerializeField] private OrdersManager model;
        protected override Model Model => model;
    }
}