namespace CqrsWithMediatR.Services
{
    public interface IKeyVaultService
    {
        string GetKeyValue(string keyValue);
    }
}
