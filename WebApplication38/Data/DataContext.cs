using Microsoft.EntityFrameworkCore;
using WebApplication38.Models;

namespace WebApplication38.Data;

public class DataContext : DbContext
{
    public DataContext() { }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<MovieActor> MovieActors { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasOne(uf => uf.Follower)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uf => uf.Following)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Movie)
                .WithMany()
                .HasForeignKey(w => w.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Movie)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MovieActor>(entity =>
        {
            entity.HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
