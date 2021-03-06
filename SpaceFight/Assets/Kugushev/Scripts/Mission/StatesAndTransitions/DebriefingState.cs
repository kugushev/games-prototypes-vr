﻿using System;
using System.Collections.Generic;
using Kugushev.Scripts.Campaign.Utils;
using Kugushev.Scripts.Campaign.ValueObjects;
using Kugushev.Scripts.Common.StatesAndTransitions;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Mission.Achievements.Abstractions;
using Kugushev.Scripts.Mission.Constants;
using Kugushev.Scripts.Mission.Enums;
using Kugushev.Scripts.Mission.Managers;
using Kugushev.Scripts.Mission.Models;
using UnityEngine;

namespace Kugushev.Scripts.Mission.StatesAndTransitions
{
    public class DebriefingState : BaseSceneLoadingState<MissionModel>
    {
        private readonly MissionSceneResultPipeline _missionSceneResultPipeline;
        private readonly AchievementsManager _achievementsManager;
        private readonly ObjectsPool _objectsPool;
        private readonly List<AbstractAchievement> _achievementsBuffer = new List<AbstractAchievement>(64);

        public DebriefingState(MissionModel model, MissionSceneResultPipeline missionSceneResultPipeline,
            AchievementsManager achievementsManager, ObjectsPool objectsPool)
            : base(model, UnityConstants.Scenes.MissionDebriefingScene, true)
        {
            _missionSceneResultPipeline = missionSceneResultPipeline;
            _achievementsManager = achievementsManager;
            _objectsPool = objectsPool;
        }

        protected override void AssertModel()
        {
            if (Model.ExecutionResult == null)
                Alert();
        }

        protected override void OnEnterBeforeLoadScene()
        {
            var debriefingInfo = _objectsPool.GetObject<DebriefingSummary, DebriefingSummary.State>(default);

            if (Model.ExecutionResult?.Winner == Model.PlayerFaction)
            {
                _achievementsBuffer.Clear();
                _achievementsManager.FindAchieved(_achievementsBuffer, Model.PlayerFaction);

                debriefingInfo.Fill(_achievementsBuffer);

                _achievementsBuffer.Clear();
            }

            Model.DebriefingSummary = debriefingInfo;

            base.OnEnterBeforeLoadScene();
        }

        protected override void OnExitBeforeUnloadScene()
        {
            if (Model.ExecutionResult != null)
            {
                var playerWin = Model.ExecutionResult.Value.Winner switch
                {
                    Faction.Green => true,
                    Faction.Red => false,
                    _ => throw new ArgumentOutOfRangeException(nameof(Model.ExecutionResult.Value.Winner),
                        $"Unexpected winner {Model.ExecutionResult.Value.Winner}")
                };
                var reward = Model.DebriefingSummary.SelectedAchievement;

                _missionSceneResultPipeline.Set(new MissionResult(playerWin, reward));
            }
            else
                Alert();
        }

        private static void Alert() => Debug.LogError("No execution result found");
    }
}