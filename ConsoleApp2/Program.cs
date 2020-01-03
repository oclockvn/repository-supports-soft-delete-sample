using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var id = 0;

            // Add
            using (var db = new ApplicationDbContext())
            {
                var p = new Product
                {
                    Name = "Nokia 7",
                    Price = 7_000_000m,
                    UpdatedAt = DateTime.Now
                };

                db.Products.Add(p);
                db.SaveChanges();

                id = p.Id;
            }

            // Update
            using (var db = new ApplicationDbContext())
            {
                var repo = new Repository<Product>(db);
                var p = new Product
                {
                    Id = id,
                    Description = "updating..."
                };

                //db.Entry(p).State = EntityState.Modified;...no need anymore
                repo.Update(p, new List<Expression<Func<Product, object>>>
                {
                    x => x.Description
                });
                var updated = db.SaveChanges();

                Console.WriteLine($"> {updated} records saved");
            }

            // Delete
            using (var db = new ApplicationDbContext())
            {
                var repo = new Repository<Product>(db);
                var p = new Product
                {
                    Id = id
                };

                repo.Delete(p);
                var updated = db.SaveChanges();

                Console.WriteLine($"> {updated} records deleted");
            }

            Console.ReadLine();
        }
    }

    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
            .AddConsole()
            .AddDebug()
            .AddFilter(DbLoggerCategory.Name, LogLevel.Information)
            ;
        }
);

        public ApplicationDbContext() : base()
        {

        }

        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .UseSqlServer("Server=.\\sqlexpress;Database=EfSoftDelete;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(x => x.DeletedAt == null);
        }
    }

    public class Repository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext db;
        private readonly DbSet<T> table;

        public Repository(ApplicationDbContext db)
        {
            this.db = db;
            table = db.Set<T>();
        }

        public T Update(T entity, List<Expression<Func<T, object>>> updateProps)
        {
            // if I pass properties here
            // that's mean I just wanna update them
            if (updateProps?.Count > 0)
            {
                table.Attach(entity);
                foreach (var prop in updateProps)
                {
                    db.Entry(entity).Property(prop).IsModified = true;
                }
            }
            else // otherwise update all
            {
                db.Entry(entity).State = EntityState.Modified;
            }

            return entity;
        }

        public T Delete(T entity, bool really = false)
        {
            if (really)
            {
                return table.Remove(entity).Entity;
            }

            entity.DeletedAt = DateTime.Now;
            return Update(entity, new List<Expression<Func<T, object>>>
            {
                x => x.DeletedAt
            });
        }
    }
}
