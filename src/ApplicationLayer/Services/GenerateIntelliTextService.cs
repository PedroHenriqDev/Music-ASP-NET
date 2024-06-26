﻿using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using DomainLayer.Interfaces;
using System.Text;

namespace ApplicationLayer.Services;

public class GenerateIntelliTextService : IGenerateIntelliTextService
{
    private readonly ISearchService _searchService;

    public GenerateIntelliTextService(ISearchService searchService)
    {
        _searchService = searchService;
    }

    private async Task<string[]> GenerateDescriptionAsync<T>(T user, string action)
        where T : class, IUser<T>
    {
        var genres = await _searchService.FindUserGenresAsync<T>(user.Id);
        StringBuilder allGenres = new StringBuilder();
        int numberOfGenres = genres.Count();
        string[] descriptions = new string[numberOfGenres + 1];
        int i = 0;

        foreach (var genre in genres)
        {
            descriptions[i] = $"I {action} {genre.Name} music style.";
            allGenres.Append(genre.Name);
            if (i < numberOfGenres - 1)
                allGenres.Append(", ");
            i++;
        }
        descriptions[numberOfGenres] = $"I {action} {allGenres} musical styles.";
        return descriptions;
    }

    public async Task<string[]> GenerateListenerDescriptionAsync(Listener listener)
    {
        return await GenerateDescriptionAsync(listener, "appreciate");
    }

    public async Task<string[]> GenerateArtistDescriptionAsync(Artist artist)
    {
        return await GenerateDescriptionAsync(artist, "produce");
    }
}
