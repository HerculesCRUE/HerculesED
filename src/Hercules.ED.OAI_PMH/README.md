Servicio OAI-PMH encargado de hacer las peticiones mediante la última fecha de modificación para la obtención de datos.

Los diversas peticiones a las que se hacen referencia están documentadas en [Análisis de ítems de la norma CVN. Servicios de conexión.](https://confluence.um.es/confluence/pages/viewpage.action?pageId=397534572)

El servicio de OAI-PMH consta de varios métodos para obtener la información. Dichos métodos son:
- ListMetadataFormats
- ListSets
- ListIdentifiers

# Obtención del Token Bearer
Antes de hacer las peticiones a los servicios correspondientes, es necesario el acceso por token. Dicho token se pedirá automaticamente, teniendo un tiempo de expiración de cinco minutos. Tras estos cinco minutos se volververá a hacer la petición de obtención de token para refrescarlo.

# ListMetadataFormats
Permite obtener el metadataPrefix utilizado para especificar los encabezados que deben devolverse.
No requiere ninguún parámetro adicional de uso.

# ListSets
Obtiene el dato que especifica los criterios establecidos para la recolección selectiva.
No requiere ninguún parámetro adicional de uso.

# ListIdentifiers
Devuelve una lista de identificadores de los datos solicitados (setSpec_ID) junto a la hora de actualización y el setSpec del dato solicitado.
Para la utilización de este método, es necesario los siguientes parámetros:
- metadataPrefix: Especifica que los encabezados deben devolverse solo si el formato de metadatos que coincide con el metadataPrefix proporcionado está disponible o, según el soporte del repositorio para las eliminaciones, se ha eliminado. Los formatos de metadatos admitidos por un repositorio y para un elemento en particular se pueden recuperar mediante la solicitud ListMetadataFormats. Ejemplo: EDMA
- from: Fecha de inicio desde la que se desean recuperar las cabeceras de las entidades (Codificado con ISO8601 y expresado en UTC, YYYY-MM-DD o YYYY-MM-DDThh:mm:ssZ). Ejemplo: 2022-01-01
- until: Fecha de fin hasta la que se desean recuperar las cabeceras de las entidades (Codificado con ISO8601 y expresado en UTC, YYYY-MM-DD o YYYY-MM-DDThh:mm:ssZ). Ejemplo: 2023-01-01
- set: Argumento con un valor setSpec, que especifica los criterios establecidos para la recolección selectiva. Los formatos de sets admitidos por un repositorio y para un elemento en particular se pueden recuperar mediante la solicitud ListSets. Ejemplo: Persona


TODO: Terminar.
