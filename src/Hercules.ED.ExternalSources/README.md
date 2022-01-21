Librería de conexión con repositorios externos
 
Los microservicios de Scopus, WoS, CrossRef, OpenCitations, Semantic Scholar, Zenodo tienen un funcionamiento similar. Todos ellos tienen una estructura en la que: (En este ejemplo se usará el nombre WoS pero este nombre se puede sustituir por el del microservicio a implementar o revisar.)
- Primeramente se llamará a la api principal, mediante las peticiones http declaras en *ROWoSLogic*.  
- El texto devuelto será convertido a un objeto de C#, para ello el modelo devuelto de almacenará en la clase definida en ROs/WoS/Models/ModeloInicial
- Se llamará a las funcionalidades  de *ROWoSCambioModelo* que nos permitirá cambiar el modelo devuelto por el servicio externo al modelo final deseado.
    - Este modelo final es igual en todos los microservicios, el modelo está guardado en ROs/WoS/Models/ROPublicationModel. Si se realiza una modificación de este fichero en un microservicio se deberá hacer en todos y cada uno de ellos. Además por cada tipo de RO existirá un modelo diferente.
    - La única excepción de este punto es el repositorio de Zenodo, con el cual únicamente devolvemos un string con el enlace al pdf en caso de que este exista.
- Una vez tenemos el modelo final con toda la información simplemente se devuelve el modelo generado.
 
El microservicio más distintivo y diferente es el de publicación, en él se llama de una forma específica, siguiendo el algoritmo diseñado, a los microservicios descritos anteriormente. La mayor diferencia es que en este caso en vez de cambiar el modelo, las publicaciones obtenidas por las diversas fuentes externas van a ir convergiendo de una forma específica según el diseño del algoritmo. De esta tarea en específico se encarga el código de *ROPublicactionLogic*.
- En este caso además de combinar las diferentes publicaciones obtenidas en los microservicios también se llamarán a los desarrollos realizados que permiten el enriquecimiento de estas publicaciones.
 
Como este microservicio también debe seguir el formato final que tenían los otros microservicios, este modelo está almacenado en ROs/Publication/Models/ROPublicacionModel.
- En este caso el formato sí que es un poco diferente porque se han introducido dos entidades dentro del modelo que define una publicación que no están en los otros microservicios. Estas entidades son aquellas que modelan los metadatos enriquecidos.
 
 

