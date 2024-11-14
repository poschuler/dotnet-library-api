using System;
using System.Net;
using FluentAssertions;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests
    : IClassFixture<LibraryApiFactory>, IAsyncLifetime
{

    private readonly LibraryApiFactory _factory;
    private readonly List<string> _createdIsbns = new();

    public LibraryEndpointsTests(LibraryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.ToString().EndsWith($"/books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "INVALID";

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");

    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with the same ISBN already exists.");
    }

    [Fact]
    public async Task GetBook_ReturnBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        existingBook.Should().BeEquivalentTo(book);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBook_ReturnNotFound_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var result = await httpClient.GetAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBook_ReturnAllBooks_WhenBooksExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        // Act
        var result = await httpClient.GetAsync($"/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        //returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetAllBook_ReturnNoBooks_WhenNoBooksExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync($"/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        //returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task SearchBooks_ReturnBooks_WhenTitleMatched()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        // Act
        var result = await httpClient.GetAsync($"/books?searchTerm={book.Title}");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        book.Title = "New Title";

        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task UpdateBook_DoesNotUpdatesBook_WhenDataIsInCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        book.Title = "New Title";

        // Act
        book.Title = string.Empty;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Title");
        //error.ErrorMessage.Should().Be("");        
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnNotFound_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var result = await httpClient.DeleteAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_DeletesBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private Book GenerateBook(string title = "The Dirty Coder")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Paul Osorio",
            PageCount = 100,
            ShortDescription = "Short description test",
            ReleaseDate = DateTime.Now
        };
    }

    private string GenerateIsbn()
    {
        //978-3-16-148410-0
        return $"{Random.Shared.Next(100, 999)}-" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();

        foreach (var isbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{isbn}");
        }
    }
}
