
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TechMove.Controllers;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Tests;

public class ContractWorkflowServiceTests
{
    private readonly ContractWorkflowService _service = new();

    [Theory]
    [InlineData(ContractStatuses.Draft, true)]
    [InlineData(ContractStatuses.Active, true)]
    [InlineData(ContractStatuses.Expired, false)]
    [InlineData(ContractStatuses.OnHold, false)]
    public void CanCreateServiceRequest_FollowsStatusRule(
        string status, bool expected)
    {
        var contract = new Contract { Status = status };

        Assert.Equal(
            expected,
            _service.CanCreateServiceRequest(contract));
    }

    [Fact]
    public void CanCreateServiceRequest_RejectsPastEndDate()
    {
        var contract = new Contract
        {
            Status = ContractStatuses.Active,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(-1)
        };

        Assert.False(_service.CanCreateServiceRequest(contract));
    }

    [Fact]
    public void EnsureCanCreateServiceRequest_ThrowsForExpired()
    {
        var contract = new Contract
        {
            Status = ContractStatuses.Expired
        };

        Assert.Throws<InvalidOperationException>(
            () => _service.EnsureCanCreateServiceRequest(contract));
    }

    [Fact]
    public void ValidateContractDates_RejectsEndBeforeStart()
    {
        var contract = new Contract
        {
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 1)
        };

        Assert.Throws<InvalidOperationException>(
            () => _service.ValidateContractDates(contract));
    }

    [Fact]
    public async Task Create_UsesActiveStatusByDefault()
    {
        var httpClient = new HttpClient(new FakeApiHandler())
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var factory = new FakeHttpClientFactory(httpClient);

        var controller = new ContractsController(
            factory,
            new FileValidationService(),
            new ContractWorkflowService(),
            new FakeWebHostEnvironment());

        var result = await controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Contract>(viewResult.Model);

        Assert.Equal(ContractStatuses.Active, model.Status);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    private sealed class FakeApiHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            };

            return Task.FromResult(response);
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;

        public string ContentRootPath { get; set; } =
            Directory.GetCurrentDirectory();

        public IFileProvider WebRootFileProvider { get; set; } =
            new NullFileProvider();

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();

        public string ApplicationName { get; set; } = "TechMove.Tests";

        public IFileProvider FileProvider { get; set; } =
            new NullFileProvider();

        public string EnvironmentName { get; set; } = "Development";
    }
}
