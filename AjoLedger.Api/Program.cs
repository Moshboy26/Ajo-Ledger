using AjoLedger.Api.Middleware;
using AjoLedger.Api.Services;
using AjoLedger.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<
    ISavingsGroupRepository,
    DapperSavingsGroupRepository>();

builder.Services.AddScoped<SavingsGroupService>();

builder.Services.AddScoped<
    ISavingsGroupMemberRepository,
    DapperSavingsGroupMemberRepository>();

builder.Services.AddScoped<SavingsGroupMemberService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<
    IContributionRepository,
    DapperContributionRepository>();

builder.Services.AddScoped<ContributionService>();
builder.Services.AddScoped<
    ILedgerAccountRepository,
    DapperLedgerAccountRepository>();

builder.Services.AddScoped<
    ILedgerAccountService,
    LedgerAccountService>();

builder.Services.AddScoped<
    ILedgerTransactionRepository,
    DapperLedgerTransactionRepository>();

builder.Services.AddScoped<
    ILedgerTransactionService,
    LedgerTransactionService>();

 builder.Services.AddScoped<
    ILedgerReportRepository,
    DapperLedgerReportRepository>();    

builder.Services.AddScoped<
    ILedgerReportService,
    LedgerReportService>();

builder.Services.AddScoped<
    IContributionLedgerPostingService,
    ContributionLedgerPostingService>();

builder.Services.AddScoped<
    IContributionLedgerOutboxRepository,
    DapperContributionLedgerOutboxRepository>();

builder.Services.AddHostedService<ContributionLedgerOutboxWorker>();
        
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
