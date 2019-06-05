using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Worker.EntityFrameworkCore
{
    public static class ControllerDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<ControllerDbContext> builder, string connectionString)
        {
            //builder.UseSqlite("Data Source = controllerdb.db; Version = 3; BinaryGUID = False;");
            builder.UseSqlite(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<ControllerDbContext> builder, DbConnection connection)
        {
            builder.UseSqlite(connection);
        }

    }
}
