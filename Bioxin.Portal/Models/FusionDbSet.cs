using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database = System.Data.Entity.Database;

namespace WebSolution.Models
{
    public class FusionDbSet :DbContext
    {
        public FusionDbSet()
    : base("FusionDbSet")
        {
            Database.SetInitializer<FusionDbSet>(null);
        }

        //public DbSet<tbl_OCRG> oCRGs { get; set; }
        //public DbSet<tbl_OCTG> oCTGs { get; set; }
        public DbSet<PrimaryData.OACT> oACTs { get; set; }
        //public DbSet<tbl_OPLN> oPLNs { get; set; }
        //public DbSet<tbl_OCRY> oCRYs { get; set; }
        //public DbSet<tbl_OCST> oCSTs { get; set; }
        //public DbSet<tbl_OCRN> oCRNs { get; set; }
        //public DbSet<tbl_OSHP> oSHPs { get; set; }
        //public DbSet<tbl_OSLP> oSLPs { get; set; }
        public DbSet<SalesOrder> salesOrders { get; set; }
        public DbSet<BusinessPartner> businessPartners { get; set; }
    }
}