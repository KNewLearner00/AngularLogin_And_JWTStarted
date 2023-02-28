using AngularAuthApi.Context;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepo<Product> _productObj;

        public SellerController(IGenericRepo<Product> productObj)
        {
            _productObj = productObj;
        }

        [HttpGet("DisplayAllProducts")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productObj.GetAll();
            return Ok(products);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> Create(Product product)
        {
            await _productObj.Create(product);
            return Ok(product);
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            var existingProduct = await _productObj.GetById(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.ProductName = product.ProductName;
            existingProduct.Image = product.Image;
            existingProduct.Price = product.Price;

            await _productObj.Update(id, existingProduct);
            return Ok(product);
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProduct = await _productObj.GetById(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            await _productObj.Delete(id);
            return NoContent();
        }
    }
}

