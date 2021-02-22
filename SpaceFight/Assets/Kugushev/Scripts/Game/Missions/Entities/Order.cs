﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Game.Common;
using Kugushev.Scripts.Game.Missions.Enums;
using Kugushev.Scripts.Game.Missions.Presets;
using UnityEngine;

namespace Kugushev.Scripts.Game.Missions.Entities
{
    public class Order : Poolable<Order.State>
    {
        public struct State
        {
            public State(Planet planet, Percentage power)
            {
                Power = power;
                SourcePlanet = planet;
                TargetPlanet = null;
                Status = OrderStatus.Created;
                LastRegisteredPosition = null;
            }


            public readonly Planet SourcePlanet;
            public readonly Percentage Power;
            [CanBeNull] public Planet TargetPlanet;
            public OrderStatus Status;
            public Vector3? LastRegisteredPosition;
        }

        private readonly List<Vector3> _path = new List<Vector3>(GameConstants.OrderPathCapacity);

        public Order(ObjectsPool objectsPool) : base(objectsPool)
        {
        }

        public IReadOnlyList<Vector3> Path => _path;
        public Planet SourcePlanet => ObjectState.SourcePlanet;
        public Planet TargetPlanet => ObjectState.TargetPlanet;
        public Percentage Power => ObjectState.Power;

        public OrderStatus Status
        {
            get => ObjectState.Status;
            internal set => ObjectState.Status = value;
        }

        internal void RegisterMovement(Vector3 position, float gapBetweenWaypoints = GameConstants.GapBetweenWaypoints)
        {
            ObjectState.LastRegisteredPosition = position;

            if (_path.Count > 0)
            {
                var last = _path[_path.Count - 1];
                if (Vector3.Distance(position, last) < gapBetweenWaypoints)
                    return;
            }

            if (_path.Capacity > GameConstants.OrderPathCapacity)
                Debug.LogWarning($"Path capacity increased to {_path.Capacity}");

            _path.Add(position);
        }

        public void Commit(Planet target)
        {
            if (ObjectState.LastRegisteredPosition != null)
                _path.Add(ObjectState.LastRegisteredPosition.Value);

            ObjectState.TargetPlanet = target;
            ObjectState.Status = OrderStatus.Execution;
        }

        protected override void OnClear(State state) => _path.Clear();

        protected override void OnRestore(State state) => _path.Clear();
    }
}