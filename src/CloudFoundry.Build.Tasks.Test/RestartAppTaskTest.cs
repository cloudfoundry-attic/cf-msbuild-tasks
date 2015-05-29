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
    public class RestartAppTaskTest
    {
        [TestMethod]
        public void RestartApp_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.Manifests.Fakes.ShimManifestDiskRepository.ReadManifestString = TestUtils.CustomReadManifest;

                CloudFoundry.Manifests.Fakes.ShimManifest.AllInstances.Applications = TestUtils.CustomManifestApplications;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.UpdateAppNullableOfGuidUpdateAppRequest = TestUtils.CustomUpdateApp;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.GetAppSummaryNullableOfGuid = TestUtils.CustomGetAppSummary;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimInfoEndpoint.AllInstances.GetV1Info = TestUtils.CustomGetV1Info;

                CloudFoundry.Logyard.Client.Fakes.ShimLogyardLog.AllInstances.StartLogStreamString = TestUtils.CustomStartLogStreamString;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllOrganizationsResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllOrganizationsResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllOrganizationsRequestOptions = TestUtils.CustomListAllOrganizations;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesForOrganizationResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllSpacesForOrganizationResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllSpacesForOrganizationNullableOfGuidRequestOptions = TestUtils.CustomListAllSpacesForOrganization;

                TestUtils.InitTestMetadata();

                RestartApp task = new RestartApp();
                task.CFSpace = "TestSpace";
                task.CFOrganization = "TestOrg";
                task.CFManifest = Settings.Default.CFManifest;
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                Assert.IsTrue(task.Execute());
            }
        }
    }
}
