﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kugushev.Scripts.Common.StatesAndTransitions;

namespace Kugushev.Scripts.Common.Utils.FiniteStateMachine
{
    public class StateMachine
    {
        private readonly IReadOnlyDictionary<IState, IReadOnlyList<TransitionRecord>> _transitions;
        private IState _currentState;

        public StateMachine(IReadOnlyDictionary<IState, IReadOnlyList<TransitionRecord>> transitions)
        {
            ResetTransitions(transitions);
            _transitions = transitions;

            _currentState = EntryState.Instance;
        }

        public async UniTask UpdateAsync(Func<float> deltaTimeProvider)
        {
            if (_transitions.TryGetValue(_currentState, out var transitions))
                foreach (var (transition, targetState) in transitions)
                    if (transition.ToTransition)
                    {
                        if (transition is IReusableTransition reusableTransition)
                            reusableTransition.Reset();
                        await SetState(targetState);
                    }

            // by reason of async SetState this OnUpdate execution might be in the another frame than tha start of this method
            var deltaTime = deltaTimeProvider();
            _currentState.OnUpdate(deltaTime);
        }

        public async UniTask DisposeAsync()
        {
            await _currentState.OnExitAsync();
            _currentState = null;
        }

        private async UniTask SetState(IState state)
        {
            await _currentState.OnExitAsync();
            _currentState = state;
            await _currentState.OnEnterAsync();
        }

        private static void ResetTransitions(IReadOnlyDictionary<IState, IReadOnlyList<TransitionRecord>> transitions)
        {
            foreach (var pair in transitions)
            foreach (var (transition, _) in pair.Value)
                if (transition is IReusableTransition reusableTransition)
                    reusableTransition.Reset();
        }
    }
}