using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.ComponentModel.DataAnnotations;

namespace FineInvest.Models
{
    public class ICSUser : IdentityUser
    {
        public string FIO { get; set; }
        public int Year { get; set; }
        public string RoleId { get; set; }
        public ICSRole Role { get; set; }

    }
    public class ICSUserManager : UserManager<ICSUser>
    {
        public ICSUserManager(IUserStore<ICSUser> store)
                : base(store)
        {
        }
        public static ICSUserManager Create(IdentityFactoryOptions<ICSUserManager> options,
                                                IOwinContext context)
        {
            EFDbContext db = context.Get<EFDbContext>();
            ICSUserManager manager = new ICSUserManager(new UserStore<ICSUser>(db));
            return manager;
        }
    }
    public class ICSRole : IdentityRole
    {
        public ICSRole() { }

        public string Description { get; set; }
    }
    class ICSRoleManager : RoleManager<ICSRole>
    {
        public ICSRoleManager(RoleStore<ICSRole> store)
                    : base(store)
        { }
        public static ICSRoleManager Create(IdentityFactoryOptions<ICSRoleManager> options,
                                                IOwinContext context)
        {
            return new ICSRoleManager(new
                    RoleStore<ICSRole>(context.Get<EFDbContext>()));
        }
    }
    public class EFDbContext : IdentityDbContext<ICSUser>
    {
        public EFDbContext() : base("ICS") { }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ArtType> ArtTypes { get; set; }
        public DbSet<Currency> CatCurrency { get; set; }
        public DbSet<Portfel> Portfels { get; set; }

        public static EFDbContext Create()
        {
            return new EFDbContext();
        }

    }
}