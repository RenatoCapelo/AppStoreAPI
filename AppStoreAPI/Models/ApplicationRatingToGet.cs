using System.Collections.Generic;

namespace AppStoreAPI.Models
{
    public class ApplicationRatingToGet
    {
        public double average { get; set; }
        public List<ApplicationRatings_dbo> ratings { get; set; }
    }
}
