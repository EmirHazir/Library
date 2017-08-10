using LibraryData;
using System;
using LibraryData.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryServices
{
    public class LibraryAssetService : ILibraryAsset
    {

        private LibraryContext _context;
        public LibraryAssetService(LibraryContext context)
        {
            this._context = context;
        }

        public void Add(LibraryAsset newAsset)
        {
            _context.Add(newAsset);
            _context.SaveChanges();
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
           return  _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);
        }

        

        public LibraryAsset GetById(int id)
        {
            return GetAll()
                .FirstOrDefault(x=>x.Id == id);

            //Böyle de yazabilirim
            //return _context.LibraryAssets
            //    .Include(asset => asset.Status)
            //    .Include(asset => asset.Location)
            //    .FirstOrDefault(x => x.Id == id); 

        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return GetById(id).Location;
           
           //Böyle de yazabilirim
           // return _context.LibraryAssets.FirstOrDefault(x => x.Id == id).Location;
        }

        public string GetDeweyIndex(int id)
        {
            //Eğer Kitapların içinde gelen idler eşitse DeveyIndexi bas değilse boş bas
            if (_context.Books.Any(x=>x.Id == id))
            {
              return  _context.Books.FirstOrDefault(x => x.Id == id).DeweyIndex;
            }
            else
            {
                return "";
            }
        }

        public string GetISBN(int id)
        {
            //Eğer Kitapların içinde gelen idler eşitse ISBN bas değilse boş bas
            if (_context.Books.Any(x => x.Id == id))
            {
                return _context.Books.FirstOrDefault(x => x.Id == id).ISBN;
            }
            else
            {
                return "";
            }
        }

        public string GetTitle(int id)
        {
            //Eğer Kitapların içinde gelen idler eşitse Title bas değilse boş bas
            if (_context.Books.Any(x => x.Id == id))
            {
                return _context.Books.FirstOrDefault(x => x.Id == id).Title;
            }
            else
            {
                return "";
            }
        }

        public string GetType(int id)
        {
            //Book u belirle
            var book = _context.LibraryAssets.OfType<Book>()
                .Where(x => x.Id == id);
            //eğer gelen id book tan herhangibiriyse Book bas değilse Video bas
            return book.Any() ? "Book" : "Video";
        }

        public string GetAuthorOrdirectory(int id)
        {
            //Book nereden
            var isBook = _context.LibraryAssets.OfType<Book>()
                .Where(x => x.Id == id).Any();
            //Video nereden
            var isVideo = _context.LibraryAssets.OfType<Video>()
                .Where(x => x.Id == id).Any();

            //Eğer book Id istemişse ver istememişse videonun Directorunu ver yoksa bilinmiyor bas
            return isBook ?
                _context.Books.FirstOrDefault(x => x.Id == id).Author
                          :
                _context.Videos.FirstOrDefault(x => x.Id == id).Director
                          ?? "Unknown";

        }
    }
}
