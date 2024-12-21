using System.Data.Entity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SOFTWAREPROJECT_2024.Models;
using SOFTWAREPROJECT_2024.Models.ViewModels;

namespace SOFTWAREPROJECT_2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //Burayı düzenlicez çünkü database bilgilerini burada kullanıcaz
        private readonly AppDbContext _context; //database için  yazdık.

        public HomeController(AppDbContext context)
        {
            _context = context; //Constructora attribute atayıp bilgileri kullanıcaz
        }

        // Bu altta bulunanlar sayfaların Iaction Resultları
        //Giriş sayfası
        public IActionResult Index()
        {
            return View();
        }

        //About Page

        public IActionResult About()
        {
            return View();
        }

        //SignIn Page

        //Hatamız aynı sayfada aynı kodu çekmek
        [Route("signin")]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //bu bir güüvenli tokeni oluşturuyor.
        public IActionResult GirişYap(Member model)
        {
            var info = _context.Members.FirstOrDefault(k =>
                k.Name == model.Name && k.MemberPassword == model.MemberPassword
            ); //burada database baktık

            if (info != null)
            {
                ViewBag.Success = "Giriş Başarılı";
                HttpContext.Session.SetString("IsActive", info.MemberId.ToString()); //giriş yaparken info register
                Console.WriteLine(
                    $" {info.MemberId} - {info.Name} -{info.Email} -{info.MemberPassword}"
                ); //deneme amaçlı
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.message = "Kullanıcı adı veya şifre hatalı tekrar deneyiniz";
                ModelState.AddModelError("", "Invalid login attempt."); //hata yeri
                return View("SignIn");
            }
        }

        //Kayıt sayfası için
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registered(Member kadir)
        {
            var search = _context.Members.FirstOrDefault(k =>
                k.Name == kadir.Name || k.Email == kadir.Email || k.Phone == kadir.Phone
            );

            if (search != null)
            {
                ViewBag.message = "Bu e-posta veya username zaten kullanılıyor.";
                return View("SignUp", kadir);
            }

            _context.Members.Add(kadir);
            _context.SaveChanges();
            Console.WriteLine("Kayıt başarıyla Oluşturuldu.");
            return RedirectToAction("SignIn");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        //Kullanıcı burada bilgilerini görcek.
        [HttpGet]
        public IActionResult Profile()
        {
            string id = HttpContext.Session.GetString("IsActive");

            if (string.IsNullOrEmpty(id))
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("SignIn", "Home");
            }
            int userid = int.Parse(id); //burada direk ıd yi integera çevirdim
            var member = _context.Members.FirstOrDefault(y => y.MemberId == userid);
            var borrowings = _context
                .Borrowings.Where(y => y.MemberId == userid)
                .Include(y => y.Book)
                .Select(y => new BorrowingWithBookViewModel
                {
                    BorrowId = y.BorrowId,
                    BookName = y.Book.BookName,
                    Author = y.Book.Author,
                    Genre = y.Book.Genre,
                    Status = y.Status,
                    BorrowDate=y.BorrowDate,//sonradan ekledim tarih bilgilerini
                    ReturnDate=y.ReturnDate,//sonradan ekledim tarih bilgilerini
                })
                .ToList();
            TempData["informationDays"] ="Please return the books on the specified dates so that other users can also receive the books on time. On-time delivery helps keep our library tidy. Thank you!";
            if (borrowings != null)
            {
                ViewBag.Message = "Henüz ödünç alınmış bir kitabınız yok.";
            }

            var profileViewModel = new ProfileViewModels
            {
                Member = member,
                Borrowing = borrowings,
            };
            return View(profileViewModel);
        }
        
        //normalde reservationu sade
        public IActionResult Reservation(string genre,int  page)
        {   
          
            var kadir = _context.Books.Select(b=>b.Genre)
                                      .Distinct()
                                      .ToList();//burada Disticnt kullanarak tekrarlayanları


            // Kitapları filtreledik ama herşeyine göre filtreledik
            var books = _context.Books.ToList();
            if (!string.IsNullOrEmpty(genre))
            {
                
                books = books.Where(b=>b.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase)||
                b.BookName.Contains(genre, StringComparison.OrdinalIgnoreCase)||
                b.Author.Contains(genre, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            if(books.Count==0){
            TempData["notbook"] = "The book you are looking for is currently unavailable. Please contact the library staff for further assistance.";


            }
           
            var allinfo =new AllBookReservation{
                Genre=kadir,
                Books=books,
            };
            
            return View(allinfo);

          
        }

        
        


        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult SearchBar(){
            return View();
        }
        

        [Route("admingiris")] //Bunu yazarak direk /home/ yerine direk yazabiliyoruz
        public IActionResult AdminGiriş()
        {
            return View();
        }

        //Admin panele biz burdan gircez ama burası şifreli yer olcak
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminSıgnIn(Admin model)
        {
            //Admini burada sorguladım varmı db de diye.
            var AdminInfo = _context.Admins.FirstOrDefault(x =>
                x.Name == model.Name && x.AdminPassword == model.AdminPassword
            );

            //daha sonra bi bakak dolumu boşmu bu çar.

            if (AdminInfo != null)
            {
                //burada admın adını sorguladık ve eğer boş değilse onu ekrana getirdik.
                TempData["ADMINNAME"] = AdminInfo.Name;
                TempData["Success"] = "Giriş Başarılı"; // TempData ile başarı mesajı gönderiliyor
                HttpContext.Session.SetString("AdminActive", AdminInfo.AdminId.ToString()); //burada bilgi olarak AdminName tuttum
                Console.WriteLine($"{AdminInfo.AdminId.ToString()}"); //small checking

                return RedirectToAction("AdminPanel", "Home");
            }
            else
            {
                TempData["Error"] = "Kullanıcı adı veya şifre hatalı, tekrar deneyiniz";
                return RedirectToAction("AdminGiriş", "Home");
            }
        }

        //Adminin yönlendiriliceği sayfa

        public IActionResult AdminPanel()
        {
            // var borrowingInfo =_context.Borrowings.ToList();//full bilgileri aldık.

            var informatin = _context
                .Borrowings.Include(b => b.Member)
                .Include(b => b.Book)
                .Select(b => new BorrowingViewModel
                {
                    BorrowId = b.BorrowId,
                    MemberId = b.MemberId,
                    BookId = b.BookId,
                    BorrowDate = b.BorrowDate,
                    ReturnDate = b.ReturnDate,
                    Status = b.Status,

                    // İlişkili tablolardan verileri alıyoruz
                    Name = b.Member.Name, // Üye adı
                    BookName = b.Book.BookName, // Kitap adı
                    Email = b.Member.Email, // Üye emaili
                    Phone = b.Member.Phone, // Üye telefon numarası
                    JoinDate = b.Member.JoinDate, // Üye katılım tarihi
                    MemberPassword = b.Member.MemberPassword, // Üye şifresi
                    Author = b.Book.Author, // Kitap yazarı
                    Genre = b.Book.Genre // Kitap türü
                })
                .ToList();

            var viewModel = new AdminPanelViewModel
            {
                Books = _context.Books.ToList(),
                Members = _context.Members.ToList(),
                BorrowingsViewModel = informatin,
            };
            return View(viewModel);
        }

        //Logout işlemleri için Post sart oldu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogOut()
        {
            //Silme cacheden

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //Delete Function

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int BorrowId)
        {
            var deleteBorrow = _context.Borrowings.FirstOrDefault(y => y.BorrowId == BorrowId); //Eğer formdan gelen Id ile
            if (deleteBorrow != null)
            {

                var bookinfo =_context.Books.FirstOrDefault(x=>x.BookId==deleteBorrow.BookId);
                if(bookinfo != null){

                    bookinfo.BookStatus="Shelf";
                    _context.Books.Update(bookinfo);

                }

                _context.Borrowings.Remove(deleteBorrow); // Eğer boş değilse databaseden sil dedik.
                _context.SaveChanges(); //sonucu kaydettik.

                
            }

            return RedirectToAction("AdminPanel", "Home");
        }

        //Added Function
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Added(BorrowingViewModel model)
        {
            // Üye ID kontrolü
            var memberInfo = _context.Members.Any(y => y.MemberId == model.MemberId);
            if (!memberInfo)
            {
                TempData["HataMesajı"] =
                    "Kullanıcı ID ve isim bilgileri eşleşmiyor. Lütfen bilgileri kontrol ediniz.";
                return RedirectToAction("AdminPanel", "Home");
            }

            // Kitap ID kontrolü
            var bookInfo = _context.Books.FirstOrDefault(y => y.BookId == model.BookId);
            if (bookInfo ==null)
            {
                TempData["HatalıKitapID"] =
                    "Kitap ID ve isim bilgileri eşleşmiyor. Lütfen bilgileri kontrol ediniz.";
                return RedirectToAction("AdminPanel", "Home");
            }
            var existingBorrow = _context.Borrowings
                                .Any(b => b.BookId == model.BookId && b.Status == "Borrowed");
            if (existingBorrow)
            {
                TempData["HataMesajı"] = "This book has already been borrowed!";
                return RedirectToAction("AdminPanel", "Home");
            }

            if(bookInfo.BookStatus == "Borrowed"){
                TempData["Status"] ="This book has already been borrowed!";
                return RedirectToAction("AdminPanel","Home");
            }
    
         
            // Yeni ödünç alma işlemi oluşturuluyor
            var reservation = new Borrowing
            {
                MemberId = model.MemberId,
                BookId = model.BookId,
                BorrowDate = model.BorrowDate,
                ReturnDate = model.ReturnDate,
                Status = model.Status,
                
            };
            //durumunu güncelledim ve devam ettik.
            bookInfo.BookStatus="Borrowed"; //aldığımız kitapın rezervasyon durumh değiştirdik ve update ettik.
            _context.Books.Update(bookInfo);

            _context.Borrowings.Add(reservation); // yeni yağtığımız ürünleri ekledik.
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Yeni rezervasyon başarıyla eklendi!";
            return RedirectToAction("AdminPanel", "Home");
        }

        //Bilerek Update yapmasını engelledim gerek yok bence siler ve ekler Updatelik ne varki
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBorrow(BorrowingViewModel model){
            if(ModelState.IsValid){
                var borrowInfo =_context.Borrowings.FirstOrDefault(y=>y.BorrowId == model.BorrowId);

                if(borrowInfo!= null){

                    borrowInfo.BorrowId = model.BorrowId;
                    borrowInfo.MemberId = model.MemberId;
                    borrowInfo.Member.Name = model.Name;
                    borrowInfo.BookId = model.BookId;
                    borrowInfo.Book.BookName = model.BookName;
                    borrowInfo.BorrowDate = model.BorrowDate;
                    borrowInfo.ReturnDate = model.ReturnDate;
                    borrowInfo.Status = model.Status;

                    _context.SaveChanges();
                    

                }else{
                    TempData["dontupdate"]="The reservation record could not be created.";
                }
            }
            return RedirectToAction("AdminPanel","Home");
        }


        //MemberDelete methodu

        [HttpPost]
        [ValidateAntiForgeryToken] //güvenlik için
        public IActionResult DeleteMember(int MemberId)
        {
            var memberdelete = _context.Members.FirstOrDefault(y => y.MemberId == MemberId);
            if (memberdelete != null)
            {
                _context.Members.Remove(memberdelete);
                _context.SaveChanges(); //değişikleri kaydet.
            }

            return RedirectToAction("AdminPanel", "Home");
        }

        //NewMember EKLE FUNCTİONA

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddedMember(Member model)
        {
            if (ModelState.IsValid)
            {
                _context.Members.Add(model); //kullanıcıı ekle
                _context.SaveChanges();
                return RedirectToAction("AdminPanel", "Home");
            }

            return View(model); //kendini dönersin
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMember(Member model)
        {
            if (ModelState.IsValid)
            {
                var information = _context.Members.FirstOrDefault(y =>
                    y.MemberId == model.MemberId
                ); //eşitlik varmı db de ona baktık

                if (information != null)
                {
                    information.MemberId = model.MemberId;
                    information.Name = model.Name;
                    information.Email = model.Email;
                    information.Phone = model.Phone;
                    information.JoinDate = model.JoinDate;
                    information.MemberPassword = model.MemberPassword;

                    _context.SaveChanges(); //değişklieri kaydettim
                }
            }

            return RedirectToAction("AdminPanel");
        }


        //ProfileUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Member model)
        {
            if (ModelState.IsValid)
            {
                var information = _context.Members.FirstOrDefault(y =>
                    y.MemberId == model.MemberId
                );

                if (information != null)
                {
                    // Sadece gönderilen alanlar güncellenir
                    if (!string.IsNullOrEmpty(model.Name))
                        information.Name = model.Name;

                    if (!string.IsNullOrEmpty(model.Email))
                        information.Email = model.Email;

                    if (!string.IsNullOrEmpty(model.Phone))
                        information.Phone = model.Phone;

                    if (!string.IsNullOrEmpty(model.MemberPassword))
                        information.MemberPassword = model.MemberPassword;

                    _context.SaveChanges(); // Değişiklikleri kaydeder
                }
            }

            return RedirectToAction("Profile", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBook(Book model)
        {
            if (ModelState.IsValid)
            {
                var info = _context.Books.FirstOrDefault(y => y.BookId == model.BookId); //Frontendden gönderdiğimiz IDyi burda db'de varmı diye kontrol ettim
                if (info != null)
                {
                    info.BookId = model.BookId;
                    info.BookName = model.BookName;
                    info.Author = model.Author;
                    info.Genre = model.Genre;
                    _context.SaveChanges(); //değişkleri kaydettim
                }
            }

            return RedirectToAction("AdminPanel", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddedBook(Book model)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(model);
                _context.SaveChanges(); //kayıtları unutma
                return RedirectToAction("AdminPanel", "Home");
            }

            return View(model); // bu da modeli dönder birşey yapmaz.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBook(int BookId)
        {
            var information = _context.Books.FirstOrDefault(y => y.BookId == BookId);

            if (information != null)
            {
                _context.Books.Remove(information);
                _context.SaveChanges(); //kayıt yapıyosun
            }

            return RedirectToAction("AdminPanel", "Home");
        }

        public IActionResult FAQ()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}