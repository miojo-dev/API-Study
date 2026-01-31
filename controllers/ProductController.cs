using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API_Study.Controllers;

public record Product
{
    public int index {get; set;}
    public string name {get; set;}
    public string? description {get; set;}
}

public class ProductController : ControllerBase
{
    private int _nextIndex = 1;
    private List<Product> _products = new List<Product>();

    [HttpGet]
    [Route("/products")]
    public ActionResult<Product> Get()
    {
        if (_products.Any())
        {
            return Ok(_products);
        }

        return NotFound("Error: no products found");
    }

    [HttpPost]
    [Route("/products")]
    public ActionResult<Product> Post([FromBody] Product newProduct)
    {
        // Debug Console
        // Console.WriteLine($"=== DEBUG ===");
        // Console.WriteLine($"newProduct is null? {newProduct == null}");
        // Console.WriteLine($"newProduct.name is null? {newProduct?.name == null}");
        // Console.WriteLine($"Request Content-Type: {Request.ContentType}");
        // Console.WriteLine($"newProduct: {newProduct}");
        // Console.WriteLine($"=== END DEBUG ===");
        
        string invalidPattern = "(?:(?:[;'\"\\\\/A-Z*])|--)";
        Regex regex = new Regex(invalidPattern);

        try
        {
            if (string.IsNullOrEmpty(newProduct.name))
            {
                return BadRequest("Name field is required");
            }

            if (regex.IsMatch(newProduct.name))
            {
                var charsFound = regex.Matches(newProduct.name)
                    .Select(m => m.Value).Distinct().ToList();
                
                string errorMsg = $"{string.Join(", ", charsFound)}, characters were found and are not allowed";
                
                return BadRequest(errorMsg);
            }

            if (!string.IsNullOrEmpty(newProduct.description))
            {
                if (regex.IsMatch(newProduct.description!))
                {
                    var charsFound = regex.Matches(newProduct.description!)
                        .Select(m => m.Value).Distinct().ToList();
                    
                    string errorMsg = $"{string.Join(", ", charsFound)}, characters were found and are not allowed";
                    
                    return BadRequest(errorMsg);
                }
            }

            newProduct.index = _nextIndex++;

            _products.Add(newProduct);

            return CreatedAtAction(nameof(Get), new { id = newProduct.index }, newProduct);
        }
        catch (ArgumentException error)
        {
            Console.WriteLine(error);
            return StatusCode(500, error);
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return StatusCode(500, new { error = "Unexpected error" });
        }

        return null;
    }
}