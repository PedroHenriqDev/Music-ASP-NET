﻿using MusicWeave.Datas;
using MusicWeave.Models.AbstractClasses;
using MusicWeave.Models.Interfaces;

namespace MusicWeave.Models.Services
{
    public class VerifyService
    {
        private readonly ConnectionDb _connectionDb;

        public VerifyService(ConnectionDb connectionDb)
        {
            _connectionDb = connectionDb;
        }

        public async Task<bool> HasNameInDbAsync<T>(string name) 
            where T : class, IEntityWithName<T>
        {
            if (await _connectionDb.GetEntityByNameAsync<T>(name) != null) 
            {
                return true;
            }
            return false;
        }

        public async Task<bool> HasEmailInDbAsync<T>(string email) 
            where T : class, IEntityWithEmail<T> 
        {
            if(await _connectionDb.GetUserByEmailAsync<T>(email) != null) 
            {
                return true;
            }
            return false;
        }
    }
}
