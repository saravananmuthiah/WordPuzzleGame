using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace MemoryCardGame.Services
{
    public static class ServiceHelper
    {
        public static IServiceProvider? ServiceProvider { get; set; }
        public static T GetService<T>() where T : notnull
        {
            if (ServiceProvider == null)
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
