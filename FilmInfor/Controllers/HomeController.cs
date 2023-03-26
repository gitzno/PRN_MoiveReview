using FilmInfor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;

namespace FilmInfor.Controllers
{
    public class HomeController : Controller
    {        
        private readonly CenimaDBContext _db;

        public HomeController(CenimaDBContext db)
        {
            _db = db;
        }        

        public IActionResult Index()
        {
            var genres = _db.Genres.ToList();
            ViewData["genres"] = genres;
            var movies = _db.Movies.Include(m=>m.Genre).Include(m=>m.Rates).Take(8).ToList();
            ViewData["movies"] = movies;
            ViewData["fullSize"] = _db.Movies.Count();
            return View();
        }
		public IActionResult Login()
		{
			ViewData["message"] = string.Empty;
			return View();
		}

		[HttpPost]
		public IActionResult Login(Person p1)
		{
			Person p = _db.Persons.FirstOrDefault(m => m.Email == p1.Email && m.Password == p1.Password && m.Type == 2);
			if (p == null)
			{
				ViewData["message"] = "thatbai";
				return View();
			}

			HttpContext.Session.SetString("account", JsonSerializer.Serialize(p));
			return RedirectToAction("Index");
		}

		public IActionResult SignOut()
		{
			if (HttpContext.Session.GetString("account") != null)
				HttpContext.Session.Remove("account");
			return RedirectToAction("Index", "Home");
		}

		public IActionResult SignUp()
		{
			ViewData["message"] = string.Empty;
			return View();
		}

		[HttpPost]
		public IActionResult SignUp(Person p, string confirmpass)
		{
			if (p.Password != confirmpass)
			{
				ViewData["message"] = "Password is not same Confirm Password!";
				return View("SignUp");
			}
			Person ps = _db.Persons.FirstOrDefault(m => m.Email == p.Email);
			if (ps == null)
			{
				p.Type = 2;
				p.IsActive = true;
				_db.Persons.Add(p);
				_db.SaveChanges();
			}
			else
			{
				ViewData["message"] = "Email is existed!";
				return View("SignUp");
			}
			return RedirectToAction("Login");
		}

		public JsonResult LoadMore(int size, string svalue, int genreid)
        {
            Person ps = null;

			if (HttpContext.Session.GetString("account") != null)
            {
				ps = JsonSerializer.Deserialize<Person>(HttpContext.Session.GetString("account"));
			}
            
            if (string.IsNullOrEmpty(svalue))
            {
                svalue = "";
            }
            var temp = ConvertToUnSign(svalue.ToLower());
            var movies = _db.Movies.Include(m => m.Genre).Include(m => m.Rates).ToList();
            if (genreid != 0)
            {
                movies = movies.Where(m => m.GenreId == genreid).ToList();
            }
            movies = movies.FindAll(m => ConvertToUnSign(m.Title.ToLower()).Contains(temp)).Skip(size).Take(8).ToList();            
            ViewData["movies"] = movies;
            var html = "";
            foreach (Movie movie in movies)
            {
                dynamic score = 0.0;
                var count = 0;
                foreach (Rate rate in movie.Rates)
                {
                    score += (double)rate.NumericRating;
                    count++;
                }
                if (count != 0)
                {
                    score /= count;
                }
                else
                {
                    score = "";
                }
                html += $"<div class=\"card\" style=\"width: 195px; margin-top: 10px;margin-left:15px\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}'><img class=\"card-img-top\" style=\"height:10rem;\" src=\"{(string.IsNullOrEmpty(movie.Image)?"./Img/No_Image_Available.jpg":movie.Image)}\" alt=\"Card\" /></a>\r\n" +
                    $"<div class=\"card-body\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}' class=\"text-decoration-none\"><h5 class=\"card-title\">{movie.Title}</h5></a>\r\n" +
                    $"<p class=\"card-text\">Năm: {movie.Year}</p>\r\n" +
                    $"<p class=\"card-text\">Loại: {movie.Genre.Description}</p>\r\n" +
                    $"<p class=\"card-text\">Điểm: {score}</p>\r\n" +
                    $"<div class=\"d-flex justify-content-center\">\r\n" +
                    ((ps != null)?$"<a href='Home/Detail?id={movie.MovieId}' class=\"btn btn-primary\">Đánh giá</a>\r\n":$"<a href='Home/Login' class=\"btn btn-primary\">Đánh giá</a>") +
                    $"</div>\r\n" +
                    $"</div>\r\n" +
                    $"</div>\n";
            }
            return Json(new
            {
                html = html,
                size = movies.Count
            });
        }

        public IActionResult Detail(int id)
        {
            var movie = _db.Movies.Include(m=>m.Genre).Include(m=>m.Rates).FirstOrDefault(m=>m.MovieId==id);
            var cmt = _db.Rates.Include(r => r.Person).Where(r=>r.MovieId==movie.MovieId).ToList();
            ViewData["movie"] = movie;
            ViewData["cmt"] = cmt;
            return View();
        }

