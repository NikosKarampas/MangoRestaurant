using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDto> list = new List<ProductDto>();

            var response = await _productService.GetAllProductsAsync<ResponseDto>();
            
            if (response != null && response.IsSuccess) 
            {
                list = JsonSerializer.Deserialize<List<ProductDto>>(Convert.ToString(response.Result));
            }

            return View(list);
        }
    }
}
