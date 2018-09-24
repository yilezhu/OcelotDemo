using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OcelotDemo.Models;

namespace GoodApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodController : ControllerBase
    {
        // GET api/Good/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            var item = new Goods
            {
                Id = id,
                Content = $"{id}的关联的商品明细",
            };
            return JsonConvert.SerializeObject(item);
        }
    }
}