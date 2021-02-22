﻿using System;
using Cysharp.Threading.Tasks;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Common.ValueObjects;
using Kugushev.Scripts.Game.Common;
using Kugushev.Scripts.Game.Common.Entities.Abstractions;
using Kugushev.Scripts.Game.Missions.Enums;
using Kugushev.Scripts.Game.Missions.Interfaces;
using UnityEngine;

namespace Kugushev.Scripts.Game.Missions.Entities
{
    public class Planet : Poolable<Planet.State>, IModel, IFighter
    {
        public struct State
        {
            public State(Faction faction, PlanetSize size, int production, Vector3 position)
            {
                Faction = faction;
                Size = size;
                Production = production;
                Position = position;
                Power = 0;
                Selected = false;
            }

            public Faction Faction { get; internal set; }
            public PlanetSize Size { get; }
            public int Production { get; }
            public Vector3 Position { get; }
            public int Power { get; internal set; }
            public bool Selected { get; internal set; }
        }

        public Planet(ObjectsPool objectsPool) : base(objectsPool)
        {
        }

        public Faction Faction => ObjectState.Faction;

        public bool CanBeAttacked => true;

        public PlanetSize Size => ObjectState.Size;

        public int Power => ObjectState.Power;

        public bool Selected
        {
            get => ObjectState.Selected;
            set => ObjectState.Selected = value;
        }

        public Position Position => new Position(ObjectState.Position);

        public UniTask ExecuteProductionCycle()
        {
            if (Faction == Faction.Neutral && ObjectState.Power >= GameConstants.NeutralPlanetMaxPower)
                return UniTask.CompletedTask;

            ObjectState.Power += ObjectState.Production;
            return UniTask.CompletedTask;
        }

        public int Recruit(Percentage powerToRecruit)
        {
            var powerToRecruitAbs = Mathf.FloorToInt(ObjectState.Power * powerToRecruit.Amount);

            if (powerToRecruitAbs > GameConstants.SoftCapArmyPower)
                powerToRecruitAbs = GameConstants.SoftCapArmyPower;

            ObjectState.Power -= powerToRecruitAbs;
            return powerToRecruitAbs;


            // if (ObjectState.Power <= GameConstants.SoftCapArmyPower)
            // {
            //     var allPower = ObjectState.Power;
            //     ObjectState.Power = 0;
            //     return allPower;
            // }
            //
            // ObjectState.Power -= GameConstants.SoftCapArmyPower;
            // return GameConstants.SoftCapArmyPower;
        }

        public void Reinforce(Army army)
        {
            ObjectState.Power += army.Power;
        }

        public FightRoundResult SufferFightRound(Faction enemyFaction, int damage = GameConstants.UnifiedDamage)
        {
            ObjectState.Power -= damage;

            if (ObjectState.Power < 0)
            {
                ObjectState.Power *= -1;
                ObjectState.Faction = enemyFaction;
                return FightRoundResult.Defeated;
            }

            return FightRoundResult.StillAlive;
        }
    }
}