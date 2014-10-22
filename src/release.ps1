if(Test-Path .build){
	Remove-Item .build -Force -Recurse
}
New-Item .build -Type Directory
$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"
$nuget = ".nuget\nuget.exe"
&$msbuild /target:Rebuild /property:Configuration=Release Dependable.sln

&$nuget pack -Symbols Dependable/Dependable.csproj -OutputDirectory .build
&$nuget pack -IncludeReferencedProjects Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj -OutputDirectory .build
&$nuget pack -Symbols Extensions/Dependable.Extensions.Persistence.Sql/Dependable.Extensions.Persistence.Sql.csproj -OutputDirectory .build

foreach($package in Get-ChildItem .build | Where-Object {$_.Name -match "^.*\.0\.nupkg"}){
	&$nuget push $package.FullName
}