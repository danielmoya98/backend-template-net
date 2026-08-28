using Xunit;

namespace BackendTemplate.IntegrationTests.Common;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
