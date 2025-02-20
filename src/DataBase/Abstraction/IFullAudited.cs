public interface IFullAudited : ICreationAudited, IModificationAudited, ISoftDelete
{
    string? DeletedBy { get; set; }
} 