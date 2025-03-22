using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CqrsWithMediatR.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IEnumerable<(Type Interface, Type Implementation)> GetServices()
        {
            // Get current executing assembly.
            var curAssembly = Assembly.GetExecutingAssembly()
                ?? throw new InvalidOperationException("ServiceRegistration: Could not retrieve the executing assembly.");

            // Find all classes that match interface naming convention
            var serviceTypes = curAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)         // Get all concrete classes
                .Select(t => new 
                {
                    Interface = t.GetInterface($"I{t.Name}"),   // Find the matching interface by convention
                    Implementation = t
                })
                .Where(t => t.Interface != null)                // Make sure a matching interface exists
                .Select(t => (t.Interface!, t.Implementation))  // Return as (Interface, Implementation) tuple
                .ToList();                                      // NOTE the null-forgiving operator.  Where has filter these out.

            return serviceTypes;
        }
    }
}
