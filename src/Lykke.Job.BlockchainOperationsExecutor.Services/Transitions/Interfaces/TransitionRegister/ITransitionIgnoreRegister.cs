﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitionIgnoreRegister<TState> : ITransitionRegisterBase<TState>
        where TState : struct, Enum
    {
        ITransitionIgnoreRegister<TState> Ignore<TCommand>();
    }
}