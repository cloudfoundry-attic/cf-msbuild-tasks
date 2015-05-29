using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks.Test
{
     [TestClass]
   
    public class ValidateTaskTest
    {
         private IDisposable context;
         [TestInitialize]
         public void Init_Validate()
         {
             context = ShimsContext.Create();
             CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

             CloudFoundry.Manifests.Fakes.ShimManifestDiskRepository.ReadManifestString = TestUtils.CustomReadManifest;

             CloudFoundry.Manifests.Fakes.ShimManifest.AllInstances.Applications = TestUtils.CustomManifestApplications;


             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractStacksEndpoint.AllInstances.ListAllStacks = TestUtils.CustomListAllStacks;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllStacksResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetStacks;

             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServicesEndpoint.AllInstances.ListAllServicesRequestOptions = TestUtils.CustomListAllServices;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServicesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServicesResponse;

             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServicesEndpoint.AllInstances.ListAllServicePlansForServiceNullableOfGuid = TestUtils.CustomListServicePlans;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServicePlansForServiceResponse>.AllInstances.ResourcesGet = TestUtils.CustomListServicePlansResponse;

             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceInstancesEndpoint.AllInstances.ListAllServiceInstances = TestUtils.CustomListAllServiceInstancesPlain;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceInstancesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceInstancesPlainResponse;

             CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllOrganizationsResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllOrganizationsResponse;

             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllOrganizationsRequestOptions = TestUtils.CustomListAllOrganizations;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesForOrganizationResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllSpacesForOrganizationResponse;

             CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllSpacesForOrganizationNullableOfGuidRequestOptions = TestUtils.CustomListAllSpacesForOrganization;

             CloudFoundry.CloudController.V2.Client.Fakes.ShimMetadata.AllInstances.GuidGet = TestUtils.CustomMetadataGuidGet;

         }

         [TestCleanup]
         public void Cleanup_Validate()
         {
             context.Dispose();
         }

         [TestMethod]
         public void Validate_Test()
         {
             Validate task = GetValidateTask();

             Assert.IsTrue(task.Execute());
         }

         private static Validate GetValidateTask()
         {
             Validate task = new Validate();
             task.BuildEngine = new FakeBuildEngine();

             task.CFUser = Settings.Default.User;
             task.CFPassword = Settings.Default.Password;
             task.CFServerUri = Settings.Default.ServerUri;
             task.CFSpace = "TestSpace";
             task.CFOrganization = "TestOrg";
             task.CFManifest = Settings.Default.CFManifest;
             return task;
         }
    }
}
