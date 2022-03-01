Servicio OAI-PMH encargado de hacer las peticiones mediante la última fecha de modificación para la obtención de datos.

El servicio de OAI-PMH consta de varios métodos para obtener la información. Dichos métodos son:
- ListMetadataFormats
- ListSets
- ListIdentifiers
- GetRecords

Dichos métodos, por detrás hacen peticiones un API encargada de obtener y ofrecer los datos pedidos.
Los diversas peticiones a las que se hacen referencia están documentadas en ["Tratamiento de datos"](https://confluence.um.es/confluence/display/HERCULES/Tratamiento+de+datos)

# Obtención del Token Bearer
Antes de hacer las peticiones a los servicios correspondientes, es necesario el acceso por token. Dicho token se pedirá automaticamente, teniendo un tiempo de expiración de cinco minutos. Tras estos cinco minutos se volververá a hacer la petición de obtención de token para refrescarlo.

# ListMetadataFormats
Permite obtener el metadataPrefix utilizado para especificar los encabezados que deben devolverse.
No requiere ningún parámetro adicional de uso.

# ListSets
Obtiene el dato que especifica los criterios establecidos para la recolección selectiva.
No requiere ningún parámetro adicional de uso.

# ListIdentifiers
Devuelve una lista de identificadores de los datos solicitados (setSpec_ID) junto a la hora de actualización y el setSpec del dato solicitado.
Para la utilización de este método, es necesario los siguientes parámetros:
- metadataPrefix: Especifica que los encabezados deben devolverse solo si el formato de metadatos que coincide con el metadataPrefix proporcionado está disponible. Los formatos de metadatos admitidos por un repositorio y para un elemento en particular se pueden recuperar mediante la solicitud ListMetadataFormats. Ejemplo: EDMA
- from: Fecha de inicio desde la que se desean recuperar las cabeceras de las entidades (Codificado con ISO8601 y expresado en UTC, YYYY-MM-DD o YYYY-MM-DDThh:mm:ssZ). Ejemplo: 2022-01-01
- until: Fecha de fin hasta la que se desean recuperar las cabeceras de las entidades (Codificado con ISO8601 y expresado en UTC, YYYY-MM-DD o YYYY-MM-DDThh:mm:ssZ). Ejemplo: 2023-01-01
- set: Argumento con un valor setSpec, que especifica los criterios establecidos para la recolección selectiva. Los formatos de sets admitidos por un repositorio y para un elemento en particular se pueden recuperar mediante la solicitud ListSets. Ejemplo: Persona

# GetRecord
Devuelve los datos con el ID obtenido por el método ListIdentifiers.
Para la utilización de este método, es necesario los siguientes parámetros:
- identifier: Identificador de la entidad a recuperar (los identificadores se obtienen con el metodo ListIdentifiers). Ejemplo: Persona_ID-PERSONA
- metadataPrefix: Especifica que los encabezados deben devolverse solo si el formato de metadatos que coincide con el metadataPrefix proporcionado está disponible. Los formatos de metadatos admitidos por un repositorio y para un elemento en particular se pueden recuperar mediante la solicitud ListMetadataFormats. Ejemplo: EDMA

# Dependencias
- IdentityServer4: v4.1.2
- IdentityServer4.AccessTokenValidation: v3.0.1
- Microsoft.AspNetCore.HttpOverrides: v2.2.0
- Microsoft.AspNetCore.Mvc.Core: v2.2.5
- Microsoft.AspNetCore.Mvc.Formatters.Json: v2.2.0
- Microsoft.AspNetCore.Mvc.NewtonsoftJson: v5.0.10
- OaiPmhNet: v0.4.1
- RestSharp: v106.12.0
- Swashbuckle.AspNetCore: v6.2.2
- Swashbuckle.AspNetCore.Annotations: v6.2.2
- System.ServiceModel.Duplex: v4.8.1
- System.ServiceModel.Http: v4.8.1
- System.ServiceModel.NetTcp: v4.8.1
- System.ServiceModel.Security: v4.8.1
