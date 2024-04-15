﻿using DataAccessLayer.Sql;
using DomainLayer.Interfaces;
using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Http;
using DomainLayer.Entities;

namespace ApplicationLayer.Services
{
    public class SearchService
    {
        private readonly ConnectionDb _connectionDb;
        private readonly IHttpContextAccessor _httpAcessor;

        public SearchService(ConnectionDb connectionDb, IHttpContextAccessor httpAcessor)
        {
            _connectionDb = connectionDb;
            _httpAcessor = httpAcessor;
        }

        public T FindUserByName<T>(string name) where T : class, IEntityWithName<T>
        {
            return _connectionDb.GetUserByName<T>(name);
        }

        public async Task<T> FindUserByNameAsync<T>(string name)
            where T : class, IEntityWithName<T>
        {
            return await _connectionDb.GetUserByNameAsync<T>(name);
        }

        public async Task<T> FindEntityByIdAsync<T>(string id)
            where T : class, IEntity
        {
            return await _connectionDb.GetEntityByIdAsync<T>(id);
        }

        public async Task<T> FindCurrentUserAsync<T>()
            where T : IUser<T>
        {
            return await _connectionDb.GetUserByNameAsync<T>(_httpAcessor.HttpContext.User.Identity.Name);
        }

        public async Task<IEnumerable<Genre>> FindUserGenresAsync<T>(T user)
            where T : class, IUser<T>
        {
            var userGenres = await _connectionDb.GetUserGenresAsync<T>(user.Id);
            return await _connectionDb.GetEntitiesByIdsAsync<Genre>(userGenres.Select(g => g.GenreId).ToList());
        }

        public async IAsyncEnumerable<T> FindAllEntitiesAsyncStream<T>()
            where T : class, IEntity
        {
            var entities = await _connectionDb.GetAllEntitiesAsync<T>();
            foreach (var entity in entities)
            {
                yield return entity;
            }
        }

        public async Task<IEnumerable<T>> FindAllEntitiesAsync<T>()
            where T : class, IEntity
        {
            return await _connectionDb.GetAllEntitiesAsync<T>();
        }

        public async Task<T> FindEntityByEmailAsync<T>(string email)
            where T : IEntityWithEmail<T>
        {
            if (email == null) throw new SearchException("Email reference null");

            return await _connectionDb.GetEntityByEmailAsync<T>(email);
        }

        public async Task<T> FindUserByCredentialsAsync<T>(string email, string password)
            where T : IUser<T>
        {
            if (password == null || email == null) throw new SearchException("Password or email were used as a null reference");

            return await _connectionDb.GetEntityByCredentialsAsync<T>(email, password);
        }

        public async Task<IEnumerable<Music>> FindMusicsByFkIdsAsync<T>(IEnumerable<string> fkIds) 
            where T : class, IEntity
        {
            return await _connectionDb.GetMusicsByFkIdsAsync<T>(fkIds);
        }

        public async Task<IEnumerable<Music>> FindMusicByFkIdAsync<T>(string fkId) 
            where T : class, IEntity
        {
            return await _connectionDb.GetMusicsByFkIdAsync<T>(fkId);
        }

        public async Task<IEnumerable<T>> FindEntityByFkIdAsync<T, TR>(string artistId)
            where T : class, IEntity where TR : class, IEntity
        {
            return await _connectionDb.GetEntitiesByForeignKeyAsync<T, TR>(artistId);
        }
    }
}