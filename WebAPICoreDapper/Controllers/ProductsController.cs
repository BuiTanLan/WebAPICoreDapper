using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebAPICoreDapper.Dtos;
using WebAPICoreDapper.Filters;
using WebAPICoreDapper.Models;
using Microsoft.Extensions.Logging;
using WebAPICoreDapper.Extensions;
using System.Globalization;
using Microsoft.Extensions.Localization;
using WebAPICoreDapper.Resources;

namespace WebAPICoreDapper.Controllers
{
    [Route("api/{culture}/[controller]")]
    [ApiController]
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class ProductController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductController> _logger;
        private readonly IStringLocalizer<ProductController> _localizer;
        private readonly LocService _locService;
        public ProductController(IConfiguration configuration, ILogger<ProductController> logger,
        IStringLocalizer<ProductController> localizer, LocService locService)
        {
            _connectionString = configuration.GetConnectionString("DbConnectionString");
            _logger = logger;
            _localizer = localizer;
            _locService = locService;
        }
        // GET: api/Product
        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            var culture = CultureInfo.CurrentCulture.Name;
            string text = _localizer["test"];
            string text1 = _locService.GetLocalizedHtmlString("ForgotPassword");
            _logger.LogTrace("Test product controller");
            using var conn = new SqlConnection(_connectionString);
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();
            var result = await conn.QueryAsync<Product>("Get_Product_All", null, null, null, System.Data.CommandType.StoredProcedure);
            return result;
        }

        // GET: api/Product/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<Product> Get(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                var result = await conn.QueryAsync<Product>("Get_Product_ById", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                return result.Single();
            }
        }

        [HttpGet("paging", Name = "GetPaging")]
        public async Task<PagedResult<Product>> GetPaging(string keyword, int categoryId, int pageIndex, int pageSize)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@keyword", keyword);
                paramaters.Add("@categoryId", categoryId);
                paramaters.Add("@pageIndex", pageIndex);
                paramaters.Add("@pageSize", pageSize);
                paramaters.Add("@totalRow", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                var result = await conn.QueryAsync<Product>("Get_Product_AllPaging", paramaters, null, null, System.Data.CommandType.StoredProcedure);

                int totalRow = paramaters.Get<int>("@totalRow");

                var pagedResult = new PagedResult<Product>()
                {
                    Items = result.ToList(),
                    TotalRow = totalRow,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                return pagedResult;
            }
        }
        // POST: api/Product
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@sku", product.Sku);
                paramaters.Add("@price", product.Price);
                paramaters.Add("@isActive", product.IsActive);
                paramaters.Add("@imageUrl", product.ImageUrl);
                paramaters.Add("@id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                var result = await conn.ExecuteAsync("Create_Product", paramaters, null, null, System.Data.CommandType.StoredProcedure);

                int newId = paramaters.Get<int>("@id");
                return Ok(newId);
            }

        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                paramaters.Add("@sku", product.Sku);
                paramaters.Add("@price", product.Price);
                paramaters.Add("@isActive", product.Sku);
                paramaters.Add("@imageUrl", product.ImageUrl);
                await conn.ExecuteAsync("Update_Product", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                return Ok();
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                await conn.ExecuteAsync("Delete_Product_ById", paramaters, null, null, System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
