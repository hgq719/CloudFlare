using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Comm100CloudFlare.WebAPI.Controllers
{
    public class RulesController : ApiController
    {
        public ActionResult GetAttactIpList()
        {
            return Ok();
        } 
    }
}
