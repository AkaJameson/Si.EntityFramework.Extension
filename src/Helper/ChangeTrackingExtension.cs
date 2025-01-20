using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Si.EntityFramework.Extension.Helper
{
    public static class ChangeTrackingExtension
    {
        /// <summary>
        /// 获取所有被修改的实体
        /// </summary>
        /// <param name="changeTracker"></param>
        /// <returns></returns>
        public static IEnumerable<EntityEntry> GetModifiedEntries(this ChangeTracker changeTracker)
        {
            return changeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);
        }
        /// <summary>
        /// 获取被修改的属性
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyEntry> GetModifiedProperties(this EntityEntry entry)
        {
            return entry.Properties
                .Where(p => p.IsModified && p.CurrentValue != p.OriginalValue);
        }
        /// <summary>
        /// 获取被修改的属性的新旧值
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static IDictionary<string, (object Original, object Current)> GetModifiedValues(
       this EntityEntry entry)
        {
            return entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => (p.OriginalValue, p.CurrentValue));
        }
    }
}
