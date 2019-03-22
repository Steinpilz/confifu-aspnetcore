using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Confifu.AspNetCore.WebHost.Controllers
{
    using global::Autofac.Core;

    [Route("")]
    [ApiController]
    public class TestController : ControllerBase
    {
        readonly ServiceA a;
        readonly ServiceB b;

        public TestController(ServiceA a, ServiceB b)
        {
            this.a = a;
            this.b = b;
        }

        // GET api/values
        [HttpGet]
        [Route("")]
        public object Test(string test = null)
        {
            return new
            {
                Now = DateTime.UtcNow,
                Test = test,
                A = this.a.Value,
                B = this.b.Value
            };
        }
    }
}
