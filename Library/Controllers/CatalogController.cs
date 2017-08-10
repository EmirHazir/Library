using Library.Models.Catalog;
using LibraryData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.Controllers
{
    public class CatalogController : Controller
    {
        #region DependenceInjection
        private ILibraryAsset _asset;
        public CatalogController(ILibraryAsset asset)
        {
            this._asset = asset;
        } 
        #endregion


        public IActionResult Index()
        {
            //Model tanımlayıp metodtan çektim
            var assetModels = _asset.GetAll();
            //Listeleyeceğim veri ViewModelde new diyerek verileri çektim
            var listingResult = assetModels
                .Select(result => new AssetIndexListModel
                {
                    Id = result.Id,
                    ImgUrl = result.ImageUrl,
                    AuthorOrDirector = _asset.GetAuthorOrdirectory(result.Id),
                    Title = result.Title,
                    Type = _asset.GetType(result.Id)
                });

            //AssetIndexListModel i IEnumable tipte AssetIndexModel bastım.Amaç verinin complex olması ve ulaşılabilirliğin zorluğu
            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };
            return View(model);
        }



    }
}
