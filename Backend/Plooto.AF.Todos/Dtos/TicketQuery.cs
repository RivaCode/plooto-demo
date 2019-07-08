namespace Plooto.AF.Todos.Dtos
{
    public class TicketQuery
    {
        public string Filter { get; set; }
        public string Fields { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;

        public TicketQuery NewPageSize(int pageSize) =>
            new TicketQuery
            {
                PageSize = pageSize,
                Fields = Fields,
                PageNumber = PageNumber,
                Filter = Filter
            };


        public TicketQuery NewPageNumber(int pageNumber) =>
            new TicketQuery
            {
                PageSize = PageSize,
                Fields = Fields,
                PageNumber = pageNumber,
                Filter = Filter
            };
    }
}
