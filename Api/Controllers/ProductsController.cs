using Core.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/V1.0/[controller]")] // "api/V1.0/products/"  // url
    [ApiController]
    public class ProductsController : ControllerBase
    {
        //private readonly IProductService _productService;
        private readonly IServiceUnitOfWork _services;
        public ProductsController(/*IProductService productService*/IServiceUnitOfWork services)
        {
            _services = services;
            //_productService = productService;
        }

        [Authorize(Policy = "AllPolicy")]
        [HttpGet("GetAll")] // api/V1.0/products/GetAll
        public async Task<IActionResult> GetAll()
        {
            var products = await _services.productService.GetAllAsync();
            return Ok(products);
        }
        [Authorize(Policy = "AllPolicy")]
        [HttpGet("getById/{id}")] // api/V1.0/products/getById/{id}
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _services.productService.GetByIdAsync(id);
                if(product is null)
                    return NotFound(new { statusCode = 404, message = $"Product with id : {id} not found" });
                return Ok(new { statusCode = 200, message= "done!" , data= product });
            }
            catch (Exception ex)
            {
                return BadRequest(new {statusCode = 400 ,message = ex.Message });
            }
        }
        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpPost("Add")]  // api/V1.0/products/Add
        public async Task<IActionResult> Add( Product model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    await _services.productService.AddAsync(model);
                    return Ok(new { statusCode = 200, message = "Product added successfully!" });
                }
                return BadRequest(new { statusCode = 400, message = "Data is not valid!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }
        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpPut("Edit")]   // api/V1.0/products/Edit
        public async Task<IActionResult> Update(Product model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _services.productService.UpdateAsync(model);
                    return Ok(new { statusCode = 200, message = "Product updated successfully!" });
                }
                return BadRequest(new { statusCode = 400, message = "Data is not valid!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("Delete/{id}")]   // api/V1.0/products/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if(_services.productService.GetByIdAsync(id) is null)
                    return NotFound(new { statusCode = 404, message = $"Product with id : {id} not found" });
                await _services.productService.DeleteAsync(id);
                return Ok(new { statusCode = 200, message = "Product deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }




    }
}
