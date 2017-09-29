using Microsoft.EntityFrameworkCore;
using BikeSharing.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Profiles.Seed.Data
{
    static class ProfilesDbContextExtensions
    {
        private static ProfilesDbContext _db;

        public static void Seed(this ProfilesDbContext db)
        {
            _db = db;

            AddSampleTenants();
            AddSampleUsers();
            AddSamplePaymentData();
            AddSampleProfiles();
            AddSampleSubscriptions();

            db.Dispose();
        }

        private static void AddSampleTenants()
        {
            _db.Tenants.Add(new Tenant()
            {
                Name = "New York"
            });

            _db.SaveChanges();
        }

        private static void AddSampleUsers()
        {
            var ny = _db.Tenants.Single(t => t.Name == "New York");

            var users = new List<string> {
                "scottgu",
                "chrisd",
                "jamesm",
                "bethm",
                "donovanb",
                "larar",
                "kaseyu",
                "scottha",
                "migueld",
                "mariam",
                "erikae",
                "craigk",
                "staceyd",
                "pierceb",
                "mohammeda",
                "jeremym"
            };

            foreach (var username in users)
            {
                _db.Users.Add(new User()
                {
                    TenantId = ny.Id,
                    UserName = username,
                    LastLogin = DateTime.MinValue
                });
            }

            _db.SaveChanges();
        }

        private static void AddSamplePaymentData()
        {
            var users = _db.Users.Count();

            for (int i = 0; i < users; i++)
            {
                _db.PaymentData.Add(new PaymentData()
                {
                    ExpirationDate = new DateTime(2020, 10, 1),
                    CreditCardType = CreditCardType.Visa,
                    CreditCard = "4111111111111111"
                });
            }
            
            _db.SaveChanges();
        }

        private static void AddSampleProfiles()
        {
            var creditCards = _db.PaymentData.ToList();

            var users = _db.Users.ToList();

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Scott",
                LastName = "Guthrie",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "scottgu"),
                Payment = creditCards.First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Chris",
                LastName = "Dias",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "chrisd"),
                Payment = creditCards.Skip(1).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "James",
                LastName = "Montemagno",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "jamesm"),
                Payment = creditCards.Skip(2).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Beth",
                LastName = "Massi",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "bethm"),
                Payment = creditCards.Skip(3).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Donovan",
                LastName = "Brown",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "donovanb"),
                Payment = creditCards.Skip(4).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Lara",
                LastName = "Rubbelke",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "larar"),
                Payment = creditCards.Skip(5).First(),
                FaceProfileId = Guid.Parse("4f655429-63c0-4880-957e-8ef01c47bc6b"),
                VoiceProfileId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                VoiceSecretPhrase = "Be yourself everyone else is already taken"
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Kasey",
                LastName = "Uhlenhuth",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "kaseyu"),
                Payment = creditCards.Skip(6).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Scott",
                LastName = "Hanselman",
                Gender = Gender.Male,
                BirthDate = new DateTime(1980, 6, 22),
                User = users.Single(u => u.UserName == "scottha"),
                Payment = creditCards.Skip(7).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Miguel",
                LastName = "de Icaza",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "migueld"),
                Payment = creditCards.Skip(8).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Maria",
                LastName = "Naggaga",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "mariam"),
                Payment = creditCards.Skip(9).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Erika",
                LastName = "Ehrli",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "erikae"),
                Payment = creditCards.Skip(10).First(),
                FaceProfileId = Guid.Parse("a1bda1d8-1714-43b2-a46c-83e7427108d7"),
                VoiceProfileId = Guid.Parse("e3fff544-ef82-49d3-b80d-7b4c863c7ff7"),
                VoiceSecretPhrase = "Be yourself everyone else is already taken"
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Craig",
                LastName = "Kitterman",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "craigk"),
                Payment = creditCards.Skip(11).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Stacey",
                LastName = "Doerr",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Female,
                User = users.Single(u => u.UserName == "staceyd"),
                Payment = creditCards.Skip(12).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Pierce",
                LastName = "Boggan",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "pierceb"),
                Payment = creditCards.Skip(13).First()
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Mohammed",
                LastName = "Adenwala",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "mohammeda"),
                Payment = creditCards.Skip(14).First(),
                FaceProfileId = Guid.Parse("9db14edb-d2a1-4753-b273-7822a7cfb2af"),
                VoiceProfileId = Guid.Parse("ecb1bb57-1b8c-48c5-9437-4bdd1312d899"),
                VoiceSecretPhrase = "Apple juice tastes funny after toothpaste"
            });

            _db.Profiles.Add(new UserProfile()
            {
                FirstName = "Jeremy",
                LastName = "Meng",
                BirthDate = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                User = users.Single(u => u.UserName == "jeremym"),
                Payment = creditCards.Skip(15).First(),
                FaceProfileId = Guid.Parse("00346eba-ddfe-43a8-b236-d062e7719bf6"),
                VoiceProfileId = Guid.Parse("21c61186-725b-4920-885f-39a0c2aa44fd"),
                VoiceSecretPhrase = "Be yourself everyone else is already taken"
            });

            _db.SaveChanges();
        }

        private static void AddSampleSubscriptions()
        {
            var users = _db.Users;

            foreach (var user in users)
            {
                _db.Subscriptions.Add(new Subscription()
                {
                    ExpiresOn = DateTime.Now.AddYears(1),
                    Type = SubscriptionType.Member,
                    Status = SubscriptionStatus.Valid,
                    UserId = user.Id
                });
            }
            
            _db.SaveChanges();
        }
    }
}
