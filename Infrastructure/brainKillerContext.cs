﻿using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class brainKillerContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=localhost;user=root;database=brainkiller;port=3306;Connect Timeout=5;");
        }
    }

    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public ulong Welcome { get; set; }
        public ulong WelcomeDm { get; set; }
        public string WelcomeDmMessage { get; set; }
        public string Background { get; set; }
        public ulong Logs { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }

    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
}