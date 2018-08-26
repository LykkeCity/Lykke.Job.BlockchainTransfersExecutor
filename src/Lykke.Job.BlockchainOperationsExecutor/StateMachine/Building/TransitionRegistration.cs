using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegistration
    {
        private readonly Delegate _handleTransition;
        private readonly IReadOnlyCollection<(Delegate Precondition, Delegate FormatMessage)> _preconditions;

        public TransitionRegistration(
            Delegate handleTransition,
            IReadOnlyCollection<(Delegate Precondition, Delegate FormatMessage)> preconditions)
        {
            _handleTransition = handleTransition;
            _preconditions = preconditions;
        }

        public IReadOnlyCollection<string> GetPreconditionErrors<TAggregate, TEvent>(TAggregate aggregate, TEvent @event)
        {
            return _preconditions
                .Where(p => !(bool) p.Precondition.DynamicInvoke(aggregate, @event))
                .Select(p => (string) p.FormatMessage.DynamicInvoke(aggregate, @event))
                .ToArray();
        }

        public void Switch<TAggregate, TEvent>(TAggregate aggregate, TEvent @event)
        {
            _handleTransition.DynamicInvoke(aggregate, @event);
        }
    }
}
