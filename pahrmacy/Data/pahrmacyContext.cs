using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using pahrmacy.Models;

namespace pahrmacy.Data
{
    public class pahrmacyContext : DbContext
    {
        public pahrmacyContext (DbContextOptions<pahrmacyContext> options)
            : base(options)
        {
        }

        public DbSet<pahrmacy.Models.useraccounts> useraccounts { get; set; } = default!;
        public DbSet<pahrmacy.Models.customer> customer { get; set; } = default!;
        public DbSet<pahrmacy.Models.items> items { get; set; } = default!;
        public DbSet<pahrmacy.Models.items> getName { get; set; } = default!;
        public DbSet<pahrmacy.Models.orders> orders { get; set; } = default!;
        public DbSet<pahrmacy.report> report { get; set; } = default!;
        public DbSet<pahrmacy.orderline> orderline { get; set; } = default!;

    }
}
