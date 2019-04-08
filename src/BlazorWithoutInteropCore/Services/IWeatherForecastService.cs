using System;
using System.Threading.Tasks;

namespace BlazorWithoutInteropCore.Services
{
    public interface IWeatherForecastService
    {

        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate);
    }
}
