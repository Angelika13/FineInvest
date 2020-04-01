namespace FineInvest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FineInvest.Models.EFDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "FineInvest.Models.EFDbContext";
        }

        protected override void Seed(FineInvest.Models.EFDbContext context)
        {

        }
    }
}
