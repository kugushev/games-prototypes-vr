﻿using Cysharp.Threading.Tasks;
using Kugushev.Scripts.Mission.Constants;
using Kugushev.Scripts.Mission.Managers;
using Kugushev.Scripts.Mission.Utils;
using UnityEngine;

namespace Kugushev.Scripts.Presentation.Controllers
{
    public class MissionPreparationController : MonoBehaviour
    {
        [SerializeField] private MissionModelProvider missionManager;

        public void AdjustTime(float sliderValue)
        {
            if (missionManager.TryGetModel(out var model))
            {
                var dayOfYear = Mathf.FloorToInt(GameplayConstants.DaysInYear * sliderValue);
                model.PlanetarySystem.SetDayOfYear(dayOfYear);
            }
        }

        public void StartMission()
        {
            if (missionManager.TryGetModel(out var model))
            {
                model.ReadyToExecute = true;
            }
        }
    }
}