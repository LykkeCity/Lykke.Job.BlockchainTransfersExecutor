using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    internal class TransitionAttempt<TState> where TState:struct, Enum
    {
        public readonly TState InitialState;
        public readonly Type TransitionCommandType;

        public TransitionAttempt(TState initialState, Type commandType)
        {
            InitialState = initialState;
            TransitionCommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
        }

        #region  Equals
        protected bool Equals(TransitionAttempt<TState> other)
        {
            return Equals(InitialState, other.InitialState) && TransitionCommandType == other.TransitionCommandType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransitionAttempt<TState>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (InitialState.GetHashCode() * 397) ^ (TransitionCommandType != null ? TransitionCommandType.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
