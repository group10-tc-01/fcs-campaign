using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fcs.Campaign.Application.Audit;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.FunctionalTests.Support;
using FluentAssertions;
using Reqnroll;
using CampaignEntity = Fcs.Campaign.Domain.Campaigns.Campaign;

namespace Fcs.Campaign.FunctionalTests.StepDefinitions;

[Binding]
public sealed class CampaignStepDefinitions : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private FunctionalWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private HttpResponseMessage? _response;
    private HttpResponseMessage? _secondResponse;
    private CampaignEntity? _campaign;
    private Guid _unknownCampaignId;
    private Guid _donationId;
    private JsonDocument? _document;

    [BeforeScenario]
    public void BeforeScenario()
    {
        _factory = new FunctionalWebApplicationFactory();
        _client = _factory.CreateClient();
        _factory.Repository.Reset();
        _factory.Publisher.Reset();
        _factory.Authentication.IsAuthenticated = true;
        _factory.Authentication.Role = "GestorONG";
        _factory.Authentication.UserId = Guid.NewGuid();
    }

    [AfterScenario]
    public void AfterScenario()
    {
        Dispose();
    }

    [Given(@"que estou autenticado como ""(.*)""")]
    public void DadoQueEstouAutenticadoComo(string role)
    {
        _factory.Authentication.IsAuthenticated = true;
        _factory.Authentication.Role = role;
    }

    [Given("que não estou autenticado")]
    public void DadoQueNaoEstouAutenticado()
    {
        _factory.Authentication.IsAuthenticated = false;
    }

    [Given(@"que existe uma campanha com status ""(.*)""")]
    [Given(@"existe uma campanha com status ""(.*)""")]
    public async Task DadoQueExisteUmaCampanhaComStatus(string status)
    {
        _campaign = new CampaignBuilder().Build();
        ApplyStatus(_campaign, ParseStatus(status));
        await _factory.Repository.AddAsync(_campaign);
    }

    [Given(@"existem (.*) campanhas com status ""(.*)""")]
    public async Task DadoQueExistemCampanhasComStatus(int quantity, string status)
    {
        for (var index = 0; index < quantity; index++)
        {
            var campaign = new CampaignBuilder()
                .WithFinancialGoal(1000 + index)
                .Build();
            ApplyStatus(campaign, ParseStatus(status));
            await _factory.Repository.AddAsync(campaign);
        }
    }

    [When("eu criar uma campanha válida")]
    public async Task QuandoEuCriarUmaCampanhaValida()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/campaigns", ValidCampaignRequest());
        await ReadResponseAsync();
    }

    [When(@"eu criar uma campanha com ""(.*)"" inválido")]
    public async Task QuandoEuCriarUmaCampanhaComCampoInvalido(string field)
    {
        var request = ValidCampaignRequest();
        request = field switch
        {
            "titulo" => request with { Title = "" },
            "descricao" => request with { Description = "" },
            "data final" => request with { EndDate = DateTime.UtcNow.Date.AddDays(-1) },
            "intervalo" => request with
            {
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(9)
            },
            "meta" => request with { FinancialGoal = 0 },
            _ => throw new InvalidOperationException($"Unknown field {field}.")
        };

        _response = await _client.PostAsJsonAsync("/api/v1/campaigns", request);
        await ReadResponseAsync();
    }

    [When("eu listar as campanhas administrativas")]
    public async Task QuandoEuListarAsCampanhasAdministrativas()
    {
        await SendGetAsync("/api/v1/campaigns");
    }

    [When(@"eu listar a página (.*) com tamanho (.*)")]
    public async Task QuandoEuListarAPaginaComTamanho(int page, int pageSize)
    {
        await SendGetAsync($"/api/v1/campaigns?page={page}&pageSize={pageSize}");
    }

    [When(@"eu listar filtrando pelos status ""(.*)""")]
    public async Task QuandoEuListarFiltrandoPelosStatus(string statuses)
    {
        var query = string.Join("&", statuses.Split(',').Select(status => $"status={status}"));
        await SendGetAsync($"/api/v1/campaigns?{query}");
    }

    [When("eu editar a campanha")]
    public async Task QuandoEuEditarACampanha()
    {
        _response = await _client.PutAsJsonAsync(
            $"/api/v1/campaigns/{_campaign!.Id}",
            new
            {
                Title = "Updated campaign",
                Description = "Updated description",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(60),
                FinancialGoal = 2500m
            });
        await ReadResponseAsync();
    }

    [When("eu editar uma campanha inexistente")]
    public async Task QuandoEuEditarUmaCampanhaInexistente()
    {
        _unknownCampaignId = Guid.NewGuid();
        _response = await _client.PutAsJsonAsync(
            $"/api/v1/campaigns/{_unknownCampaignId}",
            new
            {
                Title = "Unknown campaign",
                Description = "Unknown description",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                FinancialGoal = 100m
            });
        await ReadResponseAsync();
    }

    [When("eu consultar a campanha por identificador")]
    public async Task QuandoEuConsultarACampanhaPorIdentificador()
    {
        await SendGetAsync($"/api/v1/campaigns/{_campaign!.Id}");
    }

    [When("eu consultar uma campanha inexistente")]
    public async Task QuandoEuConsultarUmaCampanhaInexistente()
    {
        _unknownCampaignId = Guid.NewGuid();
        await SendGetAsync($"/api/v1/campaigns/{_unknownCampaignId}");
    }

    [When(@"eu alterar o status da campanha para ""(.*)""")]
    public async Task QuandoEuAlterarOStatusDaCampanhaPara(string status)
    {
        _response = await _client.PatchAsJsonAsync(
            $"/api/v1/campaigns/{_campaign!.Id}/status",
            new { Status = status });
        await ReadResponseAsync();
    }

    [When("eu consultar a transparência")]
    public async Task QuandoEuConsultarATransparencia()
    {
        await SendGetAsync("/api/v1/transparency/campaigns");
    }

    [When("eu consultar a elegibilidade da campanha")]
    public async Task QuandoEuConsultarAElegibilidadeDaCampanha()
    {
        await SendGetAsync($"/internal/campaigns/{_campaign!.Id}/donation-eligibility");
    }

    [When("eu consultar a elegibilidade de uma campanha inexistente")]
    public async Task QuandoEuConsultarAElegibilidadeDeUmaCampanhaInexistente()
    {
        await SendGetAsync($"/internal/campaigns/{Guid.NewGuid()}/donation-eligibility");
    }

    [When(@"eu refletir uma doação de (.*)")]
    public async Task QuandoEuRefletirUmaDoacaoDe(decimal amount)
    {
        _donationId = Guid.NewGuid();
        _response = await ProcessDonationAsync(amount);
        await ReadResponseAsync();
    }

    [When(@"eu refletir a mesma doação de (.*) duas vezes")]
    public async Task QuandoEuRefletirAMesmaDoacaoDuasVezes(decimal amount)
    {
        _donationId = Guid.NewGuid();
        _response = await ProcessDonationAsync(amount);
        _secondResponse = await ProcessDonationAsync(amount);
        await ReadResponseAsync(_secondResponse);
    }

    [Then(@"a resposta deve ter status (.*)")]
    public void EntaoARespostaDeveTerStatus(int statusCode)
    {
        ((int)_response!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"ambas as respostas devem ter status (.*)")]
    public void EntaoAmbasAsRespostasDevemTerStatus(int statusCode)
    {
        ((int)_response!.StatusCode).Should().Be(statusCode);
        ((int)_secondResponse!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"a campanha retornada deve estar com status ""(.*)""")]
    [Then(@"o status retornado deve ser ""(.*)""")]
    public void EntaoOStatusRetornadoDeveSer(string status)
    {
        GetData().GetProperty("status").GetString().Should().Be(status);
    }

    [Then("a campanha deve registrar o gestor autenticado")]
    public void EntaoACampanhaDeveRegistrarOGestorAutenticado()
    {
        GetData().GetProperty("createdByManagerId").GetGuid().Should().Be(_factory.Authentication.UserId);
    }

    [Then("a campanha deve conter os dados atualizados")]
    public void EntaoACampanhaDeveConterOsDadosAtualizados()
    {
        GetData().GetProperty("title").GetString().Should().Be("Updated campaign");
        GetData().GetProperty("financialGoal").GetDecimal().Should().Be(2500m);
    }

    [Then("a campanha consultada deve ser a campanha existente")]
    public void EntaoACampanhaConsultadaDeveSerACampanhaExistente()
    {
        GetData().GetProperty("id").GetGuid().Should().Be(_campaign!.Id);
    }

    [Then(@"a página retornada deve ser (.*) com tamanho (.*) e total (.*)")]
    public void EntaoAPaginaRetornadaDeveSerComTamanhoETotal(int page, int pageSize, int total)
    {
        var data = GetData();
        data.GetProperty("page").GetInt32().Should().Be(page);
        data.GetProperty("pageSize").GetInt32().Should().Be(pageSize);
        data.GetProperty("totalCount").GetInt32().Should().Be(total);
    }

    [Then(@"a listagem deve conter (.*) item")]
    [Then(@"a listagem deve conter (.*) itens")]
    public void EntaoAListagemDeveConterItens(int count)
    {
        GetData().GetProperty("items").GetArrayLength().Should().Be(count);
    }

    [Then(@"a listagem deve conter somente os status ""(.*)""")]
    public void EntaoAListagemDeveConterSomenteOsStatus(string statuses)
    {
        var expected = statuses.Split(',').ToHashSet(StringComparer.Ordinal);
        GetData().GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("status").GetString())
            .All(status => status is not null && expected.Contains(status))
            .Should().BeTrue();
    }

    [Then("a listagem de transparência deve conter somente campanhas ativas")]
    public void EntaoAListagemDeTransparenciaDeveConterSomenteCampanhasAtivas()
    {
        GetData().GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Then("a campanha deve estar elegível")]
    public void EntaoACampanhaDeveEstarElegivel()
    {
        GetData().GetProperty("eligible").GetBoolean().Should().BeTrue();
    }

    [Then("a campanha não deve estar elegível")]
    public void EntaoACampanhaNaoDeveEstarElegivel()
    {
        GetData().GetProperty("eligible").GetBoolean().Should().BeFalse();
    }

    [Then(@"o total arrecadado deve ser (.*)")]
    public void EntaoOTotalArrecadadoDeveSer(decimal total)
    {
        _campaign!.TotalAmountRaised.Should().Be(total);
    }

    [Then("a segunda resposta deve indicar duplicidade")]
    public void EntaoASegundaRespostaDeveIndicarDuplicidade()
    {
        GetData().GetProperty("duplicate").GetBoolean().Should().BeTrue();
    }

    [Then(@"deve ser publicada a auditoria ""(.*)""")]
    public async Task EntaoDeveSerPublicadaAAuditoria(string action)
    {
        var audit = await _factory.Publisher.WaitForMessageAsync<AuditLogRequestedEvent>(
            message => message.Action == action,
            TimeSpan.FromSeconds(2));
        audit.Should().NotBeNull();
    }

    [Then(@"a resposta deve informar que os status válidos são ""(.*)""")]
    public void EntaoARespostaDeveInformarOsStatusValidos(string statuses)
    {
        var content = _document!.RootElement.GetRawText();
        foreach (var status in statuses.Split(',', StringSplitOptions.TrimEntries))
        {
            content.Should().Contain(status);
        }
    }

    public void Dispose()
    {
        _document?.Dispose();
        _response?.Dispose();
        _secondResponse?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }

    private async Task SendGetAsync(string uri)
    {
        _response = await _client.GetAsync(uri);
        await ReadResponseAsync();
    }

    private async Task<HttpResponseMessage> ProcessDonationAsync(decimal amount)
    {
        return await _client.PostAsJsonAsync(
            $"/internal/campaigns/{_campaign!.Id}/donation-processed",
            new { DonationId = _donationId, Amount = amount, ProcessedAt = DateTime.UtcNow });
    }

    private async Task ReadResponseAsync(HttpResponseMessage? response = null)
    {
        _document?.Dispose();
        var content = await (response ?? _response!).Content.ReadAsStringAsync();
        _document = string.IsNullOrWhiteSpace(content) ? null : JsonDocument.Parse(content);
    }

    private JsonElement GetData()
    {
        return _document!.RootElement.GetProperty("data");
    }

    private static CampaignStatus ParseStatus(string status) =>
        Enum.Parse<CampaignStatus>(status, ignoreCase: true);

    private static void ApplyStatus(CampaignEntity campaign, CampaignStatus status)
    {
        if (status == CampaignStatus.Completed)
        {
            campaign.Complete();
        }
        else if (status == CampaignStatus.Canceled)
        {
            campaign.Cancel();
        }
    }

    private static CampaignRequest ValidCampaignRequest() =>
        new(
            "Winter campaign",
            "Collect resources for winter.",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30),
            1000m);

    private sealed record CampaignRequest(
        string Title,
        string Description,
        DateTime StartDate,
        DateTime EndDate,
        decimal FinancialGoal);
}
