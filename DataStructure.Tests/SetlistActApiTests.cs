using System.Reflection;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Common.Contracts.Generated.Models;
using Common.Database.DataObjects;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Database.Tours.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Server.Api;
using Server.Api.Auth;
using Server.Api.Cache;
using Server.Api.Controllers;
using Service.Setlists;
using Xunit;

namespace DataStructure.Tests;

public class SetlistActApiTests
{
    [Theory]
    [InlineData("Act title")]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateRequestMapping_RoundTripsTitle(string? title)
    {
        var dto = new UpdateSetlistActRequestDto { Title = title };

        Assert.Equal(title, dto.ToBo().Title);
    }

    [Fact]
    public void UpdateAct_UsesSameAuthorizationAndCacheTagsAsUpdateSetlist()
    {
        var actMethod = typeof(SetlistsController).GetMethod(nameof(SetlistsController.UpdateSetlistAct))!;
        var setlistMethod = typeof(SetlistsController).GetMethod(nameof(SetlistsController.UpdateSetlistHeader))!;

        Assert.Equal(setlistMethod.GetCustomAttribute<AuthorizeRolesAttribute>()!.Roles,
            actMethod.GetCustomAttribute<AuthorizeRolesAttribute>()!.Roles);
        Assert.Equal(setlistMethod.GetCustomAttribute<ClearCacheAttribute>()!.Tags,
            actMethod.GetCustomAttribute<ClearCacheAttribute>()!.Tags);
        Assert.Equal("{setlistId:int}/acts/{actNumber:int}", actMethod.GetCustomAttribute<HttpPutAttribute>()!.Template);
    }

    [Fact]
    public async Task UpdateAct_UpdatesTitleAtMaximumLengthAndReturnsNoContent()
    {
        var acts = Substitute.For<ISetlistActRepository>();
        var act = new SetlistActDo { SetlistId = 1, ActNumber = 2, Title = "Old title" };
        acts.GetBy(1, 2).Returns(act);
        var service = new SetlistService(
            Substitute.For<ISetlistRepository>(), Substitute.For<ISetlistEntryRepository>(),
            Substitute.For<IConcertRepository>(), Substitute.For<ISongRepository>(),
            Substitute.For<ISongVariantRepository>(), Substitute.For<ISongMashupRepository>(),
            acts, Substitute.For<ISetlistPushEventSender>(), Substitute.For<ILambdaLogger>());
        var controller = new SetlistsController(service);
        var title = new string('a', DataConstants.TitleFieldLength);

        var result = await controller.UpdateSetlistAct(1, 2, new UpdateSetlistActRequestDto { Title = title });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(title, act.Title);
        acts.Received(1).Update(act);
        await acts.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAct_RejectsTitleExceedingDatabaseLimit()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        using var provider = services.BuildServiceProvider();
        var controller = new SetlistsController(null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };

        var result = await controller.UpdateSetlistAct(1, 2,
            new UpdateSetlistActRequestDto { Title = new string('a', DataConstants.TitleFieldLength + 1) });

        var problem = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);
    }
}



