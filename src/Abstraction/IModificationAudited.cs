public interface IModificationAudited
{
    string LastModifiedBy { get; set; }
    DateTime? LastModifiedTime { get; set; }
} 