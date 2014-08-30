msbuild /target:Rebuild /property:Configuration=Release Dependable.sln
".nuget/nuget" pack -Symbols Dependable/Dependable.csproj
".nuget/nuget" pack -IncludeReferencedProjects Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj 
".nuget/nuget" pack -Symbols Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj 