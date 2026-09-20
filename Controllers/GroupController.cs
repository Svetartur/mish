using ASP_P42.Data;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Controllers
{
    public class GroupController(DataAccessor dataAccessor) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            var groups = _dataAccessor.GetAllProductGroups();
            return View(groups);
        }
    }
}
