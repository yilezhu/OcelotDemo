using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcelotDemo.Models;
using Newtonsoft.Json;

namespace OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        // GET api/Order/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            var item = new Orders {
                Id=id,
                Content=$"{id}的订单明细",
            };
            return JsonConvert.SerializeObject(item);
        }
    }
}