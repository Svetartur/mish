using System;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Models.Home.Models
{
    public class HomeModelsFormModel
    {
        [FromForm(Name = "user-login")]
        public String UserLogin { get; set; } = null!;

        [FromForm(Name = "user-password")]
        public String UserPassword { get; set; } = null!;

        [FromForm(Name = "subscribe")]
        public bool Subscribe { get; set; }

        [FromForm(Name = "gender")]
        public String Gender { get; set; } = null!;

        [FromForm(Name = "birth-date")]
        public DateTime BirthDate { get; set; }

        [FromForm(Name = "favorite-color")]
        public String FavoriteColor { get; set; } = null!;

        [FromForm(Name = "score")]
        public int Score { get; set; }
    }
}
