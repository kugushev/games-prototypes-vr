﻿using UnityEngine;

namespace Kugushev.Scripts.Common.Utils.Pooling
{
    public abstract class Poolable<TState> : IPoolable<TState>
        where TState : struct
    {
        private readonly ObjectsPool _myPool;
        [SerializeField] protected TState ObjectState;
        protected Poolable(ObjectsPool objectsPool) => _myPool = objectsPool;
        
        public bool Active { get; private set; }

        public void SetState(TState state)
        {
            ObjectState = state;
            OnRestore(ObjectState);
            Active = true;
        }

        protected virtual void OnRestore(TState state)
        {
        }

        public void ClearState()
        {
            OnClear(ObjectState);
            ObjectState = default;
            Active = false;
        }

        protected virtual void OnClear(TState state)
        {
        }

        public void Dispose()
        {
            if (Active)
                _myPool.GiveBackObject(this);
        }
    }
}