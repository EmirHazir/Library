using LibraryData;
using System;
using System.Collections.Generic;
using System.Text;
using LibraryData.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {

        #region Dependence Injection
        private LibraryContext _context;
        public CheckoutService(LibraryContext context)
        {
            this._context = context;
        }
        #endregion


        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(x => x.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(x => x.LibraryAsset)
                .Include(x => x.LibraryCard)
                .Where(x => x.LibraryAsset.Id == id);
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(x => x.LibraryAsset)
                .Where(x => x.LibraryAsset.Id == id);
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts.Where(x => x.LibraryAsset.Id == assetId)
                .OrderByDescending(x => x.Since)
                .FirstOrDefault();
        }


        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;

            UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId, now);

            _context.SaveChanges();
        }

        #region MarkFoundAndLost Methods
        private void UpdateAssetStatus(int assetId, string v)
        {
            var item = _context.LibraryAssets
                .FirstOrDefault(x => x.Id == assetId);
            item.Status = _context.Statuses.FirstOrDefault(x => x.Name == "Available");
            _context.Update(item);
        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {

            var history = _context.CheckoutHistories.FirstOrDefault(x => x.LibraryAsset.Id == assetId && x.CheckedIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckedOut = now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _context.Checkouts.FirstOrDefault(x => x.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }
        #endregion

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");

            _context.SaveChanges();
        }

        public void CheckInItem(int assetId, int libraryCarId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);


            //remove any existing checkouts
            RemoveExistingCheckouts(assetId);
            //close any existing checkouts
            CloseExistingCheckoutHistory(assetId, now);
            //look for existing item hold
            var currentHolds = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == assetId);

            //if there are holds, checkout the item to the Library with the earliest hold
            if (currentHolds.Any())
            {
                CheckoutToEarlestHold(assetId, currentHolds);
            }

            //otherwise, update the item status to available
            UpdateAssetStatus(assetId, "Available");
            _context.SaveChanges();
        }

        
        private void CheckoutToEarlestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds.OrderBy(h => h.HoldPlaced).FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckOutItem(assetId, card.Id);
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckOut(assetId))
            {
                return;
                //add logic here handle feedback to user;
            }

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            UpdateAssetStatus(assetId, "Checkd Out");

            var libraryCard = _context.LibraryCards.Include(c => c.Checkouts).FirstOrDefault(c => c.Id == libraryCardId);

            var now = DateTime.Now;
            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);

            _context.SaveChanges();

        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        private bool IsCheckOut(int assetId)
        {
            return _context.Checkouts.Where(co => co.LibraryAsset.Id == assetId).Any();

        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets.FirstOrDefault(c => c.Id == assetId);

            var card = _context.LibraryCards.FirstOrDefault
                (c => c.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }
            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };

            _context.AddAsync(hold);
            _context.SaveChanges();

        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h=>h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;

        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId)
                .HoldPlaced;
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout == null)
            {
                return "Not checked out";
            }
            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(h => h.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(x => x.LibraryAsset)
                .Include(x => x.LibraryCard)
                .FirstOrDefault(x => x.LibraryAsset.Id == assetId);

        }
    }
}
