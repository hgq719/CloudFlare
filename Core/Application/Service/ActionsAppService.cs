using Abp.Application.Services;
using Abp.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Application;
using Abp.Domain.Repositories;
using CoundFlareTools.Core.Domain.Model;

namespace CoundFlareTools.Core
{
    public interface IActionsAppService : IFrameworkAppService<ActionsDto, int>
    {
    }

    public class ActionsAppService : FrameworkAppService<Actions, ActionsDto, int>, IActionsAppService
    {
        public ActionsAppService(IActionsRepository repository)
            : base(repository)
        {

        }     
    }
}