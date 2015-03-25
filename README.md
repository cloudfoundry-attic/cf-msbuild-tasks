
#**Summary**

The cf-dotnet-sdk-msbuild-tasks project implements custom tasks that use the cf-dotnet-sdk (https://github.com/hpcloud/cf-dotnet-sdk) to facilitate .NET projects integration with CloudFoundry.

Tasks implemented in this library:
		

 - LoadYaml
	 - allows loading the settings for a manifest yaml file (http://docs.cloudfoundry.org/devguide/deploy-apps/manifest.html )
	 - required parameter: path to the manifest file (ConfigurationFile)
	 - output: application name (AppName), path to application's folder (AppPath), an array of routes (Routes), application's stack (Stack), memory limit (Memory), instances number (Instances), autoscale information as xml (Autoscale) , disk limit (Disk), provisioned services information xml (Services), placement zone (PlacementZone) and single sign on enabled information (SsoEnabled)
 
 - CreateApp 
	 - allows the user to create a new application in the cloud
	 - required parameters: cloud credentials (User, Password and ServerUri), application name (AppName), space name (Space) and application's stack (Stack)
	 - optional parameters: memory limit (Memory), instances number (Instances) and buildpack name (Buildpack)
	 - output: created application's guid (AppGuid)
 - PushApp
	 - allows the user to push an application to the cloud
	 - required parameters: cloud credentials, application's guid (AppGuid) and the path to the application's folder (AppPath)
	 - optional parameters: allow application to start (Start - default value is false) 
 - CreateRoutes
	 - allow users to create routes that can be later mapped to an application
	 - required parameters: cloud credentials, an array of routes (Routes) and the space name (Space)
	 - output: returns a list of route guids
 - BindRoutes
	 - allow users to bind routes to an application
	 - required parameters: cloud credentials, application's guid(AppGuid) and an array of route guids (RouteGuids)
 - CreateService
	 - allow users to provision a service
	 - required parameters: cloud credentials, service name (Name), space name (Space), service plan (ServicePlan) and service type (ServiceType)
	 - output: returns the provisioned service's guid (ServiceGuid)
 - CreateServices
	 - allow users to provision a collection of services
	 - required parameters: cloud credentials, services information provided as xml (Services) and space name (Space)
	 - output: returns an array of the provisioned services guids (ServicesGuids)
 - BindServices
	 - allows users to bind an array of provisioned services to an application
	 - required parameters: cloud credentials, application's guid (AppGuid) and an array of service guids (ServicesGuids)
	 - output: returns an array of binding guids (BindingGuids)
 - DeleteApp
	 - allows user to remove an application from the cloud
	 - required parameters: cloud credentials, application name (AppName) and space name (Space)
	 - optional parameters: allow task to delete routes bound to the application (DeleteRoutes) and allow task to delete provisioned services bound to the application (DeleteServices)
 - DeleteService
	 - allows user to remove a provisioned service instance
	 - required parameters: cloud credentials, provisioned service name (ServiceName) and space name (Space)
 - DeleteRoute
	 - allows user to remove an unbound route
	 - required parameters: cloud credentials and route (Route)
 - UnbindRoute
	 - allows user to remove a route binding from an application
	 - required parameters: cloud credentials, application name (AppName), space name (Space) and route (Route) 
 - UnbindService
	 - allows user to remove a provisioned service binding from an application
	 - required parameters: cloud credentials, application name (AppName), space name (Space) and service name (ServiceName)
 - SaveYaml
	 - allows user to create a manifest yaml file
	 - required parameters: application path (AppPath), application name (AppName), application's route (Route), instances number (Instances), memory limit (Memory), application's stack (Stack) and configuration file path (ConfigurationFile)
	 - optional parameters: autoscale information (MaxCpu, MinCpu, Enabled, MaxInstances and MinInstances), disk limit (Disk), placement zone (PlacementZone), single sign on enabled (SsoEnabled) and service information (ServiceName, ServicePlan, ServiceType)
 
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
	<CreateApp Name="sampleApp" Stack="win2012r2" Space="SampleSpace" User="sampleUser" Password="samplePass" ServerUri="api.1.2.3.4.xip.io">
    			<Output TaskParameter="AppGuid" PropertyName="AppGuid"/>
    		</CreateApp>
    </Target>
`

This target can be called using: `msbuild myproject.csproj /t:sampleCreate`

Later AppGuid can be used as a parameter for binding tasks. 

