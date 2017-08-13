﻿using LibraryData.Models;
using System;
using System.Collections.Generic;

namespace LibraryData
{
    public interface ICheckout
    {
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Checkout> GetAll();
        IEnumerable<Hold> GetCurrentHolds(int id);

        Checkout GetById(int checkoutId);
        Checkout GetLatestCheckout(int assetId);

        void MarkLost(int assetId);
        void MarkFound(int assetId);
        void PlaceHold(int assetId, int libraryCardId);
        void Add(Checkout newCheckout);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId, int libraryCarId);

        string GetCurrentCheckoutPatron(int assetId);
        string GetCurrentHoldPatronName(int id);

        DateTime GetCurrentHoldPlaced(int id);

        
    }
}
