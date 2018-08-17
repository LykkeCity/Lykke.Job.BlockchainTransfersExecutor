using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegistration<TState> where TState: struct, IConvertible
    {
        public readonly TState SourceState;

        private readonly Type _eventType;

        public TransitionRegistration(TState sourceState, Type eventType)
        {
            SourceState = sourceState;
            _eventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        }


        #region  Equals

        protected bool Equals(TransitionRegistration<TState> other)
        {
            return Equals(SourceState, other.SourceState) && _eventType == other._eventType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransitionRegistration<TState>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SourceState.GetHashCode() * 397) ^ (_eventType != null ? _eventType.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
