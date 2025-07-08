using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankWebApi.Connections.Data
{
    public class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
    {
        public BankDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BankDbContext>();

            // Puedes modificar la cadena según lo que uses en desarrollo
            optionsBuilder.UseSqlite("Data Source=../BankWebApi.Connections/bankapp.db");

            return new BankDbContext(optionsBuilder.Options);
        }
    }
}

