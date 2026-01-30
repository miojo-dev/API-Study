using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API_Study.Controllers;

public record Product
{
    public int index;
    public string name;
    public string? description;
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
        string invalidPattern = "(?:(?:[;'\"\\\\/A-Z*])|--)";
        Regex regex = new Regex(invalidPattern);

        try
        {
            if (string.IsNullOrEmpty(newProduct.name))
            {
                return BadRequest("Name field must have a value grater than null");
            }

            if (regex.IsMatch(newProduct.name))
            {
                var charsFound = regex.Matches(newProduct.name)
                    .Select(m => m.Value).Distinct().ToList();

                string errorMsg = $"{{string.Join(\", \", charsFound)}}, characters were found and are not allowed";
                
                return BadRequest(errorMsg);
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