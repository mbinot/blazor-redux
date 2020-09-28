using System;
using Newtonsoft.Json;

namespace BlazorRedux
{
    public class ReduxOptions<TState>
    {

        public ReduxOptions()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };
            // Defaults
            StateSerializer = state => JsonConvert.SerializeObject(state, Formatting.None, settings);
            StateDeserializer = JsonConvert.DeserializeObject<TState>;
        }

        public Reducer<TState, NewLocationAction> LocationReducer { get; set; }
        public Func<TState, string> GetLocation { get; set; }
        public Func<TState, string> StateSerializer { get; set; }
        public Func<string, TState> StateDeserializer { get; set; }
    }
}