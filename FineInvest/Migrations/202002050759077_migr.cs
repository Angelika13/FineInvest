namespace FineInvest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migr : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Rates",
                c => new
                    {
                        Cur_ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Cur_Abbreviation = c.String(),
                        Cur_Scale = c.Int(nullable: false),
                        Cur_Name = c.String(),
                        Cur_OfficialRate = c.Decimal(precision: 18, scale: 2),
                        Changes = c.Decimal(precision: 18, scale: 2),
                        curVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Cur_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Rates");
        }
    }
}
