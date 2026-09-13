using ASP_P42.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public IEnumerable<Data.Entities.ProductGroup> GetAllGroups()
        {
            return _dataContext.ProductGroups.Where(g => g.IsHidden == 0);   
        }

        [HttpPost]
        public bool CreateNewGroup(Data.Entities.ProductGroup group)
        {
            return true;
        }
    }
}
