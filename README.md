

# MORYX Demo Project
<p align="center">
    <a href="https://github.com/PHOENIXCONTACT/MORYX-Framework/tree/future">
        <img src="https://img.shields.io/badge/MORYX%2010-Check%20Out-0098A1?style=for-the-badge" alt="MORYX 10 Fully Open Source" />
    </a>
</p>
<p align="center">    
    <a href="https://stackoverflow.com/questions/tagged/moryx">
        <img src="https://img.shields.io/badge/stackoverflow-ask-orange.svg" alt="Stackoverflow">
    </a>    
    <a href="https://gitter.im/PHOENIXCONTACT/MORYX?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge">
        <img src="https://badges.gitter.im/PHOENIXCONTACT/MORYX.svg" alt="Gitter">
    </a>
</p>

Recommended repository for demonstrating a large portion of the functionality provided by the different MORYX modules and plugins. 
The project includes a simulated production of an `ElectronicDevice` product consisting of a `CircuitBoard` and a `Housing` component.
For producing such a product simulated `AssemblyCell`s, `SolderingCell`s (for manual and automatic soldering), `TestingCell`s and a `PackagingCell` are included.

All required configuration files and SQLite databases are already checked in to the repository, so you can start the project easily without any additional setup.

## Demo Quick Start
### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- Visual Studio 2022 or newer, [Visual Studio Code](https://code.visualstudio.com/) with the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) and [NuGet Package Manager extension](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager) or whatever your favorite IDE supporting .NET development is

### Build and Run 
1. Open the `Demo.sln` solution in Visual Studio or Visual Studio Code.
2. Set `StartProject.Asp` as the startup project.
3. Build the solution.
4. Run the application.

## Overview of the different projects
- *Moryx.ControlSystem.Demo*: Setup Triggers
- *Moryx.Demo*: Activities, Capabilities, ProductTypes...
- *Moryx.Demo.Tests*: Unit Tests
- *Moryx.Orders.Demo*: ProductsAssignments, RecipeAssignments...
- *Moryx.Products.Demo*: ProductImporters
- *Moryx.Resources.Demo*: Resources including their states
- *StartProject*: Starts the application, contains references to all projects except for tests

## Trouble Shooting

If you run into problems with the template or MORYX development in general, feel free to join our Gitter channel, ask on StackOverflow using the [`moryx`](https://stackoverflow.com/questions/tagged/moryx) tag or open an issue. 

<!-- ToDo: Add access management to repository

#### Adding Identity Management
To use the identity management feature of Moryx you need a running identity server.
By default the Demo uses the Moryx Identity Server.

In order to use the server either execute the `Moryx.Identity.IdentityServer.exe`, if you received a *.zip* directory with the respective files. 
Otherwise, clone the [IdentityManagement repository](https://git-ctvc.europe.phoenixcontact.com/moryx/identitymanagement) and follow the instructions under [Build and Run](https://git-ctvc.europe.phoenixcontact.com/moryx/identitymanagement#build-and-run-the-moryx-iam-server).

Note: The identity management is disabled when the Demo client is launched in debug mode, switch to release mode to demonstrate this feature.

After you have done this you can use the admin credentials specified in the project mentioned above to login at the Client. 

(*If you use a different identity server, you need to adjust the user management config in [here](src\Simulation.UI\Config)*)

#### Using a prefilled Identity Database
If you use the default Moryx Identity Server as described above, you can make use of a prefilled database of Users with Permissions and Roles.
In order to do that, import the database dump in (*data/IdentityServer.sql*) using the [Query Tool](https://www.pgadmin.org/docs/pgadmin4/development/query_tool.html) of pgAdmin on the IdentityServer database.
This database was created automatically in the previous step.

The database includes the following Users and Roles
| User | Password | Role |
|--------------|-----------------|------------|
| ProductsEditor | ProductsEditor1! | ProductsEditor |
| ResourcesEditor | ResourcesEditor1! | ResourcesEditor |
| MachineEditor | MachineEditor1! | ProductsEditor, ResourcesEditor |
| ProductsAdmin | ProductsAdmin1! | ProductsAdmin |
| ResourcesAdmin | ResourcesAdmin1! | ResourcesAdmin |
| MachineAdmin | MachineAdmin1! | ProductsAdmin, ResourcesAdmin |

An Editor role has all permissions in its respective UIs despite the permissions to Add or Import new entities or Show the AspectConfigurator.
These permissions are assigned to the Admin roles in addition to the permissions of the Editor roles.
The MachineAdmin and MachineEditor users combine the Editor and Admin roles of all UIs. -->
