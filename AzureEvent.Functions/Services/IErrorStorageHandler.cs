using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureEvent.Function.Services
{
    public interface IErrorStorageHandler
    {
        Task SendErrorToStorage(HttpRequest req, string domainName, string requestBody);
    }
}