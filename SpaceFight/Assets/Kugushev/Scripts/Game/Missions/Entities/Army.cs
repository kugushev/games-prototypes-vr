﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Game.Common;
using Kugushev.Scripts.Game.Common.Interfaces;
using Kugushev.Scripts.Game.Missions.Enums;
using Kugushev.Scripts.Game.Missions.Interfaces;
using Kugushev.Scripts.Game.Missions.Presets;
using UnityEngine;

namespace Kugushev.Scripts.Game.Missions.Entities
{
    [Serializable]
    public class Army : Poolable<Army.State>, IGameLoopParticipant, IFighter
    {
        public Army(ObjectsPool objectsPool) : base(objectsPool)
        {
        }

        [Serializable]
        public struct State
        {
            public Order order;
            public float speed;
            public float angularSpeed;
            public Faction faction;
            public int power;
            public ArmyStatus status;
            public Vector3 currentPosition;
            public Quaternion currentRotation;
            public int currentWaypoint;
            public float waypointRotationProgress;
            public float fightingTimeCollector;

            public State(Order order, float speed, float angularSpeed, Faction faction, int power)
            {
                this.order = order;
                this.speed = speed;
                this.angularSpeed = angularSpeed;
                this.faction = faction;
                this.power = power;
                status = ArmyStatus.Recruiting;
                currentPosition = order.Path[0];
                currentRotation = Quaternion.identity;
                currentWaypoint = 0;
                waypointRotationProgress = 0f;
                fightingTimeCollector = 0f;
            }
        }

        private readonly HashSet<IFighter> _targets = new HashSet<IFighter>();
        private readonly List<IFighter> _targetsToRemoveBuffer = new List<IFighter>(8);

        public ArmyStatus Status
        {
            get => ObjectState.status;
            set => ObjectState.status = value;
        }

        public Position Position => new Position(ObjectState.currentPosition);
        public Quaternion Rotation => ObjectState.currentRotation;
        public int Power => ObjectState.power;
        public bool Disbanded => ObjectState.status == ArmyStatus.Disbanded;
        public Faction Faction => ObjectState.faction;

        public bool CanBeAttacked => ObjectState.status != ArmyStatus.Arriving &&
                                     ObjectState.status != ArmyStatus.Disbanded;

        public IReadOnlyCollection<IFighter> CurrentTargets => _targets;

        public void NextStep(float deltaTime)
        {
            if (!Active)
                return;

            switch (ObjectState.status)
            {
                case ArmyStatus.Unspecified:
                    Debug.LogWarning("Army status is Unspecified");
                    break;
                case ArmyStatus.OnMatch:
                case ArmyStatus.Arriving:
                    MoveStep(deltaTime);
                    break;
                case ArmyStatus.OnSiege:
                    SiegeStep(deltaTime);
                    break;
                case ArmyStatus.Fighting:
                    FightStep(deltaTime);
                    break;
            }
        }

        private void MoveStep(float deltaTime)
        {
            var currentWaypoint = ObjectState.currentWaypoint;
            var path = ObjectState.order.Path;

            if (path.Count <= currentWaypoint + 1)
            {
                Disband();
                return;
            }

            var previous = path[currentWaypoint];
            var next = path[currentWaypoint + 1];

            ChangePosition();
            ChangeRotation();

            void ChangePosition()
            {
                var lookVector = (next - previous).normalized;
                var newPosition = ObjectState.currentPosition + lookVector * (deltaTime * ObjectState.speed);
                var dot = Vector3.Dot((next - newPosition).normalized, lookVector);
                if (dot <= 0f || ObjectState.currentPosition == next)
                {
                    ObjectState.currentPosition = next;

                    ObjectState.currentWaypoint++;
                    ObjectState.waypointRotationProgress = 0f;
                }
                else
                    ObjectState.currentPosition = newPosition;
            }

            void ChangeRotation()
            {
                ObjectState.waypointRotationProgress += deltaTime * ObjectState.angularSpeed;
                var lookRotationVector = next - ObjectState.currentPosition;
                if (lookRotationVector != Vector3.zero)
                {
                    var lookRotation = Quaternion.LookRotation(lookRotationVector);
                    ObjectState.currentRotation = Quaternion.Slerp(ObjectState.currentRotation, lookRotation,
                        ObjectState.waypointRotationProgress);
                }
            }
        }

