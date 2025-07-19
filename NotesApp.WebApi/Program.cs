using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotesApp.Infrastructure.Data;
using NotesApp.Infrastructure.Repositories;
using NotesApp.Application.Commands;
using NotesApp.Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApp.Application.Validators;
using NotesApp.Application.Commands.UpdateNote;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notes API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateNoteCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<BulkCreateNoteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateNoteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTagCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteNoteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteTagCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetAllNotesQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetAllTagsQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetNoteByIdQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetTagByIdQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetUserByIdQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RefreshTokenCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateNoteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTagCommandValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
#warning to do extentions

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<NotesDbContext>();

    /* if (!context.Database.CanConnect())
    {
        logger.LogInformation("Database does not exist. Applying migrations to create it.");
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("Database exists. Skipping migration application.");
    } */
    try
    {
        var appliedMigrations = context.Database.GetAppliedMigrations();
        var allMigrations = context.Database.GetMigrations();

        if (allMigrations.Except(appliedMigrations).Any())
        {
            logger.LogInformation("Pending migrations found. Applying migrations.");
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("Database exists and all migrations are up to date.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();