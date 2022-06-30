![](./media/CabeceraDocumentosMD.png)

# Despliegue de Hércules ED con Kubernetes y Helm

A continuación se describen los pasos para desplegar Hercules ED en un cluster de Kubernetes: 

## Paso 1: Desplegar Nginx Ingress Controller

Hay diferentes formas de desplegar Nginx ingress controller:

Con Helm se puede desplegar con el siguiente comando:
  * helm upgrade --install ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx --namespace ingress-nginx --create-namespace

O sino con Kubectl:
  * kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.2.0/deploy/static/provider/cloud/deploy.yaml

Manual: https://kubernetes.github.io/ingress-nginx/deploy/


## Paso 2: Desplegar RabbitMQ

Para desplegar RabbitMQ primero clonaremos el contenido del repositorio RabbitMQ.

* Primero crearemos el namespace donde realizaremos el despliegue de nuestra plataforma. **¡¡IMPORTANTE!!** Éste generará un namespace con nombre **"edma-hercules"**.
  * kubectl apply -f ./namespace.yaml 
 
* Segundo, aplicamos las reglas RBAC:
  * kubectl apply -f rbac.yaml 

* Tercero, crearemos el servicio para RabbitMQ. Este servicio será de tipo ClusterIP y preparará los puertos 4369, 25672, 5672 y 15672 para la administración y gestión de RabbitMQ:
  *  kubectl apply -f ./service_rabbit.yaml

* Cuarto, crearemos los secretos para poder dar nombre de usuario y contraseña al administrador de RabbitMQ y a sus Erlang Cookie:
  - Nos aseguramos que estamos trabajando en nuestro namespace:
    - kubectl config set-context --current --namespace=edma-hercules
  - Para crear el secreto de la Cookie:
    - echo "this secret value is JUST AN EXAMPLE. Replace it!" > cookie
    - kubectl create secret generic erlang-cookie --from-file=./cookie
  - Para crear el secreto de administrador:
    - echo "gnoss" > user
    - echo "gnoss1234" > pass
    - kubectl create secret generic rabbitmq-admin --from-file=./user --from-file=./pass

* Quinto, aplicaremos el archivo configmap.yaml para aplicar una configuración básica en el nodo para nuestro RabbitMQ:
  - kubectl apply -f configmap.yaml

* Sexto, desplegamos RabbitMQ utilizando un statefulset para mantener la persistencia de datos:
  - kubectl apply -f statefulset.yaml

* Septimo, creamos si lo necesitamos un ingress para poder acceder a la administración de RabbitMQ. Por defecto la URL es rabbit.hercules.com
  - kubectl apply -f ingress.yaml

## Paso 3 Desplegar HERCULES-ED.

El despliegue de HERCULES-ED está preparado para ser realizado con HELM. 

* Primero utilizaremos el comando.
  * helm install <nombre_despligue> oci://docker.gnoss.com/helm-charts/edma-gnoss

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

Como paso final debemos abastecer de contenido al contenedor "interno".

Para ello usaremos el comando:

  * kubectl cp <local_file_path> <pod_name_interno>:/app/imagenes
