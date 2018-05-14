using System;
using System.Collections.Generic;
using System.Text;

namespace Peshitta.Data.Models
{
    public class Book
    {

        public override bool Equals(object obj)
        {         
            if (obj is Book book)
            {
                return book.bookid == this.bookid;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return bookid;
        }
        public int bookid { get; set; }
        public string Title { get; set; }

        public string abbreviation { get; set; }
    }
}
