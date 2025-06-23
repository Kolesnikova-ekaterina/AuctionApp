using AuctionApp.Hubs;
using AuctionApp.Models;
using AuctionApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

// Заглушка класса PlaceBidResult (создайте в тестовом проекте, если отсутствует)
public class PlaceBidResult
{
    public bool Success { get; set; }
    public string Error { get; set; }
}

public class AuctionHubTests
{
    private readonly Mock<IAuctionService> _auctionServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IGroupManager> _groupsMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly AuctionHub _hub;

    public AuctionHubTests()
    {
        _auctionServiceMock = new Mock<IAuctionService>();
        _userManagerMock = MockUserManager<ApplicationUser>();
        _clientsMock = new Mock<IHubCallerClients>();
        _groupsMock = new Mock<IGroupManager>();
        _contextMock = new Mock<HubCallerContext>();

        _hub = new AuctionHub(_auctionServiceMock.Object, _userManagerMock.Object)
        {
            Clients = _clientsMock.Object,
            Groups = _groupsMock.Object,
            Context = _contextMock.Object
        };
    }

    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task JoinAuctionGroup_AddsUserToGroup_AndNotifiesOthers()
    {
        // Arrange
        var auctionId = 1;
        var connectionId = "conn1";
        var userName = "testuser";

        _contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userName)
        }));
        _contextMock.Setup(c => c.User).Returns(claimsPrincipal);

        var othersInGroupMock = new Mock<IClientProxy>();
        _clientsMock.Setup(c => c.OthersInGroup($"auction-{auctionId}")).Returns(othersInGroupMock.Object);
        _groupsMock.Setup(g => g.AddToGroupAsync(connectionId, $"auction-{auctionId}", default)).Returns(Task.CompletedTask);
        othersInGroupMock
        .Setup(c => c.SendCoreAsync(
            It.Is<string>(method => method == "Notify"),
            It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == $"{userName} присоединился к аукциону"),
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinAuctionGroup(auctionId);

        // Assert
        _groupsMock.Verify(g => g.AddToGroupAsync(connectionId, $"auction-{auctionId}", default), Times.Once);
        othersInGroupMock.Verify(c => c.SendCoreAsync(
        "Notify",
        It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == $"{userName} присоединился к аукциону"),
        It.IsAny<CancellationToken>()),
        Times.Once);
      }

    [Fact]
    public async Task SendAlert_SendsFormattedMessageToOthers()
    {
        // Arrange
        var message = "Hello alert";
        var userName = "alertuser";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userName)
        }));
        _contextMock.Setup(c => c.User).Returns(claimsPrincipal);

        var othersMock = new Mock<IClientProxy>();
        _clientsMock.Setup(c => c.Others).Returns(othersMock.Object);
        othersMock
         .Setup(c => c.SendCoreAsync(
             It.Is<string>(method => method == "ReceiveAlert"),
             It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == $"[{userName}]: {message}"),
             It.IsAny<CancellationToken>()))
         .Returns(Task.CompletedTask);
        // Act
        await _hub.SendAlert(message);

        // Assert
        othersMock.Verify(c => c.SendCoreAsync(
            "ReceiveAlert",
            It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == $"[{userName}]: {message}"),
            It.IsAny<CancellationToken>()),
            Times.Once);
        }

    [Fact]
    public async Task AddNewAuction_SendsAddNewAuctionReceiveToOthers()
    {
        // Arrange
        var othersMock = new Mock<IClientProxy>();
        _clientsMock.Setup(c => c.Others).Returns(othersMock.Object);
       othersMock
        .Setup(c => c.SendCoreAsync(
            It.Is<string>(method => method == "AddNewAuction_recieve"),
            It.Is<object[]>(args => args.Length == 0),
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
        // Act
        await _hub.AddNewAuction();

        // Assert
        othersMock.Verify(c => c.SendCoreAsync(
        "AddNewAuction_recieve",
        It.Is<object[]>(args => args.Length == 0),
        It.IsAny<CancellationToken>()),
        Times.Once);
    }

    [Fact]
    public async Task PlaceBid_Success_SendsBidPlacedAndAlerts()
    {
        // Arrange
        int auctionId = 1;
        decimal amount = 100m;

        var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        }));
        _contextMock.Setup(c => c.User).Returns(claimsPrincipal);

        _userManagerMock.Setup(um => um.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);

        var placeBidResult = (Success: true, Error: (string?)null);
        _auctionServiceMock
            .Setup(s => s.PlaceBidAsync(auctionId, user.Id, amount))
            .ReturnsAsync(placeBidResult);

        var auction = new Auction { OwnerId = "owner1", Title = "AuctionTitle" };
        _auctionServiceMock.Setup(s => s.GetAuctionAsync(auctionId)).ReturnsAsync(auction);

        var groupMock = new Mock<IClientProxy>();
        var othersMock = new Mock<IClientProxy>();
        var userMock = new Mock<ISingleClientProxy>();

        _clientsMock.Setup(c => c.Group($"auction-{auctionId}")).Returns(groupMock.Object);
        _clientsMock.Setup(c => c.Others).Returns(othersMock.Object);
        _clientsMock.Setup(c => c.User(auction.OwnerId)).Returns(userMock.Object);

        groupMock
        .Setup(c => c.SendCoreAsync(
            It.Is<string>(method => method == "BidPlaced"),
            It.Is<object[]>(args =>
                args.Length == 2 &&
                args[0].ToString() == $"Пользователь {user.UserName} сделал ставку: {amount}" &&
                args[1] != null && args[1].Equals(amount)),
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        othersMock
            .Setup(c => c.SendCoreAsync(
                It.Is<string>(method => method == "ReceiveAlert"),
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0].ToString() == $"Пользователь {user.UserName} сделал ставку на аукционе #{auctionId}"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        userMock
            .Setup(c => c.SendCoreAsync(
                It.Is<string>(method => method == "Notify"),
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0].ToString() == $"На ваш лот '{auction.Title}' сделана новая ставка: {amount} от {user.UserName}"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.PlaceBid(auctionId, amount);

        // Assert
        groupMock.Verify(c => c.SendCoreAsync(
        "BidPlaced",
        It.Is<object[]>(args =>
            args.Length == 2 &&
            args[0].ToString() == $"Пользователь {user.UserName} сделал ставку: {amount}" &&
            args[1] != null && args[1].Equals(amount)),
        It.IsAny<CancellationToken>()), Times.Once);

        othersMock.Verify(c => c.SendCoreAsync(
            "ReceiveAlert",
            It.Is<object[]>(args =>
                args.Length == 1 &&
                args[0].ToString() == $"Пользователь {user.UserName} сделал ставку на аукционе #{auctionId}"),
            It.IsAny<CancellationToken>()), Times.Once);

        userMock.Verify(c => c.SendCoreAsync(
            "Notify",
            It.Is<object[]>(args =>
                args.Length == 1 &&
                args[0].ToString() == $"На ваш лот '{auction.Title}' сделана новая ставка: {amount} от {user.UserName}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task PlaceBid_Failure_SendsErrorToCaller()
    {
        // Arrange
        int auctionId = 1;
        decimal amount = 100m;

        var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        }));
        _contextMock.Setup(c => c.User).Returns(claimsPrincipal);

        _userManagerMock.Setup(um => um.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);

        var placeBidResult = (Success: false, Error: "Ошибка ставки");
        _auctionServiceMock.Setup(s => s.PlaceBidAsync(auctionId, user.Id, amount)).ReturnsAsync(placeBidResult);

        var callerMock = new Mock<ISingleClientProxy>();
        callerMock
            .Setup(c => c.SendCoreAsync(
                It.Is<string>(method => method == "Error"),
                It.Is<object[]>(o => o.Length == 1 && (string)o[0] == placeBidResult.Error),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _clientsMock.Setup(c => c.Caller).Returns(callerMock.Object);

        // Act
        await _hub.PlaceBid(auctionId, amount);

        // Assert
        callerMock.Verify(c => c.SendCoreAsync(
            "Error",
            It.Is<object[]>(o => o.Length == 1 && (string)o[0] == placeBidResult.Error),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task NotifyOwner_SendsNotifyToAuctionOwner()
    {
        // Arrange
        int auctionId = 1;
        string message = "Test message";

        var auction = new Auction { OwnerId = "owner1" };
        _auctionServiceMock.Setup(s => s.GetAuctionAsync(auctionId)).ReturnsAsync(auction);

        var userMock = new Mock<ISingleClientProxy>();
        _clientsMock.Setup(c => c.User(auction.OwnerId)).Returns(userMock.Object);
        userMock.Setup(c => c.SendCoreAsync(
        It.Is<string>(method => method == "Notify"),
        It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == message),
        It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
        // Act
        await _hub.NotifyOwner(auctionId, message);

        // Assert
        userMock.Verify(c => c.SendCoreAsync(
        "Notify",
        It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == message),
        It.IsAny<CancellationToken>()),
        Times.Once);
    }

    [Fact]
    public async Task NotifyOwner_WhenAuctionNull_DoesNotSend()
    {
        // Arrange
        int auctionId = 1;
        string message = "Test message";

        _auctionServiceMock.Setup(s => s.GetAuctionAsync(auctionId)).ReturnsAsync((Auction)null);

        // Act
        await _hub.NotifyOwner(auctionId, message);

        // Assert
        _clientsMock.Verify(c => c.User(It.IsAny<string>()), Times.Never);
    }
}
