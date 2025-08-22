

namespace AgencyStore.Core.Models
{
    public class PropertyImage
    {
        public const int MAX_URL_LENGTH = 500;

        private PropertyImage(Guid id, Guid propertyId, string url, bool isMain, int order)
        {
            Id = id;
            PropertyId = propertyId;
            Url = url;
            IsMain = isMain;
            Order = order;
        }

        public Guid Id { get; }
        public Guid PropertyId { get; }
        public string Url { get; }
        public bool IsMain { get; }
        public int Order { get; }

        public static (PropertyImage image, string error) Create(Guid id, Guid propertyId, string url,
            bool isMain = false, int order = 0)
        {
            var error = string.Empty;

            if (string.IsNullOrEmpty(url))
            {
                error = "Image URL cannot be empty";
            }
            else if (url.Length > MAX_URL_LENGTH)
            {
                error = $"URL cannot be longer than {MAX_URL_LENGTH} symbols";
            }

            var image = new PropertyImage(id, propertyId, url, isMain, order);
            return (image, error);
        }
    }
}
