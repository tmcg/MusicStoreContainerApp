using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicStore.Data;

namespace MusicStore.Pages
{
   public class IndexModel : PageModel
   {
      private readonly ILogger<IndexModel> _logger;

      public List<Album> Albums { get => _AlbumList; }

      public IndexModel(ILogger<IndexModel> logger)
      {
         _logger = logger;
      }

      public void OnGet()
      {

      }

      private static List<Album> _AlbumList = new List<Album>
      {
         new Album {
            Name = "Bob Dylan - The Freewheelin'",
            ImageName = "cover-bob-dylan-the-freewheelin.jpg",
            ReleaseYear = 1963,
         },
         new Album {
            Name = "Fleetwood Mac - Rumors",
            ImageName = "cover-fleetwood-mac-rumors.jpg",
            ReleaseYear = 1977,
         },
         new Album {
            Name = "Kraftwerk - The Man Machine",
            ImageName = "cover-kraftwerk-the-man-machine.jpg",
            ReleaseYear = 1978,
         },
         new Album {
            Name = "Metallica - Master of Puppets",
            ImageName = "cover-metallica-master-of-puppets.jpg",
            ReleaseYear = 1986,
         },
         new Album {
            Name = "Nirvana - Nevermind",
            ImageName = "cover-nirvana-nevermind.jpg",
            ReleaseYear = 1991,
         },
         new Album {
            Name = "Pink Floyd - Dark Side of the Moon",
            ImageName = "cover-pink-floyd-dark-side-of-the-moon.jpg",
            ReleaseYear = 1973,
         },
         new Album {
            Name = "Smashing Pumpkins - Mellon Collie and the Infinite Sadness",
            ImageName = "cover-smashing-pumpkins-mellon-collie.jpg",
            ReleaseYear = 1995,
         },
         new Album {
            Name = "The Beatles - Sgt. Pepper's Lonely Hearts Club Band",
            ImageName = "cover-the-beatles-sgt-peppers.jpg",
            ReleaseYear = 1967,
         },
         new Album {
            Name = "The Eagles - Hotel California",
            ImageName = "cover-the-eagles-hotel-california.jpg",
            ReleaseYear = 1976,
         },
      };
   }
}