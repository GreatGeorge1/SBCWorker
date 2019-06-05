using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.EntityFrameworkCore
{
    public class ControllerDbContextFactory : IDesignTimeDbContextFactory<ControllerDbContext>
    {
        public ControllerDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ControllerDbContext>();
            // var configuration = 
            ControllerDbContextConfigurer.Configure(builder, @"Data Source = ../Worker/controllerdb.db");
            return new ControllerDbContext(builder.Options);
        }
    }
}
