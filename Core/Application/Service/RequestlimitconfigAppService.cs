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
    public interface IRequestlimitconfigAppService : IFrameworkAppService<RequestlimitconfigDto, int>
    {
    }

    public class RequestlimitconfigAppService : FrameworkAppService<Requestlimitconfig, RequestlimitconfigDto, int>, IRequestlimitconfigAppService
    {
        public RequestlimitconfigAppService(IRequestlimitconfigRepository repository)
            : base(repository)
        {

        }     
    }
}