using System;
using System.Reflection;
using System.Runtime.Loader;
using NLog;

namespace TrainingHelper.App.Plugins;

internal class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    public PluginLoadContext(string pluginPath)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        
        _logger.Warn($"Assembly {assemblyName} not found in plugin path.");

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }
        
        _logger.Warn($"Unmanaged DLL {unmanagedDllName} not found in plugin path.");
        
        return IntPtr.Zero;
    }
}