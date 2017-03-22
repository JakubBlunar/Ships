using System.Data.Entity;
using ServerInterfaces;

namespace ServerService
{
    /// <summary>
    /// Db context that represents database
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("ships")
        {
        }

        public DbSet<Game> Saves { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<GameMove> GameMoves { get; set; }
    }
}