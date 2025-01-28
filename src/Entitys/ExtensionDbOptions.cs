namespace Si.EntityFramework.Extension.Entitys
{
    public class ExtensionDbOptions
    {
        /// <summary>
        /// 是否启用雪花ID
        /// </summary>
        public bool EnableSnowflakeId { get; set; } = false;

        /// <summary>
        /// 是否启用软删除
        /// </summary>
        public bool EnableSoftDelete { get; set; } = false;

        /// <summary>
        /// 是否启用审计功能
        /// </summary>
        public bool EnableAudit { get; set; } = false;

        /// <summary>
        /// 雪花ID的WorkerId
        /// </summary>
        public int WorkerId { get; set; } = 1;

        /// <summary>
        /// 雪花ID的DatacenterId
        /// </summary>
        public int DatacenterId { get; set; } = 1;

        /// <summary>
        /// 是否启用多租户
        /// </summary>
        public bool EnableMultiTenant { get; set; } = false;

        /// <summary>
        /// 忽略多租户过滤的实体类型
        /// </summary> 
        public List<Type> IgnoredMultiTenantTypes { get; set; } = new();
    }
}
