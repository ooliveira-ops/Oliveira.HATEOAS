# Oliveira.HATEOAS 🔗

[![NuGet](https://img.shields.io/nuget/v/Oliveira.HATEOAS.svg)](https://www.nuget.org/packages/Oliveira.HATEOAS)
[![GitHub repo](https://img.shields.io/badge/GitHub-Repository-green.svg)](https://github.com/ooliveira-ops/Oliveira.HATEOAS)
![Last Commit](https://img.shields.io/github/last-commit/ooliveira-ops/Oliveira.HATEOAS)
[![.NET 10 Continuous Integration with GitHub, GitHub Actions and Nuget Packages.](https://github.com/ooliveira-ops/Oliveira.HATEOAS/actions/workflows/continuous-integration-nuget.yaml/badge.svg?branch=main)](https://github.com/ooliveira-ops/Oliveira.HATEOAS/actions/workflows/continuous-integration-nuget.yaml)

> 📚 **Study project** — this package was built as part of my studies in the Udemy course [**ASP.NET 2026 do 0 à Azure e GCP com ASP.NET 10, Docker e K8s**](https://www.udemy.com/course/restful-apis-do-0-a-nuvem-com-aspnet-core-e-docker/) by Leandro Costa / Erudio.

This is a smart library to implement the HATEOAS pattern in your RESTful APIs, implemented based on [this project](https://github.com/SotirisH/HyperMedia).

---

## 📦 Installation

### Package Manager Console

```bash
Install-Package Oliveira.HATEOAS -Version 10.0.301.4
```

### NuGet Package Manager

![NuGet Package Manager](docs/images/nuget-search.png)

---

## 🚀 How to use

### 1. Implement *ISupportsHypermedia* in your exposed object

```csharp
using RestWithASPNET10Erudio.Hypermedia;
using RestWithASPNET10Erudio.Hypermedia.Abstract;

namespace RestWithASPNET10Erudio.Data.DTO.V1
{
	public class BookDTO : ISupportsHypermedia
	{
		public long Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
		public decimal Price { get; set; }
		public DateTime LaunchDate { get; set; }

		public List<HypermediaLink> Links { get; set; } = [];
	}
}
```

### 2. Implement your enricher with *ContentResponseEnricher\<T\>*

```csharp
using Microsoft.AspNetCore.Mvc;
using RestWithASPNET10Erudio.Data.DTO.V1;
using RestWithASPNET10Erudio.Hypermedia.Constants;

namespace RestWithASPNET10Erudio.Hypermedia.Enricher
{
	public class BookEnricher : ContentResponseEnricher<BookDTO>
	{
		protected override Task EnrichModel(
			BookDTO content, IUrlHelper urlHelper)
		{
			var request = urlHelper.ActionContext.HttpContext.Request;
			var baseUrl = $"{request.Scheme}://" +
				$"{request.Host.ToUriComponent()}" +
				$"{request.PathBase.ToUriComponent()}/api/book/v1";
			content.Links.AddRange(GenerateLinks(content.Id, baseUrl));
			return Task.CompletedTask;
		}
		private IEnumerable<HypermediaLink> GenerateLinks(long id, string baseUrl)
		{
			//return new List<HypermediaLink>
			return
			[
				// This new HypermediaLink is equal to new() in C# 9.0
				new ()
				{
					Rel = RelationType.COLLECTION,
					Href = $"{baseUrl}",
					Type = ResponseTypeFormat.DefaultGet,
					Action = HttpActionVerb.GET
				},
				new ()
				{
					Rel = RelationType.SELF,
					Href = $"{baseUrl}/{id}",
					Type = ResponseTypeFormat.DefaultGet,
					Action = HttpActionVerb.GET
				},
				new ()
				{
					Rel = RelationType.CREATE,
					Href = $"{baseUrl}",
					Type = ResponseTypeFormat.DefaultPost,
					Action = HttpActionVerb.POST
				},
				new ()
				{
					Rel = RelationType.UPDATE,
					Href = $"{baseUrl}",
					Type = ResponseTypeFormat.DefaultPut,
					Action = HttpActionVerb.PUT
				},
				new ()
				{
					Rel = RelationType.DELETE,
					Href = $"{baseUrl}/{id}",
					Type = ResponseTypeFormat.DefaultDelete,
					Action = HttpActionVerb.DELETE
				}
			];
		}
	}
}
```

### 3. Expose your endpoints in the controller

The `HyperMediaFilter` enriches every `Ok(...)` response, so your controller stays focused on the business logic.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestWithASPNET10Erudio.Data.DTO.V1;
using RestWithASPNET10Erudio.Model;
using RestWithASPNET10Erudio.Services;

namespace RestWithASPNET10Erudio.Controllers.V1
{
	[ApiController]
	[Route("api/[controller]/v1")]
	[Authorize("Bearer")]
	public class BookController : ControllerBase
	{
		private readonly IBookServices _bookService;
		private readonly ILogger<BookController> _logger;

		public BookController(IBookServices bookService, ILogger<BookController> logger)
		{
			_bookService = bookService;
			_logger = logger;
		}

		[HttpGet]
		[ProducesResponseType(200, Type = typeof(BookDTO))]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		public IActionResult FindAll()
		{
			return Ok(_bookService.FindAll());
		}

		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(BookDTO))]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		public IActionResult FindByID(long id)
		{
			var book = _bookService.FindByID(id);
			if (book == null) return NotFound();
			return Ok(book);
		}

		[HttpPost]
		[ProducesResponseType(200, Type = typeof(BookDTO))]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		public IActionResult Post([FromBody] BookDTO book)
		{
			_logger.LogInformation("Creating new Book: {title}", book.Title);
			var createdBook = _bookService.Create(book);
			if (createdBook == null)
			{
				_logger.LogError("Failed to create book with title {title}", book.Title);
				return NotFound();
			}
			return Ok(createdBook);
		}

		[HttpPut]
		[ProducesResponseType(200, Type = typeof(BookDTO))]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		public IActionResult Update([FromBody] BookDTO book)
		{
			return Ok(_bookService.Update(book));
		}

		[HttpDelete("{id}")]
		[ProducesResponseType(204, Type = typeof(BookDTO))]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		public IActionResult Delete(long id)
		{
			_bookService.Delete(id);
			return NoContent();
		}
	}
}
```

### 4. Register the *HyperMediaFilter* and your enrichers in your Program.cs

```csharp
builder.Services.AddControllers(options =>
{
	options.Filters.Add<HyperMediaFilter>();
});

var filterOptions = new HyperMediaFilterOptions();
filterOptions.ContentResponseEnricherList.Add(new PersonEnricher());
filterOptions.ContentResponseEnricherList.Add(new BookEnricher());
```

### 5. Enjoy 🎉

#### Response as JSON

```json
[
    {
        "id": 1,
        "title": "Working effectively with legacy code",
        "author": "Michael C. Feathers",
        "price": 49.00,
        "launchDate": "2017-11-29T13:50:05.878",
        "links": [
            {
                "rel": "collection",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "GET"
            },
            {
                "rel": "self",
                "href": "https://localhost:44300/api/book/v1/1",
                "type": "application/json",
                "action": "GET"
            },
            {
                "rel": "create",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "POST"
            },
            {
                "rel": "update",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "PUT"
            },
            {
                "rel": "delete",
                "href": "https://localhost:44300/api/book/v1/1",
                "type": "no-content",
                "action": "DELETE"
            }
        ]
    },
    {
        "id": 2,
        "title": "Design Patterns",
        "author": "Ralph Johnson, Erich Gamma, John Vlissides e Richard Helm",
        "price": 45.00,
        "launchDate": "2017-11-29T15:15:13.636",
        "links": [
            {
                "rel": "collection",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "GET"
            },
            {
                "rel": "self",
                "href": "https://localhost:44300/api/book/v1/2",
                "type": "application/json",
                "action": "GET"
            },
            {
                "rel": "create",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "POST"
            },
            {
                "rel": "update",
                "href": "https://localhost:44300/api/book/v1",
                "type": "application/json",
                "action": "PUT"
            },
            {
                "rel": "delete",
                "href": "https://localhost:44300/api/book/v1/2",
                "type": "no-content",
                "action": "DELETE"
            }
        ]
    }
]
```

#### Response as XML

```xml
<ArrayOfBookDTO xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <BookDTO>
        <Id>1</Id>
        <Title>Working effectively with legacy code</Title>
        <Author>Michael C. Feathers</Author>
        <Price>49.00</Price>
        <LaunchDate>2017-11-29T13:50:05.878</LaunchDate>
        <Links>
            <HypermediaLink Rel="collection" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="GET" />
            <HypermediaLink Rel="self" Href="https://localhost:44300/api/book/v1/1" Type="application/json" Action="GET" />
            <HypermediaLink Rel="create" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="POST" />
            <HypermediaLink Rel="update" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="PUT" />
            <HypermediaLink Rel="delete" Href="https://localhost:44300/api/book/v1/1" Type="no-content" Action="DELETE" />
        </Links>
    </BookDTO>
    <BookDTO>
        <Id>2</Id>
        <Title>Design Patterns</Title>
        <Author>Ralph Johnson, Erich Gamma, John Vlissides e Richard Helm</Author>
        <Price>45.00</Price>
        <LaunchDate>2017-11-29T15:15:13.636</LaunchDate>
        <Links>
            <HypermediaLink Rel="collection" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="GET" />
            <HypermediaLink Rel="self" Href="https://localhost:44300/api/book/v1/2" Type="application/json" Action="GET" />
            <HypermediaLink Rel="create" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="POST" />
            <HypermediaLink Rel="update" Href="https://localhost:44300/api/book/v1" Type="application/json" Action="PUT" />
            <HypermediaLink Rel="delete" Href="https://localhost:44300/api/book/v1/2" Type="no-content" Action="DELETE" />
        </Links>
    </BookDTO>
</ArrayOfBookDTO>
```

