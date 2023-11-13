using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts;

namespace Shared.Kernel.Extensions;

public static class EventStoreExtensions
{
    public static string GenerateStreamName<T>() where T :  IEventData
    {
        var fullName = typeof(T).FullName.EmptyIfNull();
        return  fullName.Replace(".","-").Slugify(); 
    }
    
    
    public static string GenerateSubscriptionGroupName<T>() where T :  IEventData
    {
        return  typeof(T).Name.Replace(".","-").Slugify(); 
    }

    private static string Slugify(this string str)
    {
        return string.IsNullOrEmpty(str) ? string.Empty : str.SplitCamelCase().ToSlug();
    }
}