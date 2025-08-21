# github_actions_demo


<!-- [Include project github badges (build, license, release, codeql, sbom)] -->

# github_actions_demo project

<!-- [Write overall summary of this project] -->

## Table of Contents

- [github_actions_demo](#ots-dotnet-template-mvc-monolithic)
  - [Table of Contents](#table-of-contents)
  - [About the Project](#about-the-project)
  - [Project Status](#project-status)
  - [Getting Started](#getting-started)
    - [Getting the Source](#getting-the-source)
    - [Dependencies](#dependencies)
    - [Building](#building)
    - [Restore `dotnet tools`](#restore-dotnet-tools-packages)
    - [Database](#database)
      - [Update Asp.Identity tables](#update-aspidentity-tables)
    - [Running Tests](#running-tests)
    - [Usage](#usage) 
      - [Authentication](#authentication)
      - [Open Telemetry with New Relic](#open-telemetry-with-new-relic)
      - [Docker](#docker)    


## About the Project

<!-- [Include a diagram (eg mermaid) about this project as it relates to other projects/github repositories with links]   -->

**[Back to top](#table-of-contents)**

## Getting Started

The instructions provided below are intended to guide a new team member when setting 
up and running the project on their local machine. 

Initially, the project should be run on the OTS network due to an existing trust relationship with the Identity Provider (e.g., ADFS), 
which is configured to authenticate users on the local network exclusively. 
You can establish a new trust as detailed in the [authentication](#authentication) section.

<details>
 <summary>click to expand</summary> 


### Getting the Source

<!-- [Include details targeting a new team member and they can clone and run the project locally]  -->

This project is [on GitHub](https://github.com/otsgh/github_actions_demo). You can clone this project directly using this command:

From Git Bash

```shell
git clone git@github.com:otsgh/github_actions_demo.git
```

### Generate `css` and `js` files

```shell
npm install

# development environment
npm run dev
```

### Dependencies

Restore dependencies

```shell
dotnet restore
```

### Building

Instructions for how to build your project

```shell
dotnet build
```

### Restore `dotnet tools` 

```shell
dotnet tool restore
```

### Database

Note: If there is an error running the `dotnet ef database update` command, compare the version of dotnet-ef you have installed, with the version specified in dotnet-tools.json. These must match.

##### Update Asp.Identity tables

   ```shell
   dotnet ef database update --context ApplicationDbContext
   ```



### Running Tests

<!-- [How to run integration and unit tests must have a test project] -->

```shell
dotnet test
```

### Usage

#### Authentication

1. Replace the `ots.application.template` with a new certificate (`*.pfx)  by requesting a new one from the DS Administrator [DS.Administrator@la.gov](mailto:DS.Administrator@la.gov). This certificate protects the data protection key that is used to encrypt/decrypt session/cached data and cookies.  

1. Add the `*.pfx` to the `Ceritificates folder.
1. Update the `appsettings.{Environment}.json` with the certificate information (location/serial number).
1. Delete the existing `dev.Metadata` located in `wwwroot`.
1. Run the project, a new Metadata file will be generated.
1. Send DS Administrator [DS.Administrator@la.gov](mailto:DS.Administrator@la.gov) the newly generated Metadata file so that a trust can be created.
    1. Upon trust creation, when running the application it will be redirected to the Identity Provider (e.g ADFS) for credentials to log into the application.
1.Each authorized user will need the `IsEnabled` value to be set to `true` otherwise the user will be redirected to "Authorization" error page. 
This is to ensure that only authorized users can access the application.
The `IsEnabled` value is automatically set to `true` in the `Development` environment (for local development purposes).
1. Please run the following SQL command in other environments for any seeded user accounts:

```sql
UPDATE AspNetUsers
SET IsEnabled = 1
WHERE UserName = '[USERNAME]'
```




#### Open Telemetry with New Relic

OpenTelemetry is an open-source project that provides tools, APIs, and SDKs to instrument, generate,
collect, and export telemetry data (metrics, logs, and traces) from your software. 
It's designed to make it easy for developers to build observability into their systems. 
This data can be analyzed by developers and operators to understand the performance and behavior of 
their software, helping them to debug and optimize it.

##### Development Environment (Local Machine)

The following is set in appsettings.Development.json file to view logs on console.

```json
"OpenTelemetry": {
    "Enable": false, //this will disable OTEL tracing and metrics and will not output it to the console
    "SerilogWriteToProviders": false, //set to `true` if you want to write OTEL logs to the console
    "Otlp": {
       "Enable": false //this will only disable OTEL exporter and write to the console instead
     }
 }
```

### Non-Development Environment

The following is set in appsettings.[Environment].json file to send all three signals (metrics, logs, and traces) to New Relic.

```json
  "OpenTelemetry": {
      "Enable": true, //this will enable OTEL tracing and metrics and will not output it to the console
      "SerilogWriteToProviders": true,
      "Otlp": {
        "Enable": true, //this will only enable OTEL exporter and write to it and send to New Relic
        "EndpointUrl": "https://gov-otlp.nr-data.net:4317", //New Relic US FedRAMP OTLP endpoint
        "EndpointUrlHeader": {
          "api-key": "[NEW_RELIC_LICENSE_KEY]" //New Relic License Key
        }
      }
    }
```
**Steps**: 
1. Contact [Network Team](mailto:DOA-OTS-EUCNOC@la.gov) and/or [Infosec](mailto:InfoSecTeam@la.gov) to whitelist the following endpoint `https://gov-otlp.nr-data.net:4317`.
1. Obtain a `NEW_RELIC_LICENSE_KEY` from the New Relic Administrator. 
1. Add the `NEW_RELIC_LICENSE_KEY` to the corresponding `appsettings.{Environment}.json` file.
1. Run the application and navigate to the New Relic Dashboard to view the telemetry data at `https://one.newrelic.com/`.
1. The application will be instrumented with OpenTelemetry and will send the telemetry data to New Relic.
1. A new service name will be displayed under the `APM & Services` section under the `Services - OpenTelemetry` subsection. The default name will be in the format `[Environment].[Project Name]`. This service name can be modified under in the `DiagnosticsConfigurationExtensions.cs` file under `Setup`.
1. The telemetry data will encompass all three signals: metrics, logs, and traces. This data will be utilized to monitor the application and diagnose any potential issues.

#### Docker

The application can be run locally as a container using Docker. When running the application as a container, the application will be accessible at `http://localhost:8080`.
The local development and lower environment images will be based off of the `Ubuntu` image, while the any production image will be based off of the `Ubuntu Chiseled Images` (distroless). The
`Dockerfile` is located in the root of the project and can be used to build these images.
Must have Docker Desktop installed to build and run containers locally.

##### Ubuntu based images

###### Build image

```shell
docker build --target=final -t github_actions_demo-dev [Dockerfile_PATH]  
```

###### Run image

```shell
docker run -u 1654 --name github_actions_demo-mvc-dev --rm -p 8080:5000 -p 8081:5001 -e ASPNETCORE_HTTP_PORTS=http://+:5000 -e ASPNETCORE_HTTPS_PORTS=https://+:5001 -e ASPNETCORE_URLS=http://+:5000 -e ASPNETCORE_ENVIRONMENT="Development" github_actions_demo-dev --no-launch-profile
 ```

##### Ubuntu based images (production)

###### Build image

```shell
docker build --target=final-prod -t github_actions_demo-prod [Dockerfile_PATH]     
```

###### Run image

```shell
docker run -u 1654 --name github_actions_demo-mvc-prod --rm -p 8080:5000 -p 8081:5001 -e ASPNETCORE_HTTP_PORTS=http://+:5000 -e ASPNETCORE_HTTPS_PORTS=https://+:5001 -e ASPNETCORE_URLS=http://+:5000 -e ASPNETCORE_ENVIRONMENT="Production" github_actions_demo-prod --no-launch-profile
```

##### SQL Server Database

###### Pull SQL Server image

```
docker pull mcr.microsoft.com/mssql/server
``` 

###### Run Database image
```
 docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MyP@ssword" --name github_actions_demo-db -p 1433:1433  mcr.microsoft.com/mssql/server
```

</details>

**[Back to top](#table-of-contents)**

#### SQL Server without Docker

To use SQL Server database without Docker, update the `appsettings.*` files to use the "localdb" instead of "docker" DefaultConnection string.
