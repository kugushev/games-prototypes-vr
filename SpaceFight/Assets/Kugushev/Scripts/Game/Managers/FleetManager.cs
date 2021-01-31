﻿using System.Collections.Generic;
using Kugushev.Scripts.Common.Utils;
using Kugushev.Scripts.Common.Utils.Pooling;
using Kugushev.Scripts.Game.Models;
using Kugushev.Scripts.Game.Models.Abstractions;
using Kugushev.Scripts.Game.ValueObjects;
using UnityEngine;

namespace Kugushev.Scripts.Game.Managers
{
    [CreateAssetMenu(menuName = CommonConstants.MenuPrefix + "FleetManager")]
    public class FleetManager: Model
    {
        [SerializeField] private ObjectsPool pool;
        
        
        // todo: remove this shit code
        public Queue<Army> ArmiesToSent { get; } = new Queue<Army>();

        protected override void Dispose(bool destroying)
        {
            
        }

        public void CommitOrder(Order order)
        {
            // todo: dispose army on the end
            var army = pool.GetObject<Army, Army.State>(new Army.State(order, 5f, 1f, 1));
            ArmiesToSent.Enqueue(army);
        }
    }
}