using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddReduxStore<TState, TAction>(
            this IServiceCollection configure,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState>> options = null)
        {
            configure.AddScoped<DevToolsInterop>();
            var reduxOptions = new ReduxOptions<TState>();
            options?.Invoke(reduxOptions);
            configure.AddScoped<Store<TState, TAction>>(sp => new Store<TState, TAction>(initialState, rootReducer, reduxOptions, sp.GetRequiredService<DevToolsInterop>()));
            return configure;
        }
    }

    public static class DateExtensionMethods
    {
        /// <summary>B
        /// Computes the first workday of the month.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetFirstWorkdayOfMonth(this DateTime date)
        {
            var first = new DateTime(date.Year, date.Month, 1).MoveToNextWorkday();
            return first;
        }
        
        /// <summary>
        /// Computes the last workday of the month.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetLastWorkdayOfMonth(this DateTime date)
        {
            var last = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            while (last.DayOfWeek == DayOfWeek.Saturday || last.DayOfWeek == DayOfWeek.Sunday)
                last = last.AddDays(-1);
            return last;
        }
        
        /// <summary>
        /// Computes the next workday following given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetNextWorkday(this DateTime date)
        {
            return date.AddDays(1).MoveToNextWorkday();
        }

        /// <summary>
        /// Moves given date to next workday if necessary.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime MoveToNextWorkday(this DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                date = date.AddDays(1);
            return date;
        }
        
    }
}