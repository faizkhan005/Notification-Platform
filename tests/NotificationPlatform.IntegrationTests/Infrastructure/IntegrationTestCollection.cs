namespace NotificationPlatform.IntegrationTests.Infrastructure;

[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgresContainerFixture>;

