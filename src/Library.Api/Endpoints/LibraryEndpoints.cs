using System;
using FluentValidation;
using FluentValidation.Results;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;

namespace Library.Api.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "/books";

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {

        app.MapPost(BaseRoute, CreateBookAsync)
        .WithName("CreateBook")
        .Accepts<Book>(ContentType)
        .Produces<Book>(201)
        .Produces<IEnumerable<ValidationFailure>>(400)
        .WithTags(Tag);

        app.MapGet(BaseRoute, GetAllBooksAsync)
        .WithName("GetBooks")
        .Produces<IEnumerable<Book>>(200)
        .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookAsync).WithName("GetBook")
        .Produces<Book>(200)
        .Produces(404)
        .WithTags(Tag);

        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
        .WithName("UpdateBook")
        .Accepts<Book>(ContentType)
        .Produces<Book>(200)
        .Produces<IEnumerable<ValidationFailure>>(400)
        .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
        .WithName("DeleteBook")
        .Produces(204)
        .Produces(404)
        .WithTags(Tag);

        // app.MapGet("/status",
        // () =>
        // {
        //     return Results.Extensions.Html(@"<!DOCTYPE html>
        //     <html>
        //     <head>
        //         <title>Library API</title>
        //     </head>
        //     <body>
        //         <h1>Library API</h1>
        //         <p>Status: <strong>Running</strong></p>
        //     </body>
        //     </html>");
        // })
        // .ExcludeFromDescription()
        // .RequireCors("AnyOrigin");

    }

    internal static async Task<IResult> CreateBookAsync(
        Book book,
        IBookService bookService,
        IValidator<Book> validator,
        LinkGenerator linker
    )
    {
        var validationResult = await validator.ValidateAsync(book);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var result = await bookService.CreateAsync(book);

        if (!result)
        {
            return Results.BadRequest(new List<ValidationFailure>
                {
                    new ("Isbn", "A book with the same ISBN already exists.")
                });
        }

        //var link = linker.GetPathByName("GetBook", new { isbn = book.Isbn });
        //return Results.Created(link, book);
        return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
        //return Results.Created($"/books/{book.Isbn}", book);
    }

    internal static async Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm!);
            return Results.Ok(matchedBooks);
        }

        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }

    internal static async Task<IResult> GetBookAsync(string isbn, IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);

        if (book is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(book);
    }

    internal static async Task<IResult> UpdateBookAsync(
        string isbn,
        Book book,
        IBookService bookService,
        IValidator<Book> validator
    )
    {
        book.Isbn = isbn;

        var validationResult = await validator.ValidateAsync(book);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var updated = await bookService.UpdateAsync(book);

        if (!updated)
        {
            return Results.NotFound();
        }

        return Results.Ok(book);
    }

    internal static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
    {
        var book = await bookService.DeleteAsync(isbn);

        if (!book)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }
}