        public string ConvertToUnSign(string s)
        {            
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        [HttpGet]
        public JsonResult Search(string value, int genreid)
        {
			Person ps = null;

			if (HttpContext.Session.GetString("account") != null)
			{
				ps = JsonSerializer.Deserialize<Person>(HttpContext.Session.GetString("account"));
			}
			if (string.IsNullOrEmpty(value))
            {
                value = "";
            }
            var temp = ConvertToUnSign(value.ToLower());
            var movies = _db.Movies.Include(m => m.Genre).Include(m => m.Rates).ToList();
            if (genreid != 0)
            {
                movies = movies.Where(m => m.GenreId == genreid).ToList();
            }                
            movies = movies.FindAll(m => ConvertToUnSign(m.Title.ToLower()).Contains(temp)).Take(8).ToList();
            var html = "";            
            foreach (Movie movie in movies)
            {
                dynamic score = 0.0;
                var count = 0;
                foreach (Rate rate in movie.Rates)
                {
                    score += (double)rate.NumericRating;
                    count++;
                }
                if (count != 0)
                {
                    score /= count;
                } else
                {
                    score = "";
                }
                html += $"<div class=\"card\" style=\"width: 195px; margin-top: 10px;margin-left:15px\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}'><img class=\"card-img-top\" style=\"height:10rem;\" src=\"{(string.IsNullOrEmpty(movie.Image) ? "./Img/No_Image_Available.jpg" : movie.Image)}\" alt=\"Card\" /></a>\r\n" +
                    $"<div class=\"card-body\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}' class=\"text-decoration-none\"><h5 class=\"card-title\">{movie.Title}</h5></a>\r\n" +
                    $"<p class=\"card-text\">Năm: {movie.Year}</p>\r\n" +
                    $"<p class=\"card-text\">Loại: {movie.Genre.Description}</p>\r\n" +
                    $"<p class=\"card-text\">Điểm: {score}</p>\r\n" +
                    $"<div class=\"d-flex justify-content-center\">\r\n" +
					((ps != null) ? $"<a href='Home/Detail?id={movie.MovieId}' class=\"btn btn-primary\">Đánh giá</a>\r\n" : $"<a href='Home/Login' class=\"btn btn-primary\">Đánh giá</a>") +
					$"</div>\r\n" +
                    $"</div>\r\n" +
                    $"</div>\n";
            }
            var fullMovies = _db.Movies.Include(m => m.Genre).Include(m => m.Rates).ToList();
            if (genreid != 0)
            {
                fullMovies = fullMovies.Where(m => m.GenreId == genreid).ToList();
            }
            fullMovies = fullMovies.FindAll(m => ConvertToUnSign(m.Title.ToLower()).Contains(temp)).ToList();
            return Json(new { 
                html = html,
                size = movies.Count,
                fullSize = fullMovies.Count
            });
        }

        [HttpGet]
        public JsonResult Filter(int genreid, string searchVal)
        {
			Person ps = null;

			if (HttpContext.Session.GetString("account") != null)
			{
				ps = JsonSerializer.Deserialize<Person>(HttpContext.Session.GetString("account"));
			}
			if (string.IsNullOrEmpty(searchVal))
            {
                searchVal = "";
            }
            var movies = _db.Movies.Include(m => m.Genre).Include(m => m.Rates).Where(m => m.GenreId == genreid).ToList()
                    .FindAll(m => ConvertToUnSign(m.Title.ToLower()).Contains(ConvertToUnSign(searchVal.ToLower()))).Take(8).ToList();
            var html = "";
            foreach (Movie movie in movies)
            {
                dynamic score = 0.0;
                var count = 0;
                foreach (Rate rate in movie.Rates)
                {
                    score += (double)rate.NumericRating;
                    count++;
                }
                if (count != 0)
                {
                    score /= count;
                }
                else
                {
                    score = "";
                }
                html += $"<div class=\"card\" style=\"width: 195px; margin-top: 10px;margin-left:15px\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}'><img class=\"card-img-top\" style=\"height:10rem;\" src=\"{(string.IsNullOrEmpty(movie.Image) ? "./Img/No_Image_Available.jpg" : movie.Image)}\" alt=\"Card\" /></a>\r\n" +
                    $"<div class=\"card-body\">\r\n" +
                    $"<a href='Home/Detail?id={movie.MovieId}' class=\"text-decoration-none\"><h5 class=\"card-title\">{movie.Title}</h5></a>\r\n" +
                    $"<p class=\"card-text\">Năm: {movie.Year}</p>\r\n" +
                    $"<p class=\"card-text\">Loại: {movie.Genre.Description}</p>\r\n" +
                    $"<p class=\"card-text\">Điểm: {score}</p>\r\n" +
                    $"<div class=\"d-flex justify-content-center\">\r\n" +
					((ps != null) ? $"<a href='Home/Detail?id={movie.MovieId}' class=\"btn btn-primary\">Đánh giá</a>\r\n" : $"<a href='Home/Login' class=\"btn btn-primary\">Đánh giá</a>") +
					$"</div>\r\n" +
                    $"</div>\r\n" +
                    $"</div>\n";
            }
            return Json(new
            {
                html = html,
                size = movies.Count,
                fullSize = _db.Movies.Where(m => m.GenreId == genreid).ToList().FindAll(m => ConvertToUnSign(m.Title.ToLower()).Contains(ConvertToUnSign(searchVal.ToLower()))).Count
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
		public IActionResult Comment(Rate r1, string score,string pid, string moveId)
		{
            r1.Time = DateTime.Now;
            r1.MovieId = Convert.ToInt32(moveId);
            r1.PersonId = Convert.ToInt32(pid);
            r1.NumericRating = Convert.ToDouble(score);
            _db.Rates.Add(r1);
            _db.SaveChanges();
            return Redirect($"/Home/Detail/{moveId}");
		}

		[HttpPost]
		public IActionResult Edit(string comment, string score, string pid, string moveId)
		{
            var cmt = _db.Rates.FirstOrDefault(m => m.MovieId == Convert.ToInt32(moveId) && m.PersonId == Convert.ToInt32(pid));
            if(cmt != null)
            {
				cmt.Time = DateTime.Now;
				cmt.MovieId = Convert.ToInt32(moveId);
				cmt.PersonId = Convert.ToInt32(pid);
				cmt.NumericRating = Convert.ToDouble(score);
                cmt.Comment = comment;
				_db.Rates.Update(cmt);
			}
			_db.SaveChanges();
			return Redirect($"/Home/Detail/{moveId}");
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}