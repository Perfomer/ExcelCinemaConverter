using System.Data.Entity;

namespace ExcelConverter
{
    class ApplicationContext : DbContext
    {

        public ApplicationContext() : base("DefaultConnection")
        {
        }
        public DbSet<CinemaModel> Cinemas { get; set; }

    }
}
