using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieRestService.Models;

    public class MovieContext : DbContext
    {
        public MovieContext (DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        public DbSet<MovieRestService.Models.Root> Root { get; set; } = default!;
        public DbSet<MovieRestService.Models.Recent> Recent { get; set; }
    }
