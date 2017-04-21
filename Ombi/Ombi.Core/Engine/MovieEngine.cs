﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine
{
    public class MovieEngine : BaseMediaEngine, IMovieEngine
    {

        public MovieEngine(IUserIdentityManager identity, IRequestService service, IMovieDbApi movApi, IMapper mapper, ISettingsService<PlexSettings> plexSettings, ISettingsService<EmbySettings> embySettings) 
            : base(identity, service)
        {
            MovieApi = movApi;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
        }

        private IMovieDbApi MovieApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }

        public async Task<IEnumerable<SearchMovieViewModel>> LookupImdbInformation(IEnumerable<SearchMovieViewModel> movies)
        {
            var retVal = new List<SearchMovieViewModel>();
            Dictionary<int, RequestModel> dbMovies = await GetRequests(RequestType.Movie);
            foreach (var m in movies)
            {
                var movieInfo = await MovieApi.GetMovieInformationWithVideo(m.Id);
                var viewMovie = Mapper.Map<SearchMovieViewModel>(movieInfo);

                retVal.Add(await ProcessSingleMovie(viewMovie, dbMovies));
            }
            return retVal;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> ProcessMovieSearch(string search)
        {
            var result = await MovieApi.SearchMovie(search);
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> PopularMovies()
        {
            var result = await MovieApi.PopularMovies();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> TopRatedMovies()
        {
            var result = await MovieApi.TopRated();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }

        public async Task<IEnumerable<SearchMovieViewModel>> UpcomingMovies()
        {
            var result = await MovieApi.Upcoming();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }
        public async Task<IEnumerable<SearchMovieViewModel>> NowPlayingMovies()
        {
            var result = await MovieApi.NowPlaying();
            if (result != null)
            {
                return await TransformMovieResultsToResponse(result);
            }
            return null;
        }


        private async Task<List<SearchMovieViewModel>> TransformMovieResultsToResponse(IEnumerable<MovieSearchResult> movies)
        {
            var viewMovies = new List<SearchMovieViewModel>();
            Dictionary<int, RequestModel> dbMovies = await GetRequests(RequestType.Movie);
            foreach (var movie in movies)
            {

                viewMovies.Add(await ProcessSingleMovie(movie, dbMovies));

            }
            return viewMovies;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(SearchMovieViewModel viewMovie,
            Dictionary<int, RequestModel> existingRequests)
        {

            var plexSettings = await PlexSettings.GetSettingsAsync();
            var embySettings = await EmbySettings.GetSettingsAsync();
            if (plexSettings.Enable)
            {
                //        var content = PlexContentRepository.GetAll();
                //        var plexMovies = PlexChecker.GetPlexMovies(content);

                //        var plexMovie = PlexChecker.GetMovie(plexMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(),
                //            viewMovie.ImdbId);
                //        if (plexMovie != null)
                //        {
                //            viewMovie.Available = true;
                //            viewMovie.PlexUrl = plexMovie.Url;
                //        }
            }
            if (embySettings.Enable)
            {
                //        var embyContent = EmbyContentRepository.GetAll();
                //        var embyMovies = EmbyChecker.GetEmbyMovies(embyContent);

                //        var embyMovie = EmbyChecker.GetMovie(embyMovies.ToArray(), movie.Title,
                //            movie.ReleaseDate?.Year.ToString(), viewMovie.ImdbId);
                //        if (embyMovie != null)
                //        {
                //            viewMovie.Available = true;
                //        }
            }

            if (existingRequests.ContainsKey(viewMovie.Id)) // Do we already have a request for this?
            {
                var requestedMovie = existingRequests[viewMovie.Id];

                viewMovie.Requested = true;
                viewMovie.Approved = requestedMovie.Approved;
                viewMovie.Available = requestedMovie.Available;
            }


            return viewMovie;
        }

        private async Task<SearchMovieViewModel> ProcessSingleMovie(MovieSearchResult movie, Dictionary<int, RequestModel> existingRequests)
        {
            var viewMovie = Mapper.Map<SearchMovieViewModel>(movie);
            return await ProcessSingleMovie(viewMovie, existingRequests);
        }
    }
}
