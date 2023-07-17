namespace Play.Catalog.Contracts
{
    public record CatalogItemCreated(Guid ItemId, string Name, string? Description);

    public record CatalogItemUpdated(Guid ItemId, string Name, string? Description);

    public record CatalogItemDelted(Guid ItemId);
}
