using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WsdlViewer.Controllers;

public class HomeController(WsdlClient.WsdlClient wsdlClient) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("renderWsdl")]
    public async Task<IActionResult> RenderWsdl([FromForm] string? uri, IFormFile? wsdlFile)
    {
        var effectiveUri = await SaveFileOrUseUri(wsdlFile, uri);
        if (effectiveUri is null)
        {
            return BadRequest("Provide either a WSDL URL or upload a WSDL file.");
        }

        var xdoc = wsdlClient.GetWsdl(effectiveUri);
        var html = await Transformer.Transformer.Transform(xdoc);

        return View("Wsdl", html);
    }

    [HttpPost("renderXsd")]
    public async Task<IActionResult> RenderXsd([FromForm] string? uri, IFormFile? xsdFile)
    {
        var effectiveUri = await SaveFileOrUseUri(xsdFile, uri);
        if (effectiveUri is null)
        {
            return BadRequest("Provide either an XSD URL or upload an XSD file.");
        }

        var xdoc = wsdlClient.GetXsd(effectiveUri);
        var html = await Transformer.Transformer.TransformXsd(xdoc);

        return View("Xsd", html);
    }

    private static async Task<string?> SaveFileOrUseUri(IFormFile? file, string? uri)
    {
        if (file is { Length: > 0 })
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
            await using var stream = System.IO.File.Create(tempPath);
            await file.CopyToAsync(stream);
            return tempPath;
        }

        if (!string.IsNullOrWhiteSpace(uri))
        {
            return uri;
        }

        return null;
    }
}