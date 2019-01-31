using System;

namespace AlkaronEngine.Scene
{
    [global::System.AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class NoStoreAttribute : Attribute
    {
        public NoStoreAttribute()
        {
        }
    }
}
