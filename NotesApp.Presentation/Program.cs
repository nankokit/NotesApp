using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotesApp.Application.Commands;
using NotesApp.Application.Queries;
using NotesApp.Domain.Interfaces;
using NotesApp.Infrastructure.Data;
using NotesApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notes API", Version = "v1" }));

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INoteRepository, NoteRepository>();
//builder.Services.AddScoped<ITagRepository, TagRepository>();    

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateNoteCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAllNotesQuery).Assembly);
    //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API v1"));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
