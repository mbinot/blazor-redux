﻿using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate void Dispatcher<in TAction>(TAction action);

    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);

    public delegate Task AsyncActionsCreator<in TState, out TAction>(Dispatcher<TAction> dispatcher, TState state);
}
