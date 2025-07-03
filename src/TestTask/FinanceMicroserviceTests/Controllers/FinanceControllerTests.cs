using System.Security.Claims;
using FinanceMicroservice.Controllers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users;

namespace FinanceMicroserviceTests.Controllers
{
	[TestClass()]
	public class FinanceControllerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			UsersServiceClientMock = new Mock<UsersService.UsersServiceClient>(MockBehavior.Strict);
		}

		private Mock<UsersService.UsersServiceClient> UsersServiceClientMock;

		[TestMethod()]
		public async Task GetRatesTest()
		{
			const string login = "some_login";
			var expectedResponse = new GetRatesResponse();
			expectedResponse.Rates.AddRange([new CurrencyRate{ Code = "USD", Name ="Доллар", Rate = "100"}, new CurrencyRate { Code = "EUR", Name = "Евро", Rate = "150" }]);
			UsersServiceClientMock.Setup(client => client.GetRatesAsync(
					It.Is<GetRatesRequest>(request => request.Login == login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<GetRatesResponse>(Task.FromResult(expectedResponse), null!, null!, null!, null!));
			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, login)]));
			var controller = new FinanceController(UsersServiceClientMock.Object)
			{
				ControllerContext = new ControllerContext
				{
					HttpContext = new DefaultHttpContext
					{
						User = claimsPrincipal
					}
				}
			};

			var result = await controller.GetRates();

			Assert.IsInstanceOfType<OkObjectResult>(result);
			var okResult = (OkObjectResult)result;
			Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);

			var data = (List<FinanceController.UserCurrencyRate>)okResult.Value!;

			Assert.AreEqual(data[0].Code, "USD");
			Assert.AreEqual(data[0].Name, "Доллар");
			Assert.AreEqual(data[0].Rate, 100m);

			Assert.AreEqual(data[1].Code, "EUR");
			Assert.AreEqual(data[1].Name, "Евро");
			Assert.AreEqual(data[1].Rate, 150m);
		}
	}
}