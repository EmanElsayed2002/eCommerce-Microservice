using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Models;

using Microsoft.EntityFrameworkCore;

namespace eCommerce.DataAccessLayer.MyDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
