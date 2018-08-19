using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionIgnoringRegistration
    {
        public bool HasAdditionalCondition { get; }

        private readonly Delegate _additionalCondition;

        public TransitionIgnoringRegistration(Delegate additionalCondition)
        {
            _additionalCondition = additionalCondition;
            HasAdditionalCondition = additionalCondition != null;
        }

        public bool IsAdditionalConditionsSatisfied<TAggregate, TEvent>(TAggregate aggregate, TEvent @event)
        {
            if (_additionalCondition == null)
            {
                return true;
            }

            return (bool) _additionalCondition.DynamicInvoke(aggregate, @event);
        }
    }
}
