﻿using System;
namespace Tasin.Website.DAL.Services.TokenServices
{
    public class TokenSettings
    {
        //public const string NAME = "TokenSettings";

        public string SecretKey { get; set; }
        public string ExpireTimeInMinutes { get; set; }
    }
}
