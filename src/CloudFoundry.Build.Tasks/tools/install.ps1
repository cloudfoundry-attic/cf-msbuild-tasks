param($installPath, $toolsPath, $package, $project)

function Get-Solution-Folder {
	if($dte.Solution -and $dte.Solution.IsOpen) {
		return Split-Path $dte.Solution.Properties.Item("Path").Value
	}
	else {
		throw "Solution not avaliable"
	}
}

function Copy-Resources($project) {
	$solutionDir = Get-Solution-Folder
	$cfTasksPath = (Join-Path $solutionDir ".cf")

	# Create the solution folder.
	if(!(Test-Path $cfTasksPath)) {
		mkdir $cfTasksPath | Out-Null
	}

	Write-Host "Copying CF DotNet MSBuild Task files to $cfTasksPath"
	Copy-Item "$installPath\lib\net45\*" $cfTasksPath -recurse -Force | Out-Null
	Copy-Item "$toolsPath\cf-dotnet-sdk-msbuild-tasks.targets" $cfTasksPath -Force | Out-Null
	Copy-Item "$toolsPath\cf-dotnet-sdk-msbuild-tasks.props" $cfTasksPath -Force | Out-Null

	$buildFile = Join-Path $solutionDir "cf.proj"
	

	if(!(Test-Path $buildFile)) {
		Write-Host "Copying Sample cf.proj to $solutionDir"
		Copy-Item "$toolsPath\cf.proj" $solutionDir | Out-Null
	}

	Write-Host "Don't forget to commit the .cf folder"
	return "$cfTasksPath"
}

function Add-Solution-Folder($buildPath) {
	# Get the open solution.
	$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

	# Create the solution folder.
	$buildFolder = $solution.Projects | Where {$_.ProjectName -eq ".cf"}
	if (!$buildFolder) {
		$buildFolder = $solution.AddSolutionFolder(".cf")
	}

	# Add files to solution folder
	$projectItems = Get-Interface $buildFolder.ProjectItems ([EnvDTE.ProjectItems])

	$targetsPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "cf-dotnet-sdk-msbuild-tasks.targets") )
	$projectItems.AddFromFile($targetsPath)

	$targetsPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "cf-dotnet-sdk-msbuild-tasks.props") )
	$projectItems.AddFromFile($targetsPath)

	$dllPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "CloudFoundry.Build.Tasks.dll") )
	$projectItems.AddFromFile($dllPath)

	$projPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "..\cf.proj") )
	$projectItems.AddFromFile($projPath)
}

function Main 
{
	$taskPath = Copy-Resources $project
	Add-Solution-Folder $taskPath
}

Main
