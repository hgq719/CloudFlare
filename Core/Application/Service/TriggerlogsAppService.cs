﻿using Abp.Application.Services;
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
    public interface ITriggerlogsAppService : IFrameworkAppService<TriggerlogsDto, int>
    {
    }

    public class TriggerlogsAppService : FrameworkAppService<Triggerlogs, TriggerlogsDto, int>, ITriggerlogsAppService
    {
        public TriggerlogsAppService(ITriggerlogsRepository repository)
            : base(repository)
        {

        }     
    }
}