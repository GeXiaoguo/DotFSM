# Publishing to nuget.org
1. update the package version `<PackageVersion>1.0.1</PackageVersion>`
2. clean and rebuild the project
3. `cd` to the dir containing the `nupkg` and `snupkg` files
4. `nuget setApiKey xxxx`
4. `nuget push the-nupkg-file -Source https://api.nuget.org/v3/index`