using FilmInfor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FilmInfor.Controllers
{
    public class Admin : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly CenimaDBContext _db = new CenimaDBContext();
        public Admin(IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {

            return View();
        }
        public ActionResult LoginAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(IFormCollection form)
        {
            
                String Username = form["UserName"];
                String pass = form["Password"];
                Person p = _db.Persons.Where(b => b.Email ==Username)
                    .FirstOrDefault();
                if (p != null)
                {
                if (p.Password == pass)
                {
                    if (p.Type == 1)
                    {

                        _contextAccessor.HttpContext.Session.SetString("Username", Username);
                        return RedirectToAction("DashboardMovie");
                        
                    }
                }               
            }
            
            return RedirectToAction("LoginAdmin");
        }
        public bool IsLoggedIn()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public ActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            return RedirectToAction("LoginAdmin");
        }

        public ActionResult DashboardMovie()
        {

            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            var ListMovie = _db.Movies.ToList();
        
            return View(ListMovie);
        }

        public ActionResult RemoveMovie(Movie m)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            String msg = "";
            var movie = _db.Movies.Find(m.MovieId);
            if (movie != null)
            {
                _db.Movies.Remove(movie);
                _db.SaveChanges();
                msg = "done";
            }
            return RedirectToAction("DashboardMovie");
        }

        public ActionResult FormMovie(Movie Movie)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            ViewBag.Movie = Movie;
            var ListGenres = _db.Genres.ToList();
            return View(ListGenres);
        }
        [HttpPost]
        public ActionResult AddMovie(IFormCollection form)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            String Title = form["Title"];
            int Year = int.TryParse(form["Year"], out int result) ? result : 0; 
            String Image = form["Image"];
            String Description = form["Title"];
            int GenreId = 1;
            if (!string.IsNullOrEmpty(form["GenreId"]))
            {
                int.TryParse(form["GenreId"], out GenreId);
            }

            Movie m = new Movie(Title, Year, Image, Description, GenreId);
            _db.Movies.Add(m);
            _db.SaveChanges();
            return RedirectToAction("DashboardMovie");


        }
        public ActionResult DashboardPerson()
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            if (false)
                return RedirectToAction("LoginAdmin");
            var ListMovie = _db.Persons.ToList();

            return View(ListMovie);
        }
        public ActionResult FormPerson(Person Person)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            ViewBag.Person = Person;
            return View();
        }
        [HttpPost]
        public ActionResult AddPerson(IFormCollection form)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            String Title = form["Fullname"];
            String Gender = form["Gender"];
            String Email = form["Email"];
            String Password = form["Password"];
            int Type = 2;
            if (!string.IsNullOrEmpty(form["Type"]))
            {
                int.TryParse(form["Type"], out Type);
            }
            bool isActive = form["active"] == "true";
            Person p = new Person()
            {
                Fullname = Title,
             Gender = Gender,
             Email = Email,
             Password = Password,
                Type = Type,
               IsActive = isActive
            };
            _db.Persons.Add(p);
            _db.SaveChanges();
            return RedirectToAction("DashboardPerson");
        }
        public ActionResult DashboardGenre()
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            if (false)
                return RedirectToAction("LoginAdmin");
            var ListMovie = _db.Genres.ToList();

            return View(ListMovie);
        }
        public ActionResult FormGenre(Genre Person)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            ViewBag.Genre = Person;
            return View();
        }
        public ActionResult AddGenre(IFormCollection form)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            String Description = form["Description"];
            
           
            Genre p = new Genre()
            {
                Description = Description
            };
            _db.Genres.Add(p);
            _db.SaveChanges();
            return RedirectToAction("DashboardGenre");
        }
        public ActionResult DashboardRate()
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            if (false)
                return RedirectToAction("LoginAdmin");
            var ListMovie = _db.Rates.ToList();

            return View(ListMovie);
        }
        public ActionResult RemoveRate(Rate m)
        {
            if (IsLoggedIn())
                return RedirectToAction("LoginAdmin");
            String msg = "";
            var rate = _db.Rates.Find(m.MovieId, m.PersonId);
            if (rate != null)
            {
                _db.Rates.Remove(rate);
                _db.SaveChanges();
                msg = "done";
            }
            return RedirectToAction("DashboardRate");
        }
       
       
    }
}
