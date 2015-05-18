﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Framework.Caching.Memory;
using MusicStore.Models;

namespace MusicStore.Components
{
    [ViewComponent(Name = "Announcement")]
    public class AnnouncementComponent : ViewComponent
    {
        [Activate]
        public MusicStoreContext DbContext
        {
            get;
            set;
        }

        [Activate]
        public IMemoryCache Cache
        {
            get;
            set;
        }

        [Activate]
        public ISystemClock Clock
        {
            get;
            set;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cacheKey = "latestAlbum";
            Album latestAlbum;
            if(!Cache.TryGetValue(cacheKey, out latestAlbum))
            {
                latestAlbum = await GetLatestAlbum();

                if (latestAlbum != null)
                {
                    Cache.Set(
                        cacheKey,
                        latestAlbum,
                        new CacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                }
            }

            return View(latestAlbum);
        }

        private async Task<Album> GetLatestAlbum()
        {
            var latestAlbum = await DbContext.Albums
                .OrderByDescending(a => a.Created)
                .Where(a => (a.Created - Clock.UtcNow).TotalDays <= 2)
                .FirstOrDefaultAsync();

            return latestAlbum;
        }
    }
}