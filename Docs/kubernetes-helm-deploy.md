![](./media/CabeceraDocumentosMD.png)

| Fecha         | 29/6/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Despliegue de Hércules ED con Kubernetes y Helm| 
|Descripción|Guía de despliegue de Hércules ED mediante Kubernetes y Helm|
|Versión|1.1|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Despliegue de Hércules ED con Kubernetes y Helm

La arquitectura de componentes se puede consultar en [Arquitectura de Hércules ED](https://confluence.um.es/confluence/pages/viewpage.action?pageId=420085932)

A continuación se describen los pasos para desplegar Hercules ED en un cluster de Kubernetes: 

## Paso 1: Desplegar Nginx Ingress Controller

Hay diferentes formas de desplegar Nginx ingress controller:

Con Helm se puede desplegar con el siguiente comando:
  * helm upgrade --install ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx --namespace ingress-nginx --create-namespace

O sino con Kubectl:
  * kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.2.0/deploy/static/provider/cloud/deploy.yaml

Manual: https://kubernetes.github.io/ingress-nginx/deploy/


## Paso 2: Desplegar RabbitMQ

Para desplegar RabbitMQ primero nos descargamos todos los archivos del directorio https://github.com/HerculesCRUE/HerculesED/tree/main/Deploy/RabbitMQ.



* Primero crearemos el namespace donde realizaremos el despliegue de nuestra plataforma. **¡¡IMPORTANTE!!** Éste generará un namespace con nombre **"edma-hercules"**.
  * kubectl apply -f ./namespace.yaml 
 
* Segundo, aplicamos las reglas RBAC:
  * kubectl apply -f rbac.yaml 

* Tercero, aplicaremos el archivo configmap.yaml para aplicar una configuración básica en el nodo para nuestro RabbitMQ:
  *  kubectl apply -f configmap.yaml

* Cuarto, crearemos los secretos para poder dar nombre de usuario y contraseña al administrador de RabbitMQ y a sus Erlang Cookie:
  - Nos aseguramos que estamos trabajando en nuestro namespace:
    - kubectl config set-context --current --namespace=edma-hercules
  - Para aplicar los secretos usaremos:
    - kubectl apply -f .\mysecret.yaml
    - **!!IMPORTANTE!!** Será necesario establecer los valores de usuario y password en el archivo mysecret.yaml antes de ejecutar ese comando (codificados en base64) y debermos establecer esos mismos valores en la cadena de conexión a Rabbit del despliege Edma-Hercules dentro del archivo values.yaml usado en el paso 3.

* Quinto, crearemos el servicio para RabbitMQ. Este servicio será de tipo ClusterIP y preparará los puertos 4369, 25672, 5672 y 15672 para la administración y gestión de RabbitMQ. **¡¡IMPORTANTE!!** Este servicio debe ser creado antes de aplicar el statefulset de RabbitMQ:
  - kubectl apply -f ./service_rabbit.yaml

* Sexto, desplegamos RabbitMQ utilizando un statefulset para mantener la persistencia de datos:
  - kubectl apply -f statefulset.yaml
  - **!!IMPORTANTE!!** Antes de ejecutar este comando, es necesario establecer el valor del parámetro storageClassName. 

* Septimo, creamos si lo necesitamos un ingress para poder acceder a la administración de RabbitMQ. Por defecto la URL es rabbit.hercules.com
  - kubectl apply -f ingress.yaml

## Paso 3 Desplegar HERCULES-ED.

El despliegue de HERCULES-ED está preparado para ser realizado con HELM. 

* Primero utilizaremos el comando.
  * helm install <nombre_despligue> oci://docker.gnoss.com/helm-charts/edma-gnoss -f values.yaml

El archivo values.yaml lo puedes encontrar en https://github.com/HerculesCRUE/HerculesED/blob/main/Deploy/EDMA/values.yaml. Modifica todo lo que necesites según tu infraestructura y las caracterísiticas de tu cluster de Kubernetes antes de ejecutar el comando anterior. 

* Segundo. Una vez que PostgreSQL está desplegado debemos volcar la base de datos para que empiece a trabajar con ella.
Para ello usaremos el archivo “pg_dump_backup.sqlc” ubicado en la carpeta PostgreSQL.
  * Primero obtenemos el nombre de nuestro pod con:
    * kubectl get pod

  * Ahora ponemos el archivo dentro del contendor:
    * kubectl cp <local_file_path> <pod_name>:/var/lib/postgresql/data –c postgresql
 
  * Ahora ingresamos en el contenedor mediante el comando:
    * kubectl exec –it <pod_name> -c postgresql -- /bin/bash
 
  * Y nos dirigimos a “/var/lib/postgresql/data” que es donde hemos alojado nuestro backup:
    * cd /var/lib/postgresql/data

  * Y realizamos un volcado de este que consta de tres pasos:

    * 1-	Borrar la base  
      * psql -U postgres template1 -c 'drop database postgres;'

    * 2- Crearla de nuevo  
      * psql -U postgres template1 -c 'create database postgres;'

    * 3-	Volcarla  
      * pg_restore -C --clean --no-acl --no-owner -U "postgres" -d "postgres" < "./pg_dump_backup.sqlc"

 
* Tercero. Después copiamos los archivos de la base de datos de virtuoso en su contenedor:
  * kubectl cp <local_file_path> <namespace_name>/virtuoso:/opt/virtuoso-opensource/database –c virtuoso

Finalmente todo debería estar correctamente desplegado. Observar que hasta que las bases de datos no estén volcadas 
en sus contenedores se realizarán varios reinicios de los contenedores ya que necesitan de los datos de ellas.

## Paso 4 Abastecer las imagenes.

En este paso debemos abastecer de contenido al contenedor "interno".

Para ello usaremos el comando:

  * kubectl cp <local_file_path> <pod_name_interno>:/app/imagenes

## Paso 5 Abastecer archivo OAuthV3.

En este paso debemos colocar el archivo OAuthV3.config. Para ello utilizaremos el Pod con el contenedor "editorcv" el cual usa un PVC compartido con los diferentes Pods que necesitan de este archivo.

  * kubectl cp <local_file_path> <pod_name_editorcv>:/app/Config/ConfigOAuth/
 
## Paso 6 Abastecer el contenido del Pod con Documents.
 
 En este paso debemos colocar el contenido de la carpeta "documentacion" dentro del Statefulset de Documents.
 
 Para ello usaremos el comando:
   * kubectl cp <local_file_path> <pod_name_documents>:/app/Documentacion
 
## Paso 7 Abastecer el contenido del Pod con Archivo.
 
  En este paso debemos colocar el contenido de la carpeta "ontologias" dentro del Statefulset de Archivos.
 
 Para ello usaremos el comando:
   * kubectl cp <local_file_path> <pod_name_archivos>:/app/Ontologias

## Verificación

Una vez ejecutados todos los pasos, se puede verificar que EDMA está funcionando correctamente accediendo a la url edma.gnoss.com (o la dirección configurada en el archivo values.yaml). 

