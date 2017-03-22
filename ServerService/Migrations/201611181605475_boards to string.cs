namespace ServerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class boardstostring : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Game_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Games", t => t.Game_Id)
                .Index(t => t.Game_Id);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        State = c.Int(nullable: false),
                        Player1 = c.String(),
                        Player2 = c.String(),
                        Player1Turn = c.Boolean(nullable: false),
                        Player1Lives = c.Int(nullable: false),
                        Player2Lives = c.Int(nullable: false),
                        Board1s = c.String(),
                        Board2s = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GameMoves",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PlayerName = c.String(),
                        X = c.Int(nullable: false),
                        Y = c.Int(nullable: false),
                        Result = c.Int(nullable: false),
                        Game_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Games", t => t.Game_Id)
                .Index(t => t.Game_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Histories", "Game_Id", "dbo.Games");
            DropForeignKey("dbo.GameMoves", "Game_Id", "dbo.Games");
            DropIndex("dbo.GameMoves", new[] { "Game_Id" });
            DropIndex("dbo.Histories", new[] { "Game_Id" });
            DropTable("dbo.GameMoves");
            DropTable("dbo.Games");
            DropTable("dbo.Histories");
        }
    }
}
