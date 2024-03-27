﻿using Datas.Sql;
using Microsoft.Extensions.Logging;
using Exceptions;
using Models.ConcreteClasses;
using ViewModels;

namespace Services
{
    public class RegisterUserService
    {
        private readonly ILogger<RegisterUserService> _logger;
        private readonly ConnectionDb _connectionDb;
        private readonly VerifyService _verifyService;
        private readonly EncryptService _encryptService;

        public RegisterUserService(
            ILogger<RegisterUserService> logger,
            ConnectionDb connectionDb,
            VerifyService verifyService,
            EncryptService encryptService)
        {
            _logger = logger;
            _connectionDb = connectionDb;
            _verifyService = verifyService;
            _encryptService = encryptService;
        }

        private int RamdomId()
        {
            Random random = new Random();
            return random.Next();
        }

        public async Task CreateListenerAsync(RegisterListenerViewModel listenerVM)
        {
            Listener listener = new Listener(
                Guid.NewGuid().ToString(),
                listenerVM.Name,
                _encryptService.EncryptPasswordSHA512(listenerVM.Password),
                listenerVM.Email,
                listenerVM.PhoneNumber,
                listenerVM.BirthDate,
                DateTime.Now);

            if (await _verifyService.HasNameInDbAsync<Listener>(listener.Name) || await _verifyService.HasNameInDbAsync<Artist>(listener.Name))
            {
                _logger.LogInformation("User creation attempt failed because the same name already exists in the database");
                throw new RegisterException("This name exist");
            }

            if (await _verifyService.HasEmailInDbAsync<Listener>(listener.Email) || await _verifyService.HasEmailInDbAsync<Artist>(listener.Email))
            {
                _logger.LogInformation("User creation attempt failed because the same email already exists in the database");
                throw new RegisterException("This email exist");
            }

            await _connectionDb.RecordListenerAsync(listener);
        }

        public async Task CreateArtistAsync(RegisterArtistViewModel artistVM)
        {
            Artist artist = new Artist(
                Guid.NewGuid().ToString(),
                artistVM.Name,
                _encryptService.EncryptPasswordSHA512(artistVM.Password),
                artistVM.Email,
                artistVM.PhoneNumber,
                artistVM.BirthDate,
                DateTime.Now);

            if (await _verifyService.HasNameInDbAsync<Listener>(artist.Name) || await _verifyService.HasNameInDbAsync<Artist>(artist.Name))
            {
                _logger.LogInformation("User creation attempt failed because the same name already exists in the database");
                throw new RegisterException("This name exist");
            }

            if (await _verifyService.HasEmailInDbAsync<Artist>(artist.Email) || await _verifyService.HasEmailInDbAsync<Listener>(artist.Email))
            {
                _logger.LogInformation("User creation attempt failed because the same email already exists in the database");
                throw new RegisterException("Existing email.");
            }

            await _connectionDb.RecordArtistAsync(artist);
        }
    }
}