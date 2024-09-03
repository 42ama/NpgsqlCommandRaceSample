using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NpgsqlCommandRaceSample;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<SavedBookInformation> Books { get; set; }
}

[Table("books")]
public class SavedBookInformation
{
    [Column("id")] public Guid Id { get; set; }
    [Column("book_id")] public long BookId { get; set; }
    
}