        private void SiegeStep(float deltaTime)
        {
            if (_targets.Count == 0)
            {
                Debug.LogError("No enemies");
                return;
            }

            _targetsToRemoveBuffer.Clear();

            ObjectState.fightingTimeCollector += deltaTime;
            if (ObjectState.fightingTimeCollector > GameConstants.SiegeRoundDelay)
            {
                ObjectState.fightingTimeCollector = 0f;

                foreach (var target in _targets)
                    if (target is Planet targetPlanet)
                    {
                        bool captured = !target.CanBeAttacked;

                        if (!captured)
                            captured = ExecuteSiege(targetPlanet);

                        if (captured)
                            _targetsToRemoveBuffer.Add(target);
                    }
            }

            RemoveTargetsToRemove();
            if (_targets.Count == 0)
                Arrive();

            bool ExecuteSiege(Planet target)
            {
                bool captured;
                if (target.Faction != ObjectState.faction)
                {
                    // execute fight
                    var result = target.SufferFightRound(Faction);
                    captured = result == FightRoundResult.Defeated;

                    if (!captured)
                        SufferFightRound(target.Faction);
                }
                else
                    captured = true;

                return captured;
            }
        }

        private void FightStep(float deltaTime)
        {
            if (_targets.Count == 0)
            {
                Debug.LogError("No enemies");
                return;
            }

            _targetsToRemoveBuffer.Clear();

            ObjectState.fightingTimeCollector += deltaTime;
            if (ObjectState.fightingTimeCollector > GameConstants.FightRoundDelay)
            {
                ObjectState.fightingTimeCollector = 0f;

                foreach (var target in _targets)
                {
                    if (target is Army targetArmy)
                    {
                        if (!targetArmy.Active)
                        {
                            Debug.LogWarning("Enemy is not active");
                            _targetsToRemoveBuffer.Add(target);
                            continue;
                        }

                        bool enemyIsDefeated = !targetArmy.CanBeAttacked;

                        if (!enemyIsDefeated)
                            enemyIsDefeated = ExecuteFight(targetArmy);

                        if (enemyIsDefeated)
                        {
                            _targetsToRemoveBuffer.Add(targetArmy);
                        }
                    }
                }
            }

            RemoveTargetsToRemove();

            if (_targets.Count == 0)
                ObjectState.status = ArmyStatus.OnMatch;

            bool ExecuteFight(Army targetPlanet)
            {
                if (targetPlanet.Faction == Faction)
                {
                    Debug.LogError($"We're trying to hit our allies {ObjectState.faction}");
                    return false;
                }

                var result = targetPlanet.SufferFightRound(Faction);
                return result == FightRoundResult.Defeated;
            }
        }

        private void RemoveTargetsToRemove()
        {
            foreach (var targetToRemove in _targetsToRemoveBuffer) _targets.Remove(targetToRemove);
            _targetsToRemoveBuffer.Clear();
        }

        #region IFighter

        public FightRoundResult SufferFightRound(Faction enemyFaction, int damage = GameConstants.UnifiedDamage)
        {
            if (enemyFaction == Faction)
            {
                Debug.LogError($"Ally is trying to kill me {enemyFaction}");
                return FightRoundResult.StillAlive;
            }

            ObjectState.power -= damage;
            if (ObjectState.power <= 0)
            {
                Disband();
                return FightRoundResult.Defeated;
            }

            return FightRoundResult.StillAlive;
        }

        #endregion


        public void HandlePlanetVisiting(Planet planet)
        {
            if (ObjectState.status != ArmyStatus.OnMatch)
                return;

            if (planet != ObjectState.order.TargetPlanet)
                return;

            var opposite = ObjectState.faction.GetOpposite();

            if (planet.Faction == ObjectState.faction)
            {
                Arrive();
            }
            else if (planet.Faction == Faction.Neutral || planet.Faction == opposite)
            {
                ObjectState.status = ArmyStatus.OnSiege;
                _targets.Add(planet);
            }
            else
            {
                Debug.LogError($"Unexpected planet faction {planet.Faction}");
            }
        }

        public void HandleCrash()
        {
            Disband();
        }

        public void HandleArmyInteraction(Army otherPartyArmy)
        {
            if (ObjectState.status != ArmyStatus.OnMatch)
                return;

            if (otherPartyArmy.Faction == ObjectState.faction)
                return;

            if (!otherPartyArmy.CanBeAttacked)
                return;

            ObjectState.status = ArmyStatus.Fighting;
            _targets.Add(otherPartyArmy);
        }

        private void Arrive()
        {
            ObjectState.status = ArmyStatus.Arriving;
            ObjectState.order.TargetPlanet.Reinforce(this);
        }

        private void Disband() => ObjectState.status = ArmyStatus.Disbanded;

        protected override void OnRestore(State state) => _targets.Clear();

        protected override void OnClear(State state)
        {
            state.order.Dispose();
            _targets.Clear();
        }
    }
}