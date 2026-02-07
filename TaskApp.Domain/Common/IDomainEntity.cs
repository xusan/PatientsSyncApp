using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApp.Domain.Common
{
    public interface IDomainEntity
    {
        public int Id { get; set; }
    }
}
