using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentsListViewModel
    {
        public List<DocumentsViewModel> documentsViewModels { get; set; }
        public List<ItemsViewModel> itemsViewModels { get; set; }
    }
}