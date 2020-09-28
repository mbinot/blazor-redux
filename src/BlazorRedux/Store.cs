﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;


namespace BlazorRedux
{
    public class Store<TState, TAction> : IDisposable
    {
        private readonly TState _initialState;
        private readonly Reducer<TState, TAction> _rootReducer;
        private readonly ReduxOptions<TState> _options;
        private readonly DevToolsInterop _devToolsInterop;
        private NavigationManager _navigationManager;
        private string _currentLocation;
        private bool _timeTraveling;
        private readonly object _syncRoot = new object();

        public TState State { get; private set; }
        public IList<HistoricEntry<TState, object>> History { get; }
        public event EventHandler Change;

        public Store(TState initialState, Reducer<TState, TAction> rootReducer, ReduxOptions<TState> options, DevToolsInterop devToolsInterop)
        {
            _initialState = initialState;
            _rootReducer = rootReducer;
            _options = options;
            _devToolsInterop = devToolsInterop;

            State = initialState;

            _devToolsInterop.Reset += OnDevToolsReset;
            _devToolsInterop.TimeTravel += OnDevToolsTimeTravel;
            _devToolsInterop.Log("initial", _options.StateSerializer(State));

            History = new List<HistoricEntry<TState, object>>
            {
                new HistoricEntry<TState, object>(State)
            };
        }

        internal void Init(NavigationManager uriHelper)
        {
            if (_navigationManager != null || uriHelper == null) return;

            lock (_syncRoot)
            {
                _navigationManager = uriHelper;
                _navigationManager.LocationChanged += OnLocationChanged;
            }

            // TODO: Queue up any other actions, and let this apply to the initial state.
            DispatchLocation(new NewLocationAction { Location = _navigationManager.ToAbsoluteUri(_navigationManager.Uri).ToString() });

            Console.WriteLine("Redux store initialized.");
        }

        public void Dispose()
        {
            if (_navigationManager != null)
                _navigationManager.LocationChanged -= OnLocationChanged;
            
            _devToolsInterop.Reset -= OnDevToolsReset;
            _devToolsInterop.TimeTravel -= OnDevToolsTimeTravel;
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {

            var newAbsoluteUri = e.Location;
            if (_timeTraveling) return;
            if (newAbsoluteUri == _currentLocation) return;

            lock (_syncRoot)
            {
                _currentLocation = newAbsoluteUri;
            }

            DispatchLocation(new NewLocationAction { Location = newAbsoluteUri });
        }

        private void OnDevToolsReset(object sender, EventArgs e)
        {
            var state = _initialState;
            TimeTravel(state);
        }

        private void OnDevToolsTimeTravel(object sender, StringEventArgs e)
        {
            var state = _options.StateDeserializer(e.String);
            _timeTraveling = true;
            TimeTravel(state);
            _timeTraveling = false;
        }

        private void OnChange(EventArgs e)
        {
            if (Change == null) return;
            var handler = Change;
            handler?.Invoke(this, e);

            var getLocation = _options.GetLocation;
            if (getLocation == null) return;
            var newLocation = getLocation(State);
            if (newLocation == _currentLocation || newLocation == null) return;

            lock (_syncRoot)
            {
                _currentLocation = newLocation;
            }
          
            _navigationManager.NavigateTo(newLocation);
        }

        public void Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _rootReducer(State, action);
                _devToolsInterop.Log(action.ToString(), _options.StateSerializer(State));
                History.Add(new HistoricEntry<TState, object>(State, action));
            }

            OnChange(null);
        }

        void DispatchLocation(NewLocationAction locationAction)
        {
            var locationReducer = _options.LocationReducer;

            if (locationReducer == null && locationAction is TAction)
            {
                // Just use the RootReducer unless the user has configured a LocationReducer
                var genericAction = (TAction)(object)locationAction;
                Dispatch(genericAction);
            }

            if (locationReducer == null) return;

            lock (_syncRoot)
            {
                State = locationReducer(State, locationAction);
                _devToolsInterop.Log(locationAction.ToString(), _options.StateSerializer(State));
                History.Add(new HistoricEntry<TState, object>(State, locationAction));
            }

            OnChange(null);
        }

        public void TimeTravel(TState state)
        {
            lock (_syncRoot)
            {
                State = state;
            }

            OnChange(null);
        }
    }
}