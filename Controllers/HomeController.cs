using System.Diagnostics;
using ASP_P42.Models;
using ASP_P42.Models.Home.Models;
using ASP_P42.Services.Hash;
using ASP_P42.Services.Kdf;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Controllers
{
    public class HomeController(
        IHashService hashService,
        IKdfService kdfService
        ) : Controller
    {
        private readonly IHashService _hashService = hashService;
        private readonly IKdfService _kdfService = kdfService;
        

        public IActionResult IoC()
        {
            String digest = _kdfService.Dk(
                "96DCBBBA", 
                "96DCBBBA-9AEE-44A2-8835-72DFE4E1A710");
            
            ViewBag.Hash = _hashService.GetHashCode();
            ViewData["digest"] = digest;
            return View();
        }

        public IActionResult Models(String? id)
        {
            HomeModelsViewModel viewModel = new()
            {
                PageTitle = "Моделі в ASP",
                Intro = "Модель (у MVC) - архітектурна частина проєкту, яка відповідає за \r\n    взаємодію з даними.\r\n    Модель (в ASP) - клас (об'єкт), призначений для передачі даних \r\n    (DTO - Data Transfer Object, Entity)",
                ClassificationHeader = "Розрізняють декілька типів моделей за призначенням:",
                ClassificationList = [
                    "Модель представлення (ViewModel або PageModel) - дані, з яких будується сторінка (або її частина - представлення)",
                    "Модель форми (FormModel) - дані, що заповнюються користувачем на сторінці і передаються на обробку.",
                    "Модель даних (DTO - Data Transfer Object, Entity) - дані, що  зберігаються на постійній основі, частіше за все у БД. ",
                ],
                ExampleHeader = "Наприклад, для моделі \"користувач\":",
                ExampleList = [
                    "Модель форми (реєстрація) - логін, пароль, повтор пароля, ...",
                    "Модель даних (у БД) - логін, DK(хеш паролю), сіль, ..., дата створення",
                    "Модель представлення (профіль або кабінет) - логін, ..., дата створення \r\n        (паролів немає)",
                ],
            };

            return id == "json" ? Json(viewModel) : View(viewModel);
        }

        [HttpPost]
        public IActionResult ModelsForm(HomeModelsFormModel formModel)
        {
            return View(formModel);
        }

        public IActionResult Razor()
        {
            return View();
        }

        public IActionResult DbContext()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}