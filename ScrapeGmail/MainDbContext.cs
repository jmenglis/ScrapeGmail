using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ScrapeGmail.Models;

namespace ScrapeGmail {
    public class MainDbContext : DbContext {
        public MainDbContext() : base("name=DefaultConnection") {
        }
        public DbSet<DataModel> Users { get; set; }

    }
}