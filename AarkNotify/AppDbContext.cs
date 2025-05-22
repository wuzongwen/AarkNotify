using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AarkNotify
{
    public class AppDbContext : DbContext
    {
        public DbSet<NotifySettings> NotifySettings { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<SystemLogs> SystemLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotifySettings>().HasData(new NotifySettings
            {
                Id = 1,
                NotifyKey = "xxxxxx",
                NotifyPath = "https://open.feishu.cn/open-apis/bot/v2/hook/xxxxxxx",
                Secret = "xxxxx"
            });
            modelBuilder.Entity<SystemSettings>().HasData(new SystemSettings
            {
                Id = 1,
                Limit = 10,
                BlackListLimit = 15,
                BlackList = "1.1.1.1",
                WhiteList = "127.0.0.1",
                OpTime = DateTime.Now
            });
        }
    }

    public class NotifySettings
    {
        public int Id { get; set; }
        public string NotifyKey { get; set; }
        public string NotifyPath { get; set; }
        public string Secret { get; set; }
    }

    public class SystemSettings
    {
        public int Id { get; set; }
        public int Limit { get; set; }
        public int BlackListLimit { get; set; }
        public string BlackList { get; set; }
        public string WhiteList { get; set; }
        public DateTime OpTime { get; set; }
    }

    public class SystemLogs
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }
}
