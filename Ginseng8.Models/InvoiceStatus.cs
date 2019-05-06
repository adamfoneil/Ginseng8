using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
    public class InvoiceStatus : AppTable
    {
        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsClosed { get; set; }

        public static DataTable GetSeedData()
        {
            return new InvoiceStatus[]
            {
                new InvoiceStatus() { Name = "Outstanding" },
                new InvoiceStatus() { Name = "Paid", IsClosed = true },
                new InvoiceStatus() { Name = "Forgiven", IsClosed = true },
                new InvoiceStatus() { Name = "Canceled", IsClosed = true }
            }.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
        }
    }
}