using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace WsdlViewer.Transformer;

public static class Transformer
{
    public static async Task<string> TransformXsd(XDocument document)
    {
        var resolver = new EmbeddedResourceXmlUrlResolver();
        var settings = new XsltSettings
        {
            EnableDocumentFunction = true,
            EnableScript = false
        };

        var argumentList = new XsltArgumentList();
        argumentList.AddParam("ANTIRECURSION-DEPTH", string.Empty, 10);

        var transform = new XslCompiledTransform();

        transform.Load("src/wsdl-viewer-xsd-tree.xsl", settings, resolver);

        await using var stringWriter = new StringWriter();
        await using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Async = true }))
        {
            transform.Transform(document.CreateReader(), argumentList, xmlWriter, resolver);
        }

        return stringWriter.ToString();
    }
    
    public static async Task<string> Transform(XDocument document)
    {
        var resolver = new EmbeddedResourceXmlUrlResolver();
        var settings = new XsltSettings
        {
            EnableDocumentFunction = true,
            EnableScript = false
        };

        var argumentList = new XsltArgumentList();

        var transform = new XslCompiledTransform();

        transform.Load("wsdl-viewer.xsl", settings, resolver);

        await using var stringWriter = new StringWriter();
        await using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Async = true }))
        {
            transform.Transform(document.CreateReader(), argumentList, xmlWriter, resolver);
        }

        return stringWriter.ToString();
    }

    private class EmbeddedResourceXmlUrlResolver : XmlUrlResolver
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
        {
            var resourceNamespace = Assembly
                .GetManifestResourceNames()
                .SingleOrDefault(n => n.Contains(absoluteUri.Segments.Last()));

            return Assembly.GetManifestResourceStream(resourceNamespace!)!;
        }
    }
}