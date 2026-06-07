using System;
using System.Data.Entity;

namespace BizRent.Data
{
    public class DataBaseContext : IDisposable
    {
        private readonly test3Entities _context;

        public DataBaseContext()
        {
            _context = new test3Entities();
        }

        public DbSet<Users> Users => _context.Users;
        public DbSet<Roles> Roles => _context.Roles;
        public DbSet<Areas> Areas => _context.Areas;
        public DbSet<AreaTypes> AreaTypes => _context.AreaTypes;
        public DbSet<AreaStatuses> AreaStatuses => _context.AreaStatuses;
        public DbSet<Tenants> Tenants => _context.Tenants;
        public DbSet<Contracts> Contracts => _context.Contracts;
        public DbSet<ContractTypes> ContractTypes => _context.ContractTypes;
        public DbSet<ContractStatuses> ContractStatuses => _context.ContractStatuses;
        public DbSet<Payments> Payments => _context.Payments;
        public DbSet<PaymentTypes> PaymentTypes => _context.PaymentTypes;
        public DbSet<PaymentStatuses> PaymentStatuses => _context.PaymentStatuses;
        public DbSet<Applications> Applications => _context.Applications;
        public DbSet<ApplicationStatuses> ApplicationStatuses => _context.ApplicationStatuses;

        public int SaveChanges() => _context.SaveChanges();
        public void Dispose() => _context?.Dispose();
        
    }
}