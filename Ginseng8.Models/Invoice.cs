using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class Invoice : BaseTable, IFindRelated<int>
    {
        [PrimaryKey]
        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        [PrimaryKey]
        public int Number { get; set; }

        [References(typeof(Application))]
        public int ApplicationId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [References(typeof(InvoiceStatus))]
        public int StatusId { get; set; }

        public DateTime? StatusDate { get; set; }

        public Application Application { get; set; }
        public InvoiceStatus Status { get; set; }
        public Organization Organization { get; set; }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Application = commandProvider.Find<Application>(connection, ApplicationId);
            Status = commandProvider.Find<InvoiceStatus>(connection, StatusId);
            Organization = commandProvider.Find<Organization>(connection, OrganizationId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Application = await commandProvider.FindAsync<Application>(connection, ApplicationId);
            Status = await commandProvider.FindAsync<InvoiceStatus>(connection, StatusId);
            Organization = await commandProvider.FindAsync<Organization>(connection, OrganizationId);
        }
    }
}