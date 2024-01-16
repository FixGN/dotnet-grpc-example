using GrpcExample.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcExample.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ToDoItem> ToDoItems => Set<ToDoItem>();
}
