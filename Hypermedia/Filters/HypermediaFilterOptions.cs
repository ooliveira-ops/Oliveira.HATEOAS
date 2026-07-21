using Erudio.HATEOAS.Hypermedia.Abstract;

namespace Erudio.HATEOAS.Hypermedia.Filters
{
	public class HypermediaFilterOptions
	{
		public List<IResponseEnricher> ContentResponseEnricherList { get; set; } = [];
	}
}
