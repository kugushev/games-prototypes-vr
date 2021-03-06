﻿using JetBrains.Annotations;
using Kugushev.Scripts.Common;
using Kugushev.Scripts.Common.Models.Abstractions;
using Kugushev.Scripts.Common.Utils;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Mission.Constants;
using Kugushev.Scripts.Mission.Enums;
using Kugushev.Scripts.Mission.Interfaces;
using Kugushev.Scripts.Mission.Models;
using UnityEngine;

namespace Kugushev.Scripts.Mission.Player
{
    [CreateAssetMenu(menuName = CommonConstants.MenuPrefix + "OrdersManager")]
    public class OrdersManager : ScriptableObject, IModel, ICommander
    {
        [SerializeField] private ObjectsPool pool;
        [SerializeField] private float gapBetweenWaypoints = GameplayConstants.GapBetweenWaypoints;
        private readonly TempState _state = new TempState();

        private class TempState
        {
            public Order CurrentOrder;
            public Planet HighlightedPlanet;
            public Fleet Fleet;
        }

        [CanBeNull] public Order CurrentOrder => _state.CurrentOrder;

        public void HandlePlanetTouch(Planet planet)
        {
            if (_state.CurrentOrder == null)
            {
                if (planet.Faction == Faction.Green)
                {
                    _state.HighlightedPlanet = planet;
                }
            }
            else if (_state.CurrentOrder.SourcePlanet != planet)
            {
                _state.Fleet.CommitOrder(_state.CurrentOrder, planet);
                _state.CurrentOrder = null;
            }
        }

        public void HandlePlanetDetouch()
        {
            if (_state.CurrentOrder == null)
                _state.HighlightedPlanet = null;
            else
                _state.CurrentOrder.Status = OrderStatus.Assignment;
        }

        public void HandleSelect(Percentage allocatedPower)
        {
            if (_state.HighlightedPlanet != null)
            {
                DropCurrentOrder();
                _state.CurrentOrder = pool.GetObject<Order, Order.State>(new Order.State(_state.HighlightedPlanet,
                    allocatedPower));
            }
        }

        public void HandleDeselect()
        {
            DropCurrentOrder();
        }

        private void DropCurrentOrder()
        {
            if (_state.CurrentOrder != null)
            {
                var order = _state.CurrentOrder;
                order.Dispose();

                _state.CurrentOrder = null;
            }
        }

        public void HandleMove(Vector3 position)
        {
            var currentOrder = _state.CurrentOrder;
            if (currentOrder != null && currentOrder.Status == OrderStatus.Assignment)
            {
                currentOrder.RegisterMovement(position, gapBetweenWaypoints);
            }
        }

        #region ICommander

        public void AssignFleet(Fleet fleet, Faction faction)
        {
            _state.Fleet = fleet;
        }

        public void WithdrawFleet()
        {
            _state.Fleet = null;
        }

        #endregion

        public void Dispose()
        {
            DropCurrentOrder();
        }
    }
}