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
        private ICheckout _checkout;
        public CatalogController(ILibraryAsset asset, ICheckout checkout)
        {
            this._asset = asset;
            this._checkout = checkout;
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
                    DeweyCallNumber = _asset.GetDeweyIndex(result.Id),
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

        public IActionResult Detail(int id)
        {
            var asset = _asset.GetById(id);
            var currentHolds = _checkout.GetCurrentHolds(id)
                .Select(x => new AssetHoldModel {
                    HoldPlaced = _checkout.GetCurrentHoldPlaced(x.Id),
                    PatronName = _checkout.GetCurrentHoldPatronName(x.Id)
                });
            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Type = _asset.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                CurrentLocation = _asset.GetCurrentLocation(id).Name,
                Dewey = _asset.GetDeweyIndex(id),
                AuthorOrDirector = _asset.GetAuthorOrdirectory(id),
                CheckoutHistory = _checkout.GetCheckoutHistory(id),
                ISBN = _asset.GetISBN(id),
                LatestCheckout =  _checkout.GetLatestCheckout(id),
                PatronName  = _checkout.GetCurrentCheckoutPatron(id),
                CurrentHolds = currentHolds

            };

            return View(model);
        }



    }
}
