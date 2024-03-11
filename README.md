# Limbo Skyfish

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/limbo-works/Limbo.Umbraco.Skyfish/blob/v1/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/vpre/Limbo.Umbraco.Skyfish.svg)](https://www.nuget.org/packages/Limbo.Umbraco.Skyfish)
[![NuGet](https://img.shields.io/nuget/dt/Limbo.Umbraco.Skyfish.svg)](https://www.nuget.org/packages/Limbo.Umbraco.Skyfish)
[![Limbo.Umbraco.Skyfish at packages.limbo.works](https://img.shields.io/badge/limbo-packages-blue)](https://packages.limbo.works/limbo.umbraco.skyfish/)
<!--[![Umbraco Marketplace](https://img.shields.io/badge/umbraco-marketplace-%233544B1)](https://marketplace.umbraco.com/package/limbo.umbraco.skyfish)-->

**Limbo.Umbraco.Skyfish** is a video picker property editor for the Umbraco backoffice that allows users to insert videos from [**Skyfish**](https://www.skyfish.com/).

<table>
  <tr>
    <td><strong>License:</strong></td>
    <td><a href="https://github.com/limbo-works/Limbo.Umbraco.Skyfish/blob/v1/main/LICENSE.md"><strong>MIT License</strong></a></td>
  </tr>
  <tr>
    <td><strong>Umbraco:</strong></td>
    <td>
      Umbraco 10, 11 and 12
    </td>
  </tr>
  <tr>
    <td><strong>Target Framework:</strong></td>
    <td>
      .NET 6
    </td>
  </tr>
</table>






<br /><br />

## Installation

**Umbraco 10+**  

Version 1 of this package supports Umbraco version 10, 11, and 12. The package is only available via [**NuGet**](https://www.nuget.org/packages/Limbo.Umbraco.Skyfish/1.0.0-beta003).

To install the package, you can use either the .NET CLI:

```
dotnet add package Limbo.Umbraco.Skyfish --version 1.0.0-beta003
```

or the NuGet Package Manager:

```
Install-Package Limbo.Umbraco.Skyfish -Version 1.0.0-beta003
```





<br /><br />

## Configuration

Access to the Skyfish API requires a public key, secret key, username and a password. If you're logged into the Skyfish, you can find your public key and secret under [API credentials](https://www.skyfish.com/account/api).

In order for the package to access the API, you should add the credentials to the `Limbo:Skyfish:Credentials` array in your app settings. In `appsettings.json`, this would look like:

```json
{
  "Limbo": {
    "Skyfish": {
      "Credentials": [
        {
          "Key": "1c9850b8-1861-4f20-a62f-e24acbbac063",
          "Name": "{ClientName} Skyfish",
          "Description": "Credentials for accessing {ClientName}'s Skyfish account via {email}",
          "PublicKey": "...",
          "SecretKey": "...",
          "Username": "...",
          "Password": "..."
        }
      ]
    }
  }
}
```

`Key` should be a random but unique GUID key that helps identifying the crendetials. The values for `Name` and `Description` are currently not used, but that may change in the future.

With the current implementation, the package will always use the first set of `Credentials`, meaning multiple credentials are not directly supported.
