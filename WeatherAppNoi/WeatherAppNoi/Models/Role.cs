﻿using Microsoft.AspNetCore.Identity;

namespace WeatherAppNoi.Models
{
    public class Role:IdentityRole<int>
    {
        public List<UserRole> UserRoles { get; set; } = [];
    }
}
