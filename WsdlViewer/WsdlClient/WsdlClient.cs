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
        var schema = LoadSchemaFromUrl(uri);
        return ConvertToXDocument(schema.Write);
    }

    public XDocument GetWsdl(string uri)
    {
        using var reader = XmlReader.Create(uri);
        var serviceDescription = ServiceDescription.Read(reader);

        var schemaSet = new XmlSchemaSet();

        foreach (XmlSchema schema in serviceDescription.Types.Schemas)
            ResolveSchema(schema, schemaSet);

        serviceDescription.Types.Schemas.Clear();
        foreach (XmlSchema schema in schemaSet.Schemas())
        {
            serviceDescription.Types.Schemas.Add(schema);
        }
        
        return ConvertToXDocument(serviceDescription.Write);
    }

    private static void ResolveSchema(XmlSchema schema, XmlSchemaSet schemaSet)
    {
        var imports = schema.Includes.OfType<XmlSchemaImport>().ToArray();

        foreach (var import in imports)
        {
            // No need to resolve remotely
            if (string.IsNullOrWhiteSpace(import.SchemaLocation))
                continue;

            var loadedSchema = LoadSchemaFromUrl(import.SchemaLocation);
            
            schema.Includes.Remove(import);
            schemaSet.Add(loadedSchema);

            ResolveSchema(loadedSchema, schemaSet);
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

    private static XmlSchema LoadSchemaFromUrl(string url)
    { // TODO: Should remote resolve too
        using var reader = XmlReader.Create(url);
        return XmlSchema.Read(reader, (sender, e) =>
        {
            throw e.Exception;
        })!;
    }
}