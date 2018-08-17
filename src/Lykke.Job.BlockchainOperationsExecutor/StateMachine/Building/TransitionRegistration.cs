using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegistration<TState> where TState: struct, IConvertible
    {
        private readonly TState _sourceState;
        private readonly Type _eventType;

        public TransitionRegistration(TState sourceState, Type eventType)
        {
            _sourceState = sourceState;
            _eventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        }


        #region  Equals

        private bool Equals(TransitionRegistration<TState> other)
        {
            return Equals(_sourceState, other._sourceState) && _eventType == other._eventType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TransitionRegistration<TState>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_sourceState.GetHashCode() * 397) ^ (_eventType != null ? _eventType.GetHashCode() : 0);
            }
        }

        #endregion


        public override string ToString()
        {
            return $"[From state {_sourceState} on event {_eventType.Name}]";
        }
    }
}
