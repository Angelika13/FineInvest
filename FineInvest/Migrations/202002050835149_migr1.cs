namespace FineInvest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migr1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Currencies", "NumCode", c => c.Int(nullable: false));
            AlterColumn("dbo.Currencies", "Nominal", c => c.Int(nullable: false));
            AlterColumn("dbo.Currencies", "Value", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Currencies", "Changes", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.Currencies", "Valute");
            DropTable("dbo.Rates");
        }
        
        public override void Down()
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
            
            AddColumn("dbo.Currencies", "Valute", c => c.String());
            AlterColumn("dbo.Currencies", "Changes", c => c.Double(nullable: false));
            AlterColumn("dbo.Currencies", "Value", c => c.String());
            AlterColumn("dbo.Currencies", "Nominal", c => c.String());
            AlterColumn("dbo.Currencies", "NumCode", c => c.String());
        }
    }
}
