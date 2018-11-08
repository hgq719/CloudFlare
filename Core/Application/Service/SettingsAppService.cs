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
    public interface ISettingsAppService : IFrameworkAppService<SettingsDto, int>
    {
    }

    public class SettingsAppService : FrameworkAppService<Settings, SettingsDto, int>, ISettingsAppService
    {
        public SettingsAppService(ISettingsRepository repository)
            : base(repository)
        {

        }     
    }
}