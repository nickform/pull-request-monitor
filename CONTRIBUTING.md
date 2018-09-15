# Contributing to Pull Request Monitor

[TODO]

## Strong-naming workaround to keep Visual Studio responsive

In order to take advantage of .NET's configuration management system, particularly when it comes to upgrading the pull request monitor assemblies on users' machines, it is necessary to strong name sign the assembly. Since several third-party dependencies are not strong name signed, this requires a brute-force approach to signing achieved through the Brutal.Dev.StrongNameSigner NuGet package. Unfortunately this setup seems to play badly with Visual Studio slowing things down to a crawl and causing builds to be triggered when they otherwise wouldn't be needed (_e.g._ every time a test is run).

As a workaround, developers can manually edit the ```...\vsprops\PullRequestMonitor.props``` file, replacing
```
    <SignAssembly>true</SignAssembly>
```
with
```
    <SignAssembly>false</SignAssembly>
```
and deleting the following line referencing ```Brutal.Dev.StrongNameSigner``` in both project.json files _before_ opening the solution file in Visual Studio. This gives a dramatic improvement in developer experience. Please do not check this change in as it will appear to users that Pull Request Monitor has lost their settings.
