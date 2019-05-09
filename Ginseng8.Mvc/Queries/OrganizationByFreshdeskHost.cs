using System;
using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class OrganizationByFreshdeskHost : Query<Organization>
    {
        public OrganizationByFreshdeskHost() : base(
            @"SELECT TOP (1) *
			FROM [dbo].[Organization]			
			WHERE [FreshdeskUrl] LIKE Concat('%', @host, '%')")
        {
        }

        public string Url
        {
            set => Host = new Uri(value).Host;
        }

        public string Host { get; private set; }
    }
}
