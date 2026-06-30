using Microsoft.EntityFrameworkCore;

namespace Helpr.Infrastructure.Persistence;

public class HelprDbContext(DbContextOptions<HelprDbContext> options) : DbContext(options)
{
}
