using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Pagination
{
    public class PaginationRequest
    {
        private const int MaxPageSize = 100;

        private int _pageIndex = 1;
        private int _pageSize = 10;

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 10,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }
    }
}
