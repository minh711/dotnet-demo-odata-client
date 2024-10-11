using System.Collections.Generic;

namespace client.Models
{
    public class ODataResponse<T>
    {
        public IEnumerable<T> Value { get; set; }
        public int Count { get; set; }
    }
}
