using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Elasticsearch.Data.Entities;
using Nest;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace Elasticsearch.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {

         private static readonly ConnectionSettings connSettings = new ConnectionSettings(new Uri("http://192.168.99.100:9200/"))
            .DefaultIndex("default")
         //Optionally override the default index for specific types
            .MapDefaultTypeIndices(m => m
            .Add(typeof(Product), "default"));
        private static readonly ElasticClient elasticClient = new ElasticClient(connSettings);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]Product model)
        {
            if (ModelState.IsValid)
            {
                model.Id = Guid.NewGuid();
                var res = await elasticClient.IndexAsync(model);
                string term = model.Id.ToString();

                System.Threading.Thread.Sleep(1000);

                var sres = await elasticClient.SearchAsync<Product>(x => x
                 .Query(q => q.
                    SimpleQueryString(qs => qs.Query(term))));
                if (!res.IsValid)
                {
                    throw new InvalidOperationException(res.DebugInformation);
                }

                return Json(sres.Documents);

            }
            else { 
                return BadRequest();
            }
        }

        [HttpGet("find")]
        public async Task<IActionResult> Find(string term)
        {
            var res = await elasticClient.SearchAsync<Product>(x => x
                 .Query(q => q.
                    SimpleQueryString(qs => qs.Query(term))));
            if (!res.IsValid)
            {
                throw new InvalidOperationException(res.DebugInformation);
            }

            return Json(res.Documents);
        }
    }
}
