using Erudio.HATEOAS.Hypermedia;

namespace Erudio.HATEOAS.Hypermedia.Abstract
{
	public interface  ISupportsHypermedia
	{
		List<HypermediaLink> Links { get; set; }
	}
}
