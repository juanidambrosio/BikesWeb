using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BikeSharing.Users.Seed.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BikeSharing.Users.Seed
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static readonly ICollection<ApplicationUser> Users = new ApplicationUser[]
        {
            new ApplicationUser() { Name="Scott Guthrie", UserName =  "Scottgu", PasswordHash = "Bikes360", ProfileImage = "http://images.forbes.com/media/2016/06/27/0627_cloud-wars-scott-guthrie_1200x675.jpg"},
            new ApplicationUser() { Name="Chris Dias", UserName =  "Chrisd", PasswordHash = "Bikes360", ProfileImage = "http://www.omnisharp.net/images/team/chrisdias.png"},
            new ApplicationUser() { Name="James Montemagno", UserName =  "Jamesm", PasswordHash = "Bikes360", ProfileImage = "http://uploads.speakerrate.com/speakers/130761/avatars/tile/1431733948_James300.jpg"},
            new ApplicationUser() { Name="Beth Massi", UserName =  "Bethm", PasswordHash = "Bikes360", ProfileImage = "http://4.bp.blogspot.com/_u-MR_4uAYxI/TT9tl1MXz8I/AAAAAAAAAEg/vpbg1nWK5yw/s1600/BethMassi.jpg"},
            new ApplicationUser() { Name="Donovan Brown", UserName =  "Donovanb", PasswordHash = "Bikes360", ProfileImage = "https://wintellectnow.blob.core.windows.net/public/Donovan_Brown.jpg"},
            new ApplicationUser() { Name="Lara Rubbelke", UserName =  "LaraR", PasswordHash = "Bikes360", ProfileImage = "http://media.gettyimages.com/photos/lara-rubbelke-principal-software-engineer-at-microsoft-corp-speaks-a-picture-id471533346"},
            new ApplicationUser() { Name="Kasey Uhlenhuth", UserName =  "Kaseyu", PasswordHash = "Bikes360", ProfileImage = "http://www.coins-global.com/storage/images-processed/w-547_h-310_m-crop__Kasey_252px.jpg"},
            new ApplicationUser() { Name="Scott Hanselman", UserName =  "Scottha", PasswordHash = "Bikes360", ProfileImage = "http://d13pix9kaak6wt.cloudfront.net/avatar/shanselman_1295083527_55.jpg"},
            new ApplicationUser() { Name="Miguel de Icaza", UserName =  "Migueld", PasswordHash = "Bikes360", ProfileImage = "http://www.zdnet.com/i/story/60/01/025963/miguel_de_icaza.png"},
            new ApplicationUser() { Name="Maria Naggaga", UserName =  "Mariam", PasswordHash = "Bikes360", ProfileImage = "http://www.codingdojo.com/blog/wp-content/uploads/CD_MS_QA_Blog.png"},
            new ApplicationUser() { Name="Erika Ehrli", UserName =  "Erikae", PasswordHash = "Bikes360", ProfileImage = "https://media.licdn.com/mpr/mpr/shrinknp_200_200/AAEAAQAAAAAAAAKtAAAAJGIyYjRhM2NhLWQ2M2MtNGNjNi1iMjk2LWViZmMwNTdkOTFkZQ.jpg"},
            new ApplicationUser() { Name="Craig Kitterman", UserName =  "Craigk", PasswordHash = "Bikes360", ProfileImage = "https://pbs.twimg.com/profile_images/434160257/Craig_Kitterman_002_HIGH_RES.jpg"},
            new ApplicationUser() { Name="Stacie Doerr", UserName =  "Stacied", PasswordHash = "Bikes360", ProfileImage = "https://www.facebook.com/stacey.doerr.1"},
            new ApplicationUser() { Name="Pierce Boggan", UserName =  "Pierceb", PasswordHash = "Bikes360", ProfileImage = "https://www.bing.com/images/search?q=pierce+boggan&view=detailv2&&id=0B98571BA51EBD8C4DE0DF5B627C20078B28A320&selectedIndex=2&ccid=LKBJNTeA&simid=608031962887753707&thid=OIP.M2ca049353780bdc26a8d6497fba1804do1&ajaxhist=0"},
            new ApplicationUser() { Name="Jeremy Meng", UserName =  "Yumeng", PasswordHash = "Bikes360", ProfileImage = ""},
            new ApplicationUser() { Name="Xiang Yan", UserName =  "Xiangyan", PasswordHash = "Bikes360", ProfileImage = ""},
            new ApplicationUser() { Name="Mohammed Adenwala", UserName =  "Madenwal", PasswordHash = "Bikes360", ProfileImage = ""},
        };

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            Configuration = builder.Build();
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                var userManager = new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(context),
                        BuildIdentityOptions(),
                        new PasswordHasher<ApplicationUser>(),
                        new List<UserValidator<ApplicationUser>>() { new UserValidator<ApplicationUser>() },
                        new List<PasswordValidator<ApplicationUser>>() { new PasswordValidator<ApplicationUser>() },
                        new UpperInvariantLookupNormalizer(),
                        new Microsoft.AspNetCore.Identity.IdentityErrorDescriber(),
                        null,
                        null);

                foreach (var user in Users)
                {
                    var existingUser = await userManager.FindByNameAsync(user.Name);
                    if (existingUser == null)
                    {
                        await userManager.CreateAsync(user, user.PasswordHash);
                    }
                }
            }
        }

        private static IOptions<IdentityOptions> BuildIdentityOptions()
        {
            return Options.Create(new IdentityOptions
            {
                Password = new PasswordOptions
                {
                    RequireDigit = false,
                    RequiredLength = 0,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false,
                    RequireLowercase = false
                },
                Lockout = new LockoutOptions
                {
                    AllowedForNewUsers = false,
                },
                User = new UserOptions
                {
                    RequireUniqueEmail = false
                }
            });
        }
    }
}
