using Abp.Domain.Repositories;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public interface IActionsRepository : IRepository<Actions, int>
    {
    }
}