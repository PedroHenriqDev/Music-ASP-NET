﻿using ApplicationLayer.Factories;
using ApplicationLayer.ViewModels;
using DataAccessLayer.Sql;
using DomainLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Exceptions;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services
{
    public class RecordService
    {
        private readonly ILogger<RecordService> _logger;
        private readonly ConnectionDb _connectionDb;
        private readonly VerifyService _verifyService;
        private readonly EncryptService _encryptService;
        private readonly ModelFactory _modelFactory;
        private readonly CloudStorageService _storageService;

        public RecordService(
            ILogger<RecordService> logger,
            ConnectionDb connectionDb,
            VerifyService verifyService,
            EncryptService encryptService,
            ModelFactory modelFactory, 
            CloudStorageService storageService)
        {
            _logger = logger;
            _connectionDb = connectionDb;
            _verifyService = verifyService;
            _encryptService = encryptService;
            _modelFactory = modelFactory;
            _storageService = storageService;
        }

        public async Task<EntityQuery<T>> CreateUserAsync<T>(RegisterUserViewModel userVM)
            where T : class, IUser<T>, new()
        {
            string userType = typeof(T).Name;
            T user = _modelFactory.FacUser<T>(Guid.NewGuid().ToString(), userVM.Name, userVM.Email, _encryptService.EncryptPasswordSHA512(userVM.Password), userVM.PhoneNumber, userVM.BirthDate, DateTime.Now);
            try
            {
                await _connectionDb.RecordUserAndUserGenresAsync(user, _modelFactory.FacUserGenres<T>(user.Id, userVM.SelectedGenreIds));
                return new EntityQuery<T>(true, $"{userType} created successfully", user, DateTime.Now);
            }
            catch (RecordAssociationException ex)
            {
                _logger.LogError("The error occurred when creating the object in the associated table, Genre and User");
                throw new RecordException<EntityQuery<T>>($"The error occurred when creating the object associated with the genre, contact the developer, error in sql: {ex.Message}", new EntityQuery<T>(false, $"Unable to create a {userType}", user, DateTime.Now));
            }
            catch (Exception ex)
            {
                _logger.LogError("Brutal error in method CreateUserAsync");
                throw new RecordException<EntityQuery<T>>($"This error occurred while registration was happening, {ex.Message}", new EntityQuery<T>(false, "Unable to create a artist", user, DateTime.Now));
            }
        }

        public async Task<EntityQuery<Music>> CreateMusicAsync(AddMusicViewModel musicVM)
        {
            string id = Guid.NewGuid().ToString();
            var music = await _modelFactory.FacMusicAsync(musicVM, id);
            try 
            {
                await _connectionDb.RecordMusicAsync(music);
                await _storageService.UploadMusicAsync(await _modelFactory.FacMusicDataAsync(musicVM, music.Id));
                return new EntityQuery<Music>(true, "Create music successfully", music, DateTime.Now);
            }
            catch(Exception ex) 
            {
                if(ex is MusicException) 
                {
                    await _connectionDb.DeleteEntityByIdAsync<Music>(id);
                }
                return new EntityQuery<Music>(false, $"Unable to create song, because this error: {ex.Message}", music, DateTime.Now);
            }
        }
    }
}