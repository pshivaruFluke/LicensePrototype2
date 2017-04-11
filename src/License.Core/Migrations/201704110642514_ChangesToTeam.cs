namespace License.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangesToTeam : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TeamMember", "AdminId", "dbo.AspNetUsers");
            DropIndex("dbo.TeamMember", new[] { "AdminId" });
            CreateTable(
                "dbo.Team",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        AdminId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateIndex("dbo.TeamMember", "TeamId");
            AddForeignKey("dbo.TeamMember", "TeamId", "dbo.Team", "Id", cascadeDelete: true);
            DropColumn("dbo.TeamMember", "AdminId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TeamMember", "AdminId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.TeamMember", "TeamId", "dbo.Team");
            DropForeignKey("dbo.Team", "AdminId", "dbo.AspNetUsers");
            DropIndex("dbo.Team", new[] { "AdminId" });
            DropIndex("dbo.TeamMember", new[] { "TeamId" });
            DropTable("dbo.Team");
            CreateIndex("dbo.TeamMember", "AdminId");
            AddForeignKey("dbo.TeamMember", "AdminId", "dbo.AspNetUsers", "Id");
        }
    }
}
