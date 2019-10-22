# Unscrew Project Structure

UPS project allows to synchronize Visual Studio solution folder structure to physical file system also fixing all file and project references.

### Global Tool

The `UPS` tool is invoked by specifying the path to the solution (.sln) file.

```
dotnet tool install --global dotnet-ups --version 1.0.0
```

[NuGet](https://www.nuget.org/packages/dotnet-ups)


#### Before
```
├───DatabaseConnection
├───Helpers
│   └───MusicianLibrary
├───MusicLibrary
├───SpecialLibrary
└───StructurlFailure
```

#### After
```
├───DatabaseConnection
├───StructurlFailure
└───ThirdParty
    ├───MusicianLibrary
    ├───MusicLibrary
    └───SpecialFolder
        └───SpecialLibrary
```

