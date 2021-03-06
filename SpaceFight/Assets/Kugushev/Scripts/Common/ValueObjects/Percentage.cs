﻿using UnityEngine;

namespace Kugushev.Scripts.Common.ValueObjects
{
    public readonly struct Percentage
    {
        private readonly float _amount;
        public Percentage(float amount) => _amount = amount;

        public float Amount
        {
            get
            {
                if (_amount < 0f)
                {
                    Debug.LogError($"Unexpected percentage {_amount}");
                    return 0f;
                }

                if (_amount > 1f)
                {
                    Debug.LogError($"Unexpected percentage {_amount}");
                    return 1f;                    
                }
                return _amount;
            }
        }
    }
}