# Rapid Failures
for($i = 0; $i -lt 10; $i++){
	Write-Host "Rapid Failure $i"
	net stop mssqlserver	
	net start mssqlserver
}

# Failures that last for 1 - 10 seconds
for($i = 0; $i -lt 10; $i++){
	$delay = get-random -minimum 1 -maximum 10	
	Write-Host "Failing SQL server for $delay seconds"
	net stop mssqlserver
	sleep -s $delay
	net start mssqlserver
}

# Rapid Failures
for($i = 0; $i -lt 10; $i++){
	Write-Host "Rapid Failure $i"
	net stop mssqlserver	
	net start mssqlserver
}
