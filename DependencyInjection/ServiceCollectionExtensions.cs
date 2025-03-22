using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CqrsWithMediatR.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServicesWithDefaultConventions(this IServiceCollection services)
        {
            //
            // This method will automatically register classes that follow the convention
            // MyClassName:IMyClassName.
            //
            // This method will dynamically scan the loaded assemblies (and dynamically load any,
            // if necessary) and invoke the method ServiceRegistration.GetServices(), if found.
            // GetServices() returns the list of interfaceType/implementationType pairs that need
            // to be registered with the dependency injection (DI) container.  This list of 
            // interface/implementation pairs is then registered with the DI container.
            //
            // This logic makes the following assumptions.
            //
            //   1.  Each application class library implements the static method ServiceRegistration.GetServices(),
            //       which returns a list of interfaceType/implementationType pairs that match the naming
            //       convention (MyClassName : IMyClassName).  This indicates which types need to be registered
            //       with the dependency injector container.  If non-naming convention services need to be
            //       registered, the respective GetServices() method will need to be modified. 
            //   
            //   2.  Each service is registered as Transient : create new instance every time requested.
            //       If AddScoped or AddSingleton is needed, this logic will need to be modified.
            //
            //   3.  Assemblies are in the same directory as the startup project's bin directory.
            //       If any of the application class library assemblies are located elsewhere,
            //       this logic will need to be modified.
            //
            //   4.  The application has the necessary permissions to load assemblies dynamically
            //       from the file system.  If not, permissions will need to be granted or this
            //       logic will need to be modified.
            //

            // Get all currently loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => 
                {
                    var assemblyName = assembly.GetName().Name;
                    return assemblyName != null
                        && !assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                        && !assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            // Get directory where the application is running
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Get all assembly files from the bin directory minus system dll's
            var assemblyFiles = Directory.GetFiles(basePath, "*.dll")
                .Where(file =>
                {
                    return !Path.GetFileName(file).StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                        && !Path.GetFileName(file).StartsWith("System", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            // Load assemblies that are not already loaded
            foreach(var assemblyFile in assemblyFiles)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);

                // Load assembly, if not already loaded
                if (loadedAssemblies.Any(a => a.GetName().Name == assemblyName.Name))
                {
                    try 
                    {
                        Assembly.Load(assemblyName);
                    }
                    catch 
                    {
                        // Ignore exception - assembly cannot be loaded
                    }
                }
            }

            // Get list of all loaded assemblies after loading new ones
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    var assemblyName = assembly.GetName().Name;
                    return assemblyName != null &&
                           !assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) &&
                           !assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();


            // Scan each assembly for GetServices() method and register each service
            foreach(var assembly in assemblies)
            {
                // Find the static method 'GetServices' in each assembly
                var serviceRegistrationType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "ServiceRegistration" && t.IsClass && t.IsPublic);

                if (serviceRegistrationType != null)
                {
                    // Get the GetServices()
                    var getServicesMethod = serviceRegistrationType.GetMethod("GetServices", BindingFlags.Public | BindingFlags.Static);
                    if (getServicesMethod != null)
                    {
                        // Invoke GetServices() to get list of services to register
                        var serviceList = getServicesMethod.Invoke(null, null) as IEnumerable<(Type Interface, Type Implementation)>;
                        if (serviceList != null)
                        {
                            // Register each service
                            foreach(var (interfaceType, implementationType) in serviceList)
                            {
                                if (interfaceType != null && implementationType != null)
                                {
                                    //   AddTransient - Create a new instance every time it is requested
                                    //   AddScoped    - One instance per HTTP request
                                    //   AddSingleton - One instance for the entire app
                                    //
                                    // Register as Transient : create new instance every time requested.
                                    // ================================================================================
                                    //   If changed to AddScoped or AddSingleton the possibility of concurrent 
                                    //   request issues may exist.  If any class-level fields are used to persist
                                    //   variables (for example, for remote debugging of Azure Web Apps), concurrent
                                    //   requests may overwrite the field values.
                                    // ================================================================================
                                    services.AddTransient(interfaceType, implementationType);

                                    // TODO : Support AddScoped and AddSingleton
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
