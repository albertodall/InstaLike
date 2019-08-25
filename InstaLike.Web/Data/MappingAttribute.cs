using System;

namespace InstaLike.Web.Data
{
    /// <summary>
    /// Marker attribute to identify mapping classes suited for On-Premises database deployments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class OnPremDatabaseMappingAttribute : Attribute
    {
        public OnPremDatabaseMappingAttribute() { }
    }

    /// <summary>
    /// Marker attribute to identify mapping classes suited for cloud database deployments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class CloudDatabaseMappingAttribute : Attribute
    {
        public CloudDatabaseMappingAttribute() { }
    }
}
