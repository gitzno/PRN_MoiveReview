using System;
using System.Collections.Generic;

namespace FilmInfor.Models
{
    public partial class Movie
    {
        public Movie()
        {
            Rates = new HashSet<Rate>();
        }

        public Movie(string title, int year, string image, string description, int genreId)
        {
            Title = title;
            Year = year;
            Image = image;
            Description = description;
            GenreId = genreId;
        }

        public int MovieId { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public int? GenreId { get; set; }

        public virtual Genre Genre { get; set; }
        public virtual ICollection<Rate> Rates { get; set; }
    }
}
