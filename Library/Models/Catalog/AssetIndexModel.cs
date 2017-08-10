using Library.Models.Catalog;
using System.Collections.Generic;

namespace Library.Models.Catalog
{
    public class AssetIndexModel
    {
        public IEnumerable<AssetIndexListModel> Assets { get; set; }
    }
}
