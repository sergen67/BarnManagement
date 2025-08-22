namespace BarnManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Animals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 30),
                        AgeDays = c.Int(nullable: false),
                        Gender = c.String(nullable: false, maxLength: 10),
                        LifetimeDays = c.Int(nullable: false),
                        IsAlive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Barns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TotalCapacity = c.Int(nullable: false),
                        CurrentAnimalCount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnimalId = c.Int(nullable: false),
                        ProductType = c.String(nullable: false, maxLength: 30),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 3),
                        ProducedAt = c.DateTime(nullable: false),
                        IsSold = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Animals", t => t.AnimalId, cascadeDelete: true)
                .Index(t => t.AnimalId);
            
            CreateTable(
                "dbo.Sales",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 3),
                        SoldAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.Binary(),
                        PasswordSalt = c.Binary(),
                        Role = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            CreateIndex("dbo.Users", "Username", unique: true, name: "IX_Users_Username");
            CreateIndex("dbo.Animals", "Type", name: "IX_Animals_Type");
            CreateIndex("dbo.Products", "ProductType", name: "IX_Products_ProductType");
            CreateIndex("dbo.Products", "IsSold", name: "IX_Products_IsSold");

          
            Sql(@"IF OBJECT_ID('dbo.vw_Balance','V') IS NOT NULL DROP VIEW dbo.vw_Balance;
          EXEC('CREATE VIEW dbo.vw_Balance AS SELECT ISNULL(SUM(UnitPrice*Quantity),0) AS Balance FROM dbo.Sales');");

         
            Sql(@"IF OBJECT_ID('dbo.tr_Sales_AfterInsert','TR') IS NOT NULL DROP TRIGGER dbo.tr_Sales_AfterInsert;
          EXEC('CREATE TRIGGER dbo.tr_Sales_AfterInsert ON dbo.Sales AFTER INSERT AS
                BEGIN SET NOCOUNT ON;
                UPDATE p SET IsSold = 1 FROM dbo.Products p JOIN inserted i ON i.ProductId = p.Id; END');");
        }
        
        public override void Down()
        {
            Sql("IF OBJECT_ID('dbo.tr_Sales_AfterInsert','TR') IS NOT NULL DROP TRIGGER dbo.tr_Sales_AfterInsert;");
            Sql("IF OBJECT_ID('dbo.vw_Balance','V') IS NOT NULL DROP VIEW dbo.vw_Balance;");
            DropForeignKey("dbo.Sales", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "AnimalId", "dbo.Animals");
            DropIndex("dbo.Sales", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "AnimalId" });
            DropTable("dbo.Users");
            DropTable("dbo.Sales");
            DropTable("dbo.Products");
            DropTable("dbo.Barns");
            DropTable("dbo.Animals");
        }
    }
}
