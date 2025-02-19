using System;

namespace Si.EntityFramework.Extension.MultiDbContext.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReadOnlyAttribute : Attribute
    {
        public bool ForceMaster { get; set; } = false;
    }
}
