using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Threading;

namespace ORM
{


    public partial class EntityModel : DbContext
    {
        public EntityModel()
            : base("name=TodoContent")
        {
            
        }

        public virtual DbSet<Todo> Todoes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
                      
        }
    }

}


