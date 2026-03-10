using System.Net.Http;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace WsdlViewer.WsdlClient;

// TODO: Needs remote URI resolving
public class WsdlClient
{
    public XDocument GetXsd(string uri)
    {
        var baseUri = CreateBaseUri(uri);
        var schema = LoadSchemaFromUrl(baseUri, uri);
        return ConvertToXDocument(schema.Write);
    }

    public XDocument GetWsdl(string uri)
    {
        var baseUri = CreateBaseUri(uri);

        using var reader = XmlReader.Create(baseUri.AbsoluteUri);
        var serviceDescription = ServiceDescription.Read(reader);

        var schemaSet = new XmlSchemaSet();

        foreach (XmlSchema schema in serviceDescription.Types.Schemas)
            ResolveSchema(schema, schemaSet, baseUri);

        serviceDescription.Types.Schemas.Clear();
        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            serviceDescription.Types.Schemas.Add(schema);
        }
        
        return ConvertToXDocument(serviceDescription.Write);
    }

    private static void ResolveSchema(XmlSchema schema, XmlSchemaSet schemaSet, Uri baseUri)
    {
        var imports = schema.Includes.OfType<XmlSchemaImport>().ToArray();

        foreach (var import in imports)
        {
            if (string.IsNullOrWhiteSpace(import.SchemaLocation))
                continue;

            var resolvedLocation = ResolveLocation(baseUri, import.SchemaLocation);
            var loadedSchema = LoadSchemaFromUrl(baseUri, resolvedLocation);
            
            schema.Includes.Remove(import);
            schemaSet.Add(loadedSchema);

            ResolveSchema(loadedSchema, schemaSet, new Uri(resolvedLocation, UriKind.Absolute));
        }
    }
    
    private static XDocument ConvertToXDocument(Action<XmlWriter> write)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter);

        write(xmlWriter);
        
        xmlWriter.Flush();
        return XDocument.Parse(stringWriter.ToString());
    }

    private static XmlSchema LoadSchemaFromUrl(Uri baseUri, string location)
    {
        var resolved = ResolveLocation(baseUri, location);
        var uri = new Uri(resolved, UriKind.Absolute);

        // Use HttpClient for HTTP(S), XmlReader for file and other schemes
        if (uri.Scheme is "http" or "https")
        {
            using var httpClient = new HttpClient();
            using var stream = httpClient.GetStreamAsync(uri).GetAwaiter().GetResult();
            using var reader = XmlReader.Create(stream);

            return XmlSchema.Read(reader, (sender, e) =>
            {
                throw e.Exception;
            })!;
        }

        using var fileReader = XmlReader.Create(uri.AbsoluteUri);
        return XmlSchema.Read(fileReader, (sender, e) =>
        {
            throw e.Exception;
        })!;
    }

    private static Uri CreateBaseUri(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        var fullPath = Path.GetFullPath(uri);
        return new Uri(fullPath, UriKind.Absolute);
    }

    private static string ResolveLocation(Uri baseUri, string location)
    {
        if (Uri.TryCreate(location, UriKind.Absolute, out var absolute))
        {
            return absolute.AbsoluteUri;
        }

        // If baseUri is not absolute, fall back to combining with current directory
        if (!baseUri.IsAbsoluteUri)
        {
            var fullPath = Path.GetFullPath(location);
            return new Uri(fullPath, UriKind.Absolute).AbsoluteUri;
        }

        var combined = new Uri(baseUri, location);
        return combined.AbsoluteUri;
    }
}