using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WsdlViewer.Models;

namespace WsdlViewer.Controllers;

public class HomeController(WsdlClient.WsdlClient wsdlClient) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("renderWsdl")]
    public async Task<IActionResult> RenderWsdl([FromQuery] [Required] string uri)
    {
        var xdoc = wsdlClient.GetWsdl(uri);
        var html = await Transformer.Transformer.Transform(xdoc);

        return Content(html, "text/html");
    }

    [HttpGet("renderXsd")]
    public async Task<IActionResult> RenderXsd([FromQuery] [Required] string uri)
    {
        var xdoc = wsdlClient.GetXsd(uri);
        var html = await Transformer.Transformer.TransformXsd(xdoc);

        return Content(html, "text/html");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}