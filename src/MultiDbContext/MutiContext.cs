using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.DataBase.Configuration;
using Si.EntityFramework.Extension.MultiDbContext.Configuration;

namespace Si.EntityFramework.Extension.MultiDbContext
{
    public class MutiContext : ApplicationDbContext
    {
        private readonly MutiDbConfiguration mutiDbConfiguration;
        public MutiContext(MutiDbConfiguration mutiDbConfiguration,DbContextOptions options, ExtensionDbOptions siOptions, IUserInfo sessions = null) : base(options, siOptions, sessions)
        {
        }

    }
}
