
#**Summary**

The cf-dotnet-sdk-msbuild-tasks project implements custom tasks that use the cf-dotnet-sdk (https://github.com/hpcloud/cf-dotnet-sdk) to facilitate .NET projects integration with CloudFoundry.

Tasks implemented in this library:

 - Login 
	 - allows the user to obtain a refresh token from the cloud
	 - required parameters: cloud credentials (CFUser - CFPassword or CFRefreshToken, CFServerUri and CFSkipSslValidation)
	 - output: refresh token (CFRefreshToken)
 - CreateApp 
	 - allows the user to create a new application in the cloud
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 - output: created application's guid (CFAppGuid)
 - PushApp
	 - allows the user to push an application to the cloud
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 - optional parameters: allow application to start (CFStart - default value is false) 
 - CreateRoutes
	 - allow users to create routes that can be later mapped to an application
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 - output: returns a list of route guids
 - BindRoutes
	 - allow users to bind routes to an application
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
 - BindServices
	 - allows users to bind an array of provisioned services to an application
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 - output: returns an array of binding guids (BindingGuids)
 - DeleteApp
	 - allows user to remove an application from the cloud
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 - optional parameters: allow task to delete routes bound to the application (CFDeleteRoutes) and allow task to delete provisioned services bound to the application (CFDeleteServices)
 - DeleteService
	 - allows user to remove a provisioned service instance
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
 - DeleteRoute
	 - allows user to remove an unbound route
     - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
 - UnbindRoute
	 - allows user to remove a route binding from an application
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
 - UnbindService
	 - allows user to remove a provisioned service binding from an application
	 - required parameters: cloud credentials, space name (CFSpace), organization name (CFOrganization) and application's manifest (CFManifest)
	 
#**Prerequisites**
 - Microsoft Visual Studio 2013
 - .NET Framework 4.5

#**Usage**

In order to use this library it is necessary to map the task that is referenced to the assembly that contains the task implementation (in this case the cf-dotnet-sdk-msbuild-tasks library). This can be done by adding the UsingTask Element in your msbuild project file (https://msdn.microsoft.com/en-us/library/t41tzex2.aspx)

#**Project dependencies**

This library uses Cloud Foundry .NET SDK and YamlDotNet libraries.

#**Building the project**

The solution can be build simply by running `msbuild CloudFoundry.Build.sln`. The build output will be located in the lib folder and the tests are located in lib\tests.

#**Running tests**

Since the unit tests are using fakes the cf-dotnet-sdk-msbuild-tasks project tests can be run only using vstest.console.exe since mstest does not support the fakes framework yet. 

To run the unit tests:
`vstest.console.exe lib\tests\CloudFoundry.Build.Tasks.Test.dll`

To run the integration tests it is necessary to modify the CloudFoundry.Build.Tasks.IntegrationTests.dll.config file and add the credentials and url of the CloudFoundry deployment used for the tests.

To run the integration tests:
`vstest.console.exe lib\tests\CloudFoundry.Build.Tasks.IntegrationTests.dll`



#**Integration with a project**

It is not necessary for the project that will use this library to include a reference to this library. As stated in the usage section this library should be used with a UsingTask Element straight in the project file. 

Sample integration of the create application task in a target of the project file: 

`<Target Name="sampleCreate">
	<UsingTask AssemblyFile="$(SolutionDir)\packages\Cloudfoundry.Build.Tasks.dll" TaskName="CreateApp">
	</UsingTask>
	        <CreateApp CFAppName="sampleApp" CFStack="win2012r2" CFSpace="SampleSpace" CFUser="sampleUser" CFPassword="samplePass" CFServerUri="api.1.2.3.4.xip.io">
    			<Output TaskParameter="CFAppGuid" PropertyName="CFAppGuid"/>
    		</CreateApp>
    </Target>
`

This target can be called using: `msbuild myproject.csproj /t:sampleCreate`

