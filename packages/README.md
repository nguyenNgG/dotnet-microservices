NuGet Packages for common code would be here. (not pushed to GitHub repository)

Create package:
```
dotnet pack -o <path-to-this-directory>
```

Add this folder as a source for NuGet to find packages:
```
dotnet nuget add source <path-to-this-directory> -n PlayEconomy
```