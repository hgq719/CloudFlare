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
    public interface ITriggerlogdetailsAppService : IFrameworkAppService<TriggerlogdetailsDto, int>
    {
    }

    public class TriggerlogdetailsAppService : FrameworkAppService<Triggerlogdetails, TriggerlogdetailsDto, int>, ITriggerlogdetailsAppService
    {
        public TriggerlogdetailsAppService(ITriggerlogdetailsRepository repository)
            : base(repository)
        {

        }     
    }
}