using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalChattApp.Model;

namespace MinimalChattApp.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext (DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public DbSet<MinimalChattApp.Model.User> User { get; set; }
        public DbSet<MinimalChattApp.Model.Message> Message { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
