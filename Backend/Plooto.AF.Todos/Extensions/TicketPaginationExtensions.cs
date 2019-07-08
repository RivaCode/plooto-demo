using Plooto.AF.Todos.Dtos;
using System.Linq;
using System.Text;
using System.Web;

namespace Plooto.AF.Todos.Extensions
{
    internal static class TicketPaginationExtensions
    {
        public static string FirstPage(this TicketQuery @this) =>
            @this.PageNumber != 1 ? @this.ToQueryString(1, @this.PageSize) : null;

        public static string LastPage(this TicketQuery @this, int totalPages) =>
            @this.PageNumber != totalPages ? @this.ToQueryString(totalPages, @this.PageSize) : null;

        public static string NextPage(this TicketQuery @this, int totalPages) =>
            @this.PageNumber < totalPages ? @this.ToQueryString(@this.PageNumber + 1, @this.PageSize) : null;

        public static string PreviousPage(this TicketQuery @this) =>
            @this.PageNumber > 1 ? @this.ToQueryString(@this.PageNumber - 1, @this.PageSize) : null;

        private static string ToQueryString(this TicketQuery @this, int pageNumber, int pageSize)
        {
            var newTicketQuery = @this.NewPageNumber(pageNumber).NewPageSize(pageSize);
            var propertiesQuery =
                from p in newTicketQuery.GetType().GetProperties()
                where p.GetValue(newTicketQuery, null) != null
                select $"{p.Name}={HttpUtility.UrlEncode(p.GetValue(newTicketQuery, null).ToString())}";

            return new StringBuilder().AppendJoin("&", propertiesQuery).ToString();
        }
    }
}
