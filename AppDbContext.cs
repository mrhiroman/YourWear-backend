﻿using Microsoft.EntityFrameworkCore;
using YourWear_backend.Entities;

namespace YourWear_backend;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<PublishedWear> PublishedWears { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
}