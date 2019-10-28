# Unscrew Project Structure

UPS project allows to synchronize Visual Studio solution folder structure to physical file system also fixing all file and project references.

### Global Tool

The `UPS` tool is invoked by specifying **full path** to the solution (.sln) file.

```
dotnet tool install --global dotnet-ups
```

Download from [NuGet](https://www.nuget.org/packages/dotnet-ups)

#### Solution
![Solution view](https://github.com/floatas/UPS/blob/master/Content/SlnView.PNG?raw=true "Solution view")

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

