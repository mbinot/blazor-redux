using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorRedux
{
    public class ReduxComponent<TState, TAction> : ComponentBase, IDisposable
    {
        [Inject] public Store<TState, TAction> Store { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        public TState State => Store.State;

        public RenderFragment ReduxDevTools;

        public void Dispose()
        {
            Store.Change -= OnChangeHandler;
        }

        protected override void OnInitialized()
        {
            Store.Init(NavigationManager);
            Store.Change += OnChangeHandler;

            ReduxDevTools = builder =>
            {
                var seq = 0;
                builder.OpenComponent<ReduxDevTools>(seq);
                builder.CloseComponent();
            };
            
            base.OnInitialized();
        }

        private void OnChangeHandler(object sender, EventArgs e)
        {
//            base.Invoke(StateHasChanged);
            InvokeAsync(StateHasChanged).Wait();
        }

        
        public void Dispatch(TAction action)
        {
            Store.Dispatch(action);
        }
    }
}