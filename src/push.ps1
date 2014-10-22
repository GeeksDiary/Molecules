foreach($package in Get-ChildItem .build | Where-Object {$_.Name -match "^.*\.0\.nupkg"}){
	&$nuget push $package.FullName
}