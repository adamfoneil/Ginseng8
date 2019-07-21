using Postulate.Base.Interfaces;
using System;

namespace Ginseng.Models
{
    internal class SystemUser : IUser
    {
        public string UserName { get; set; }
        public DateTime LocalTime { get; set; }
    }
}