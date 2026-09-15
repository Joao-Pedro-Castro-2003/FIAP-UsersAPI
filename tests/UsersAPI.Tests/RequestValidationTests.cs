using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UsersAPI.Controllers;
namespace UsersAPI.Tests;
public class RequestValidationTests
{
    [Fact]
    public void MvcValidatesRequestRecordsWithoutMetadataExceptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IObjectModelValidator>();
        (object request, bool valid)[] cases = [
            (new CreateUserRequest("Player", "player@example.com", "Password123"), true),
            (new CreateUserRequest("P", "invalid", "123"), false),
            (new LoginRequest("player@example.com", "Password123"), true),
            (new LoginRequest("invalid", ""), false)
        ];
        foreach (var (request, valid) in cases) {
            var context = new ActionContext(new DefaultHttpContext { RequestServices = provider }, new RouteData(), new ActionDescriptor());
            validator.Validate(context, null, "", request);
            Assert.Equal(valid, context.ModelState.IsValid);
        }
    }
}
