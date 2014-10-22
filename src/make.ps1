if(Test-Path .build){
	Remove-Item .build -Force -Recurse
}
New-Item .build -Type Directory
$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"
$nuget = ".nuget\nuget.exe"
&$msbuild /target:Rebuild /p:Configuration=Release Dependable.sln

&$nuget pack -Symbols -Prop Configuration=Release Dependable/Dependable.csproj -OutputDirectory .build

&$nuget pack -IncludeReferencedProjects -Prop Configuration=Release Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj -OutputDirectory .build
&$nuget pack -Symbols -Prop Configuration=Release Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj -OutputDirectory .build

&$nuget pack -IncludeReferencedProjects -Prop Configuration=Release Extensions/Dependable.Extensions.Dependencies.Autofac/Dependable.Extensions.Dependencies.Autofac.csproj -OutputDirectory .build
&$nuget pack -Symbols -Prop Configuration=Release Extensions/Dependable.Extensions.Dependencies.Autofac/Dependable.Extensions.Dependencies.Autofac.csproj -OutputDirectory .build