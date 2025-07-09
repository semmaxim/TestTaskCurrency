using System.Security.Claims;
using FinanceMicroservice.Controllers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Currency;

namespace FinanceMicroserviceTests.Controllers
{
	[TestClass()]
	public class FinanceControllerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			CurrencyServiceClientMock = new Mock<CurrencyService.CurrencyServiceClient>(MockBehavior.Strict);
		}

		private Mock<CurrencyService.CurrencyServiceClient> CurrencyServiceClientMock;

		[TestMethod()]
		public async Task GetRatesTest()
		{
			const string login = "some_login";
			var expectedResponse = new GetCurrencyRatesForUserResponse();
			expectedResponse.Rates.AddRange([new CurrencyRate{ Code = "USD", Name ="Доллар", Rate = "100"}, new CurrencyRate { Code = "EUR", Name = "Евро", Rate = "150" }]);
			CurrencyServiceClientMock.Setup(client => client.GetCurrencyRatesForUserAsync(
					It.Is<GetCurrencyRatesForUserRequest>(request => request.Login == login),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(new AsyncUnaryCall<GetCurrencyRatesForUserResponse>(Task.FromResult(expectedResponse), null!, null!, null!, null!));
			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, login)]));
			var controller = new FinanceController(CurrencyServiceClientMock.Object)
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