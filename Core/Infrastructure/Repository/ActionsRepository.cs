using Abp.Dependency;
using Abp.NHibernate;
using Abp.NHibernate.Repositories;
using Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framwork.Infrastructure.Repository;
using CoundFlareTools.Core.Domain.Model;

namespace CoundFlareTools.Core
{
    public class ActionsRepository : FrameworkRepository<Actions, int>, IActionsRepository
    {
        public ActionsRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }
    }
}