![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 27/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.GitHubConnect| 
|Descripción|Manual del servicio API Hercules.ED.GitHubConnect para la importación de ROs desde GitHub|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Versión inicial|

# Hercules.ED.GitHubConnect

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.GitHubConnect&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.GitHubConnect)

## Introducción
Microservicio encargado de conectar GitHub con HerculesED a través de un modelo de datos estandarizado, y obtener los diferentes repositorios del investigador

## Interfaz Swagger
Para realizar las pruebas, se ha implementado una interfaz Swagger con la ruta: https://[dominio]:[puerto]/swagger/index.html

## Estructura
La estructura es una estructura NVC con un endpoint para realizar las peticones. 


### Controladores
Tenemos un único controlador para realizar las peticiones y obtener así la información desde GitHub del investigador:

#### APIController
Este controlador contiene un único método, y es el encargado de obtener los repositorios y la información de GitHub y devolver el objeto con todos los repositorios


### Modelos
Los modelos se corresponden a los modelos de datos devueltos por GitHub:

#### DataGitHub
Modelo principal de los datos de GitHub
#### ObjEnriquecimiento
Modelo correspondiente al modelo de enriquecimiento de datos de los ROs obtenidos
#### Repositories
Clase que contiene los modelos de los repositorios del investigador con todos los datos necesarios

## Petición para el enriquecimiento de datos
Se realizará una petición GET desde la propia API ha GitHub para obtener los datos enriquecidos.
Ejemplo de petición curl:
```
curl -X 'GET' \
  'https://[host]:[port]/github/GetData?pUser=[user]&pToken=[GitHubToken]' \
  -H 'accept: application/json'
```
- user: El nombre de usuario de GitHub
- GitHubToken: El token es un token generado por el usuario con permisos para leer su información

*Crear tu token: https://github.com/settings/tokens*

*Podría haber limitaciones de peticiones por causa de las limitaciones de GitHub*

## Configuración del servicio
El servicio no requiere ninguna configuración especial.

## Dependencias
- Newtonsoft.Json
- Serilog.AspNetCore
- Swashbuckle.AspNetCore
