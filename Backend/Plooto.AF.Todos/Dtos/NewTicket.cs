using System.Collections.Generic;

namespace Plooto.AF.Todos.Dtos
{
    public class NewTicket
    {
        public string Description { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
