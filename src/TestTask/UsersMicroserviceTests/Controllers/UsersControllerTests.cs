using Grpc.Core;
using JwtLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users;
using UsersMicroservice.Controllers;
using UsersMicroservice.Services;

namespace UsersMicroserviceTests.Controllers
{
	[TestClass()]
	public class UsersControllerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			UsersServiceClientMock = new Mock<UsersService.UsersServiceClient>(MockBehavior.Strict);
			JwtServiceMock = new Mock<IJwtService>(MockBehavior.Strict);
			PasswordServiceMock = new Mock<IPasswordService>(MockBehavior.Strict);
		}

		private Mock<UsersService.UsersServiceClient> UsersServiceClientMock;
		private Mock<IJwtService> JwtServiceMock;
		private Mock<IPasswordService> PasswordServiceMock;

		private const string Login = "some_login";
		private const string Password = "pwd";
		private const string PasswordHash = "pwd_hash";
		private const string PasswordSalt = "pwd_salt";
		private readonly List<string> Codes = ["USD", "EUR"];

		private const string CookieName = "some_cookie_name";

		#region Register

		[TestMethod]
		public async Task RegisterLoginAlreadyPresentTest()
		{
			var isUserExistResponse = new IsUserExistsResponse { Exists = true };
			UsersServiceClientMock.Setup(client => client.IsUserExistsAsync(
					It.Is<IsUserExistsRequest>(request => request.Login == Login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<IsUserExistsResponse>(Task.FromResult(isUserExistResponse), null!, null!, null!, null!));

			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object);

			var result = await controller.Register(Login, Password, Codes);
			Assert.IsInstanceOfType<ConflictObjectResult>(result);
			var conflictResult = (ConflictObjectResult)result;
			Assert.AreEqual(conflictResult.StatusCode, 409);
		}

		[TestMethod]
		public async Task RegisterUnsuccessfulTest()
		{
			var isUserExistResponse = new IsUserExistsResponse { Exists = false };
			UsersServiceClientMock.Setup(client => client.IsUserExistsAsync(
					It.Is<IsUserExistsRequest>(request => request.Login == Login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<IsUserExistsResponse>(Task.FromResult(isUserExistResponse), null!, null!, null!, null!));
			var addUserResponse = new AddUserResponse { Success = false };
			UsersServiceClientMock.Setup(client => client.AddUserAsync(
					It.Is<AddUserRequest>(request => request.Login == Login && request.PasswordHash == PasswordHash && request.PasswordSalt == PasswordSalt && request.Codes.SequenceEqual(Codes)),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<AddUserResponse>(Task.FromResult(addUserResponse), null!, null!, null!, null!));
			var passwordHash = PasswordHash;
			var passwordSalt = PasswordSalt;
			PasswordServiceMock.Setup(service => service.GenerateCredentials(Password, out passwordHash, out passwordSalt));

			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object);

			await Assert.ThrowsExceptionAsync<Exception>(async () => await controller.Register(Login, Password, Codes));
		}

		[TestMethod]
		public async Task RegisterSuccessfulTest()
		{
			var isUserExistResponse = new IsUserExistsResponse { Exists = false };
			UsersServiceClientMock.Setup(client => client.IsUserExistsAsync(
					It.Is<IsUserExistsRequest>(request => request.Login == Login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<IsUserExistsResponse>(Task.FromResult(isUserExistResponse), null!, null!, null!, null!));
			var addUserResponse = new AddUserResponse { Success = true };
			UsersServiceClientMock.Setup(client => client.AddUserAsync(
					It.Is<AddUserRequest>(request => request.Login == Login && request.PasswordHash == PasswordHash && request.PasswordSalt == PasswordSalt && request.Codes.SequenceEqual(Codes)),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<AddUserResponse>(Task.FromResult(addUserResponse), null!, null!, null!, null!));
			var passwordHash = PasswordHash;
			var passwordSalt = PasswordSalt;
			PasswordServiceMock.Setup(service => service.GenerateCredentials(Password, out passwordHash, out passwordSalt));

			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object);

			var result = await controller.Register(Login, Password, Codes);
			Assert.IsInstanceOfType<OkResult>(result);
		}

		#endregion

		#region Login

		[TestMethod]
		public async Task LoginWithWrongCredentialsTest()
		{
			var getUserCredentialsResponse = new GetUserCredentialsResponse { PasswordHash = PasswordHash, PasswordSalt = PasswordSalt };
			UsersServiceClientMock.Setup(client => client.GetUserCredentialsAsync(
				It.Is<GetUserCredentialsRequest>(request => request.Login == Login),
				It.IsAny<Metadata>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<GetUserCredentialsResponse>(Task.FromResult(getUserCredentialsResponse), null!, null!, null!, null!));
			PasswordServiceMock.Setup(service => service.ValidateCredentials(
					It.Is<string>(s => s == Password),
					It.Is<string>(s => s == PasswordHash),
					It.Is<string>(s => s == PasswordSalt)))
				.Returns(false);

			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object);

			var result = await controller.Login(Login, Password);
			Assert.IsInstanceOfType<UnauthorizedResult>(result);
		}

		[TestMethod]
		public async Task LoginOkTest()
		{
			var lifetime = TimeSpan.FromSeconds(30);
			var token = "some_very_large_token";
			var getUserCredentialsResponse = new GetUserCredentialsResponse { PasswordHash = PasswordHash, PasswordSalt = PasswordSalt };
			UsersServiceClientMock.Setup(client => client.GetUserCredentialsAsync(
					It.Is<GetUserCredentialsRequest>(request => request.Login == Login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<GetUserCredentialsResponse>(Task.FromResult(getUserCredentialsResponse), null!, null!, null!, null!));
			JwtServiceMock.Setup(service => service.GetCookieName()).Returns(CookieName);
			JwtServiceMock.Setup(service => service.GetLifetime()).Returns(lifetime);
			JwtServiceMock.Setup(service => service.GenerateToken(It.Is<string>(s => s == Login))).Returns(token);
			PasswordServiceMock.Setup(service => service.ValidateCredentials(
					It.Is<string>(s => s == Password),
					It.Is<string>(s => s == PasswordHash),
					It.Is<string>(s => s == PasswordSalt)))
				.Returns(true);

			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object)
			{
				ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
			};

			var result = await controller.Login(Login, Password);
			
			Assert.IsInstanceOfType<OkResult>(result);

			var cookie = controller.Response.Headers.SetCookie;
			var components = cookie.ToString().Split(";", StringSplitOptions.TrimEntries);
			Assert.IsTrue(components.Contains(CookieName + "=" + token));

			var expiresComponent = components.First(s => s.StartsWith("expires"));
			var expires = DateTime.Parse(expiresComponent.Split('=')[1]);
			Assert.IsTrue(expires > DateTime.Now && expires < DateTime.Now + TimeSpan.FromMinutes(1));
		}

		#endregion

		[TestMethod]
		public void LogoutTest()
		{
			JwtServiceMock.Setup(service => service.GetCookieName()).Returns(CookieName);
			var controller = new UsersController(UsersServiceClientMock.Object, JwtServiceMock.Object, PasswordServiceMock.Object)
			{
				ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
			};

			var result = controller.Logout();

			Assert.IsInstanceOfType<OkResult>(result);

			var cookie = controller.Response.Headers.SetCookie;
			var components = cookie.ToString().Split(";", StringSplitOptions.TrimEntries);
			Assert.IsTrue(components.Contains(CookieName + "="));
		}
	}
}