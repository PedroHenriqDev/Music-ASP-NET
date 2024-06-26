﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DomainLayer.Interfaces;
using UtilitiesLayer.Helpers;
using System.Security.Claims;
using ApplicationLayer.Interfaces;

namespace ApplicationLayer.Services;

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly ILogger<UserAuthenticationService> _logger;
    private readonly IPictureService _pictureService;
    private readonly IHttpContextAccessor _httpAcessor;
    public UserAuthenticationService(
        ILogger<UserAuthenticationService> logger,
        IPictureService pictureService,
        IHttpContextAccessor httpAccessor)
    {
        _logger = logger;
        _pictureService = pictureService; 
        _httpAcessor = httpAccessor;
    }

    public async Task SignInUserAsync<T>(T user) 
        where T : IUser<T>
    {
        if (user is null)
        {
            _logger.LogError("Error in authentication");
            throw new ArgumentNullException("Reference user null!");
        }

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(CookieKeys.UserIdCookieKey, user.Id)
        };

        if (!(user.PictureProfile is null))
        {
            string pictureUrl = await _pictureService.SavePictureProfileAsync(user.PictureProfile, _httpAcessor.HttpContext.Request.PathBase);
            claims.Add(new Claim(CookieKeys.PictureProfileCookieKey, pictureUrl));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties();

        await _httpAcessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
    }

    public void SetCookie<T>(string key, T value)
    {
        var options = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        var serializedValue = JsonSerializationHelper.SerializeObject(value);
        _httpAcessor.HttpContext.Response.Cookies.Append(key, serializedValue, options);
    }

    public async Task SignOutUserAsync()
    {
        await _httpAcessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
