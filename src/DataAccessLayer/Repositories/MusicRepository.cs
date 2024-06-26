﻿using Dapper;
using DataAccessLayer.Mappers;
using DataAccessLayer.Sanitization;
using DomainLayer.Entities;
using DomainLayer.Interfaces;
using Npgsql;

namespace DataAccessLayer.Repositories;

public class MusicRepository : IMusicRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly DataMapper _mapper;

    public MusicRepository(NpgsqlConnection connection,
                           DataMapper mapper)
    {
        _connection = connection;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Music>> GetMusicsByIdsAsync(List<string> ids)
    {
        string sqlQuery = $@"
                                 SELECT
                                    m.Id, 
                                    m.Name,
                                    m.ArtistId,
                                    m.GenreId,
                                    m.Date,
                                    m.DateCreation,
                                    m.Duration,
                                    a.Id AS ArtistId,
                                    a.Name AS Name
                                 FROM 
                                    Musics m
                                 INNER JOIN 
                                    Artists a ON m.ArtistId = a.Id
                                 WHERE
                                    m.Id = ANY(@ids))";

        var result = await _connection.QueryAsync<Music, Artist, Music>(
            sqlQuery, (music, artist) =>
            {
                return _mapper.MapMusicArtist(music, artist);
            },
            splitOn: "ArtistId",
            param: new { ids = FieldSanitization.JoinIds(ids) });

        return result;
    }

    public async Task<IEnumerable<Music>> GetMusicsByQueryAsync(string query)
    {
        string sqlQuery = $@"
                                 SELECT
                                    m.Id,
                                    m.Name,
                                    m.ArtistId,
                                    m.GenreId,
                                    m.Date,
                                    m.DateCreation,
                                    m.Duration,
                                    a.Id AS ArtistId,
                                    a.Name AS Name
                                 FROM
                                    Musics m
                                INNER JOIN
                                    Artists a ON m.ArtistId = a.Id
                                WHERE
                                    LOWER(m.Name) LIKE LOWER('%' || @query || '%')
                                    OR LOWER(a.Name) LIKE LOWER('%' || @query || '%')
                                    OR LOWER(a.Description) LIKE LOWER('%' || @query || '%')";

        var result = await _connection.QueryAsync<Music, Artist, Music>(
            sqlQuery,
            (music, artist) =>
            {
                return _mapper.MapMusicArtist(music, artist);
            },
            splitOn: "ArtistId",
            param: new { query });

        return result;
    }

    public async Task<IEnumerable<Music>> GetMusicsByFkIdAsync<T>(string fkId)
        where T : class, IEntity
    {
        string fkField = FieldSanitization.ForeignKeyName<T>();
      
        string sqlQuery = $@"
                                   SELECT
                                       m.Id,
                                       m.Name,
                                       m.ArtistId,
                                       m.GenreId,
                                       m.Date,
                                       m.DateCreation,
                                       m.Duration,
                                       a.Id AS ArtistId,
                                       a.Name AS Name
                                    FROM
                                       Musics m
                                    INNER JOIN 
                                       Artists a ON m.ArtistId = a.Id
                                    WHERE 
                                       m.{fkField} = @fkId";

        var result = await _connection.QueryAsync<Music, Artist, Music>
            (sqlQuery,
            (music, artist) =>
            {
                return _mapper.MapMusicArtist(music, artist);
            },
            splitOn: "ArtistId",
            param: new { fkId });

        return result;
    }

    public async Task<IEnumerable<Music>> GetMusicsByFkIdsAsync<T>(IEnumerable<string> fkIds)
       where T : class, IEntity
    {
        string fkField = FieldSanitization.ForeignKeyName<T>();
      
        string sqlQuery = @$"
                                   SELECT
                                       m.Id,
                                       m.Name,
                                       m.ArtistId,
                                       m.GenreId,
                                       m.Date,
                                       m.DateCreation,
                                       m.Duration,
                                       a.Id AS ArtistId,
                                       a.Name AS Name
                                   FROM
                                       Musics m
                                   INNER JOIN
                                       Artists a ON m.ArtistId = a.Id
                                   WHERE
                                      m.{fkField} = ANY(@fkIds)";

        var result = await _connection.QueryAsync<Music, Artist, Music>(
                sqlQuery,
                (music, artist) =>
                {
                    return _mapper.MapMusicArtist(music, artist);
                },
                splitOn: "ArtistId",
                param: new { fkIds });

        return result;
    }

    public async Task<IEnumerable<FavoriteMusic>> GetBasicFavoriteMusicsByListenerIdAsync(string listenerId)
    {
        string sqlQuery = "SELECT * FROM FavoriteMusics WHERE ListenerId = @listenerId";
     
        return await _connection.QueryAsync<FavoriteMusic>(sqlQuery, new
        {
            listenerId = listenerId
        });
    }

    public async Task<IEnumerable<Music>> GetDetailedFavoriteMusicsByListenerIdAsync(string listenerId)
    {
        string sqlQuery = @$"
                                SELECT
                                  m.Id,
                                  m.Name,
                                  m.GenreId,
                                  m.Duration,
                                  m.ArtistId,
                                  a.Name,
                                  a.Description,
                                  a.PictureProfile,
                                  f.ListenerId,
                                  f.MusicId
                                FROM 
                                  FavoriteMusics f
                                INNER JOIN
                                  Musics m ON f.MusicId = m.Id
                                INNER JOIN
                                  Artists a ON a.Id = m.ArtistId                                        
                                WHERE
                                  f.ListenerId = @listenerId";

        return await _connection.QueryAsync<Music, Artist, Music>(sqlQuery,
             (music, artist) =>
             {
                 return _mapper.MapMusicArtist(music, artist);
             },
             splitOn: "ArtistId",
             param: new { listenerId = listenerId });
    }

    public async Task<Music> GetDetailedMusicByIdAsync(string musicId)
    {
        string sqlQuery = @"
                                SELECT 
                                   m.Id,
                                   m.Name,
                                   m.ArtistId,
                                   m.GenreId,
                                   m.Duration,
                                   m.Date,
                                   m.DateCreation,
                                   a.Id,
                                   a.Name,
                                   a.Description,
                                   a.DateCreation,
                                   a.BirthDate,
                                   a.PictureProfile,
                                   mv.MusicId,
                                   g.Id,
                                   g.Name,
                                   g.Description
                                FROM 
                                   Musics m
                                INNER JOIN
                                    Artists a ON a.Id = m.ArtistId
                                LEFT JOIN
                                    MusicViews mv ON mv.MusicId = m.Id
                                INNER JOIN
                                    Genres g ON g.Id = m.GenreId
                                WHERE
                                    m.Id = @musicId";

        var musicDictionary = new Dictionary<string, Music>();
     
        var result = await _connection.QueryAsync<Music, Artist, MusicView, Genre, Music>(
            sqlQuery,
            (music, artist, musicView, genre) =>
            {
                return _mapper.MapMusicViews(_mapper.MapMusicArtist(music, artist), musicDictionary, musicView, genre);
            },
            splitOn: "Id,Id,MusicId,Id",
            param: new { musicId });

        return musicDictionary.Values.FirstOrDefault();
    }

    public async Task RecordMusicAsync(Music music)
    {
        string sqlQuery = @"INSERT INTO Musics (Id, Name, ArtistId, GenreId, Date, DateCreation, Duration) 
                                    VALUES(@id, @name, @artistId, @genreId, @date, @dateCreation, @duration)";

        await _connection.QueryAsync(sqlQuery, new
        {
            id = music.Id,
            name = music.Name,
            artistId = music.ArtistId,
            genreId = music.GenreId,
            date = music.Date,
            dateCreation = music.DateCreation,
            duration = music.Duration
        });
    }

    public async Task RecordMusicViewAsync(MusicView musicView)
    {
        string sqlQuery = @"INSERT INTO MusicViews (Id, MusicId, ListenerId, CreatedAt) 
                                    VALUES (@id, @musicId, @listenerId, @createdAt)";

        await _connection.QueryAsync(sqlQuery, new
        {
            id = musicView.Id,
            musicId = musicView.MusicId,
            listenerId = musicView.ListenerId,
            createdAt = musicView.CreatedAt
        });
    }

    public async Task RecordFavoriteMusicAsync(FavoriteMusic favoriteMusic)
    {
        string sqlQuery = @"INSERT INTO FavoriteMusics (Id, ListenerId, MusicId)
                                    VALUES (@id, @listenerId, @musicId)";

        await _connection.QueryAsync(sqlQuery, new
        {
            id = favoriteMusic.Id,
            listenerId = favoriteMusic.ListenerId,
            musicId = favoriteMusic.MusicId
        });
    }

    public async Task RemoveFavoriteMusicAsync(FavoriteMusic favoriteMusic)
    {
        string sqlQuery = $"DELETE FROM FavoriteMusics WHERE MusicId = @musicId AND ListenerId = @listenerId";
       
        await _connection.QueryAsync(sqlQuery, new
        {
            musicId = favoriteMusic.MusicId,
            listenerId = favoriteMusic.ListenerId
        });
    }
}