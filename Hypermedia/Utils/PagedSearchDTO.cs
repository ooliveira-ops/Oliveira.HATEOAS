using System.Xml.Serialization;
using Erudio.HATEOAS.Hypermedia.Abstract;


namespace Erudio.HATEOAS.Hypermedia.Utils
{
	// "<T>" = genérico, servindo p qlqr tipo de obj... "desde que T suporte Hypermedia"
	public class PagedSearchDTO<T> where T : ISupportsHypermedia
	{
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int TotalResults { get; set; }
		public string SortFields { get; set; }
		public string SortDirections { get; set; } = "asc";

		[XmlIgnore]
		public Dictionary<string, object> Filters { get; set; } = [];
		public List<T> List { get; set; } = []; //vazia ([])
		public PagedSearchDTO() { }

		 public PagedSearchDTO(
			 int currentPage,
			 int pageSize,
			 string sortFields,
			 string sortDirections,
			 Dictionary<string, object> filters)
		{
			CurrentPage = currentPage;
			PageSize = pageSize;
			SortFields = sortFields;
			SortDirections = sortDirections;
			Filters = filters ?? [];
		}
		 public PagedSearchDTO(
			 int currentPage,
			 string sortFields,
			 string sortDirections
			 )
			: this(currentPage, 10, sortFields, sortDirections, null) { }


		public int GetCurrentPage() => CurrentPage  == 0 ? 1 : CurrentPage;
		public int GetPageSize() => PageSize == 0 ? 10 : PageSize;
	}
}
